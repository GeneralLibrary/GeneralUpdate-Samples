using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;

namespace Hub.Samples;

public class DrivelutionSample : ISample
{
    public int Index => 8;
    public string Name => "驱动更新 — 检测·验证·安装·卸载";
    public bool RequiresServer => false;
    public string[] CleanPaths => new[] { Path.Combine(Path.GetTempPath(), "drivelution_demo") };

    // P/Invoke to check actual install result
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool InstallHinfSection(IntPtr hwnd, IntPtr hinst, string cmdline, int show);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool SetupUninstallOEMInf(string infName, uint flags, IntPtr reserved);

    [DllImport("newdev.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool DiInstallDriver(IntPtr hwnd, string infPath, uint flags, out bool needReboot);

    [DllImport("newdev.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool DiUninstallDriver(IntPtr hwnd, string infPath, uint flags, out bool needReboot);

    const uint DIIRFLAG_FORCE_INF = 0x00000001;

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("  此示例仅支持 Windows 平台");
            return;
        }

        var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        Console.WriteLine();
        Console.WriteLine("══ 步骤 1/5 — 平台检测 ══");
        var platform = GeneralDrivelution.GetPlatformInfo();
        Console.WriteLine($"  OS:     {platform.OperatingSystem} ({platform.Architecture})");
        Console.WriteLine($"  管理员: {(isAdmin ? "是 ✓" : "否 ✗")}");

        // ── 2. 创建驱动包 ──
        var demoDir = Path.Combine(Path.GetTempPath(), "drivelution_demo", Guid.NewGuid().ToString("N")[..8]);
        var driverDir = Path.Combine(demoDir, "driver");
        Directory.CreateDirectory(driverDir);

        var infPath = Path.Combine(driverDir, "demo.inf");
        File.WriteAllText(infPath, """
[Version]
Signature   = "$Windows NT$"
Class       = SoftwareDevice
ClassGuid   = {62f9c741-b25b-46ef-b5cc-51f0ce52790b}
Provider    = %Mfg%
DriverVer   = 06/01/2024,1.0.0.0

[Manufacturer]
%Mfg% = Standard,NTamd64

[Standard.NTamd64]
%DeviceDesc% = DemoInstall, ROOT\DEMO0001

[DemoInstall]
AddReg = AddReg

[DemoInstall.Services]
AddService = ,,

[AddReg]
HKR,,DemoVersion,0x00000000,"1.0.0.0"

[Strings]
Mfg        = "GeneralUpdate Demo"
DeviceDesc = "GeneralUpdate Demo Device (Safe)"
""");

        Console.WriteLine();
        Console.WriteLine("══ 步骤 2/5 — 驱动包 ══");
        Console.WriteLine($"  文件: demo.inf ({new FileInfo(infPath).Length} bytes)");
        Console.WriteLine($"  类型: SoftwareDevice (无需 .sys 内核驱动)");
        Console.WriteLine($"  设备: ROOT\\DEMO0001");

        // ── 3. Drivelution API 验证 ──
        Console.WriteLine();
        Console.WriteLine("══ 步骤 3/5 — Drivelution API 验证 ══");
        var driverInfo = new DriverInfo
        {
            Name = "DemoDriver", Version = "1.0.0",
            FilePath = infPath,
            TargetOS = platform.OperatingSystem,
            Architecture = platform.Architecture
        };
        var valid = await GeneralDrivelution.ValidateAsync(driverInfo, ct);
        Console.WriteLine($"  ValidateAsync: {(valid ? "通过 ✓" : "未通过 ✗")}");

        // ── 4. 安装 ──
        Console.WriteLine();
        Console.WriteLine("══ 步骤 4/5 — 安装驱动 ══");

        if (!isAdmin)
        {
            Console.WriteLine("  ⚠ 需要管理员权限 → 右键终端 → 以管理员身份运行");
            SafeDelete(demoDir);
            return;
        }

        // 检查驱动签名策略
        var signingEnforced = CheckSigningEnforcement();
        Console.WriteLine($"  驱动签名强制: {(signingEnforced ? "是 (Secure Boot / Win11 默认)" : "否 (测试模式)")}");

        // 备份
        var backupFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "driver_backups", "demo.inf.bak");
        Directory.CreateDirectory(Path.GetDirectoryName(backupFile)!);
        File.Copy(infPath, backupFile, true);
        Console.WriteLine($"  备份: ✓");

        // 用 DiInstallDriver (newdev.dll) — Windows DIFx 框架
        // 即使未签名，也会尝试安装并返回准确结果
        Console.Write("  安装 (DiInstallDriver)... ");
        bool needReboot;
        var installOk = DiInstallDriver(IntPtr.Zero, infPath, DIIRFLAG_FORCE_INF, out needReboot);
        var err = installOk ? 0 : Marshal.GetLastWin32Error();

        if (installOk)
        {
            Console.WriteLine("成功 ✓");
        }
        else
        {
            Console.WriteLine("失败 ✗");
            Console.WriteLine($"  错误码: {err}");
            Console.WriteLine(GetSigningErrorMessage(err));
        }

        // ── 5. 验证 & 卸载 ──
        Console.WriteLine();
        Console.WriteLine("══ 步骤 5/5 — 验证 & 卸载 ══");

        // 检查是否真的装入了驱动存储
        var (enumOk, enumOut) = await RunPnPAsync("/enum-drivers", ct);
        var inStore = enumOut.Contains("demo.inf");
        Console.WriteLine($"  驱动存储: {(inStore ? "已注册 ✓" : "未注册 (签名拦截)")}");

        // 如果在存储中，卸载
        var uninstOk = false;
        if (inStore)
        {
            Console.Write("  卸载驱动... ");
            uninstOk = SetupUninstallOEMInf("demo.inf", 1 /* SUOI_FORCEDELETE */, IntPtr.Zero);
            Console.WriteLine(uninstOk ? "SetupUninstallOEMInf ✓" : "✗");

            if (!uninstOk)
            {
                bool rb;
                uninstOk = DiUninstallDriver(IntPtr.Zero, infPath, 0, out rb);
                Console.WriteLine($"  DiUninstallDriver: {(uninstOk ? "✓" : "✗")}");
            }
        }

        // ── 总结 ──
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════");
        Console.WriteLine("  安装流程总结");
        Console.WriteLine("═══════════════════════════════");
        R("平台检测", true);
        R("Drivelution 验证", valid);
        R("备份文件", true);
        R($"DiInstallDriver{(signingEnforced ? " (签名拦截)" : "")}", installOk);
        R("驱动存储", inStore);
        R("卸载", uninstOk || !inStore);

        if (installOk)
        {
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════╗");
            Console.WriteLine("  ║  ✓ 安装/卸载 成功        ║");
            Console.WriteLine("  ╚══════════════════════════╝");
        }
        else if (signingEnforced)
        {
            Console.WriteLine();
            Console.WriteLine("  ┌─────────────────────────────────────┐");
            Console.WriteLine("  │ Windows 驱动签名强制已开启           │");
            Console.WriteLine("  │                                     │");
            Console.WriteLine("  │ Drivelution API 流程验证: ✓          │");
            Console.WriteLine("  │ 实际安装: ✗ (签名拦截)               │");
            Console.WriteLine("  │                                     │");
            Console.WriteLine("  │ 要测试完整安装, 以管理员运行:         │");
            Console.WriteLine("  │   bcdedit /set testsigning on       │");
            Console.WriteLine("  │   重启后即可安装未签名驱动            │");
            Console.WriteLine("  └─────────────────────────────────────┘");
        }
        else
        {
            Console.WriteLine($"  ⚠ 安装失败, 错误码: {err}");
        }

        SafeDelete(demoDir);
    }

    static bool CheckSigningEnforcement()
    {
        try
        {
            var psi = new ProcessStartInfo("bcdedit.exe", "/enum")
            { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var p = Process.Start(psi);
            if (p == null) return true; // can't check, assume enforced
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            // testsigning Yes → disabled enforcement
            return !output.Contains("testsigning") || !output.Contains("Yes");
        }
        catch { return true; }
    }

    static string GetSigningErrorMessage(int err)
    {
        // Common driver install error codes
        if (err == 5)  return "  → 拒绝访问 (需要管理员权限)";
        if (err == unchecked((int)0xE0000247)) return "  → 驱动未签名, Windows 强制签名策略拦截";
        if (err == unchecked((int)0xE000022F)) return "  → 第三方 INF 未包含数字签名";
        return $"  → 错误码 {err}: 常见原因=驱动签名强制 (Win11 默认)";
    }

    static async Task<(bool ok, string output)> RunPnPAsync(string args, CancellationToken ct)
    {
        try
        {
            var psi = new ProcessStartInfo("pnputil.exe", args)
            { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            using var p = Process.Start(psi);
            if (p == null) return (false, "");
            var o = await p.StandardOutput.ReadToEndAsync(ct);
            var e = await p.StandardError.ReadToEndAsync(ct);
            await p.WaitForExitAsync(ct);
            var text = (o + e).Trim();
            return (p.ExitCode == 0, string.IsNullOrEmpty(text) ? "" : text.Split('\n').Last().Trim());
        }
        catch { return (false, ""); }
    }

    static void R(string label, bool ok) => Console.WriteLine($"  {(ok ? "✓" : "✗")} {label}");
    static void SafeDelete(string p) { try { if (Directory.Exists(p)) Directory.Delete(p, true); } catch { } }
}
