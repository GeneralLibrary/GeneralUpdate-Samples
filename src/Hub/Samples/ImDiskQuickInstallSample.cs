using System.Diagnostics;
using System.Runtime.InteropServices;
using GeneralUpdate.Drivelution;
using GeneralUpdate.Drivelution.Abstractions.Models;

namespace Hub.Samples;

/// <summary>
/// Scan the bundled ImDisk driver directory and install the driver.
///
/// Drivelution APIs: GetDriversFromDirectoryAsync (scan), ValidateAsync (verify)
/// Native install:    InstallHinfSection (setupapi.dll, same method ImDisk's own installer uses)
/// Verify:            pnputil /enum-drivers
/// </summary>
public class ImDiskQuickInstallSample : ISample
{
    public int Index => 8;
    public string Name => "Driver Update";
    public bool RequiresServer => false;
    public string[] CleanPaths => Array.Empty<string>();

    // Driver files bundled at src/ImDiskDriver/
    // BaseDir = src/Hub/bin/Debug/net10.0/  ->  ../../../../ImDiskDriver  =  src/ImDiskDriver/
    private static readonly string DriverDir = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ImDiskDriver"));

    // ---- Native SetupAPI (bypasses PnPUtil signature restrictions) ----
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool InstallHinfSection(IntPtr hwnd, IntPtr hinst, string cmdline, int show);

    [DllImport("newdev.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool DiInstallDriver(IntPtr hwnd, string infPath, uint flags, out bool needReboot);

    const uint DIIRFLAG_FORCE_INF = 0x00000001;

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        GeneralTracer.SetTracingEnabled(false);

        // ---- 1. Platform ----
        Console.WriteLine();
        Console.WriteLine("---- Step 1/4: Platform ----");

        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("  Windows only. Exiting.");
            return;
        }

        var platform = GeneralDrivelution.GetPlatformInfo();
        Console.WriteLine($"  OS:      {platform.OperatingSystem} ({platform.Architecture})");
        Console.WriteLine($"  Version: {platform.SystemVersion}");
        Console.WriteLine();

        // ---- 2. Scan (Drivelution API) ----
        Console.WriteLine("---- Step 2/4: Scan ----");
        Console.WriteLine($"  Directory: {DriverDir}");

        if (!Directory.Exists(DriverDir))
        {
            Console.WriteLine($"  ERROR: Directory not found: {DriverDir}");
            return;
        }

        var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(
            DriverDir, searchPattern: null, cancellationToken: ct);

        Console.WriteLine($"  Found: {drivers.Count} driver package(s)");

        if (drivers.Count == 0)
        {
            Console.WriteLine("  No INF driver found. INF files in directory:");
            foreach (var f in Directory.GetFiles(DriverDir, "*.inf", SearchOption.TopDirectoryOnly))
                Console.WriteLine($"    {f}");
            return;
        }

        var driver = drivers[0];
        Console.WriteLine($"  Name:    {driver.Name}");
        Console.WriteLine($"  Version: {driver.Version}");
        Console.WriteLine($"  File:    {driver.FilePath}");
        Console.WriteLine();

        // ---- 3. Validate (Drivelution API) ----
        Console.WriteLine("---- Step 3/4: Validate ----");

        var valid = await GeneralDrivelution.ValidateAsync(driver, ct);
        Console.WriteLine($"  Validate: {(valid ? "PASS" : "FAIL")}");
        Console.WriteLine();

        // ---- 4. Install (cascade: DIFx -> SetupAPI -> PnPUtil) ----
        Console.WriteLine("---- Step 4/4: Install ----");

        var infPath = driver.FilePath;
        var installOk = false;
        var method = "";

        // Method A: DiInstallDriver (newdev.dll)
        Console.WriteLine("  [1] DiInstallDriver (DIFx)...");
        installOk = DiInstallDriver(IntPtr.Zero, infPath, DIIRFLAG_FORCE_INF, out var needReboot);
        var err1 = installOk ? 0 : Marshal.GetLastWin32Error();
        Console.WriteLine(installOk
            ? $"  => OK{(needReboot ? " (reboot required)" : "")}"
            : $"  => Failed (0x{err1:X8})");
        method = "DiInstallDriver (DIFx)";

        // Method B: InstallHinfSection (setupapi.dll) — ImDisk's own installer uses this
        if (!installOk)
        {
            Console.WriteLine("  [2] InstallHinfSection (SetupAPI)...");
            installOk = InstallHinfSection(IntPtr.Zero, IntPtr.Zero, $"DefaultInstall 132 {infPath}", 0);
            var err2 = installOk ? 0 : Marshal.GetLastWin32Error();
            Console.WriteLine(installOk
                ? "  => OK"
                : $"  => Failed (0x{err2:X8})");
            method = "InstallHinfSection (SetupAPI)";
        }

        // Method C: PnPUtil (last resort)
        if (!installOk)
        {
            Console.WriteLine("  [3] PnPUtil (last resort)...");
            var (pnpOk, pnpOut) = await RunAsync("pnputil.exe",
                $"/add-driver \"{infPath}\" /install", ct);
            installOk = pnpOk;
            Console.WriteLine(installOk ? "  => OK" : "  => Failed");
            method = "PnPUtil";
        }

        // ---- Verify ----
        Console.WriteLine();
        var (_, enumOut) = await RunAsync("pnputil.exe", "/enum-drivers", ct);
        var inStore = enumOut.Contains(Path.GetFileName(infPath));
        Console.WriteLine($"  Driver store: {(inStore ? "REGISTERED" : "NOT FOUND")}");

        // ---- Summary ----
        Console.WriteLine();
        Console.WriteLine("============================================");
        Console.WriteLine("  RESULT");
        Console.WriteLine("============================================");
        Console.WriteLine($"  Install:  {(installOk ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"  Method:   {method}");
        Console.WriteLine($"  Driver:   {driver.Name} v{driver.Version}");
        Console.WriteLine($"  In store: {inStore}");

        if (!installOk)
        {
            Console.WriteLine();
            Console.WriteLine("  All install methods failed — signature enforcement.");
            Console.WriteLine("  In an ADMIN terminal, run:");
            Console.WriteLine("    bcdedit /set testsigning on");
            Console.WriteLine("  Then REBOOT and retry.");
        }

        Console.WriteLine();
    }

    static async Task<(bool ok, string output)> RunAsync(string exe, string args, CancellationToken ct)
    {
        try
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            if (p == null) return (false, "");
            var o = await p.StandardOutput.ReadToEndAsync(ct);
            var e = await p.StandardError.ReadToEndAsync(ct);
            await p.WaitForExitAsync(ct);
            return (p.ExitCode == 0, (o + e).Trim());
        }
        catch { return (false, ""); }
    }
}
