using System.Diagnostics;
using System.Runtime.InteropServices;
using GeneralUpdate.Bowl;
using Microsoft.Win32;

namespace Hub.Samples;

public class BowlSample : ISample
{
    public int Index => 7;
    public string Name => "进程守护 — 崩溃监控·Dump导出";
    public bool RequiresServer => false;
    public string[] CleanPaths => Array.Empty<string>();

    // MiniDumpWriteDump — 不需要 admin 就能给自己写 dump
    [DllImport("dbghelp.dll", SetLastError = true)]
    static extern bool MiniDumpWriteDump(
        IntPtr hProcess, int pid, SafeHandle hFile,
        uint dumpType, IntPtr expParam, IntPtr userParam, IntPtr callback);

    const uint MiniDumpNormal = 0x00000002;

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var version = "1.0.0.0";
        var failDir = Path.Combine(baseDir, "fail", version);
        var backupDir = Path.Combine(baseDir, "backup", version);

        // 清理
        KillAll("procdump64", "procdump", "powershell");
        await Task.Delay(300);
        SafeDelete(failDir);
        SafeDelete(backupDir);
        Directory.CreateDirectory(failDir);
        Directory.CreateDirectory(backupDir);

        // ── 1. Bowl API 配置 ──
        Console.WriteLine();
        Console.WriteLine("══ Bowl API 配置 ══");
        var context = new BowlContext
        {
            ProcessNameOrId = "MyApp.exe",
            DumpFileName = $"{version}_fail.dmp",
            FailFileName = $"{version}_fail.json",
            TargetPath = baseDir,
            FailDirectory = failDir,
            BackupDirectory = backupDir,
            WorkModel = "Upgrade",
            DumpType = DumpType.Mini,
            AutoRestore = true
        }.Normalize();

        Console.WriteLine($"  ProcessNameOrId = \"{context.ProcessNameOrId}\"");
        Console.WriteLine($"  DumpFileName    = \"{context.DumpFileName}\"");
        Console.WriteLine($"  FailFileName    = \"{context.FailFileName}\"");
        Console.WriteLine($"  WorkModel       = \"{context.WorkModel}\"");
        Console.WriteLine($"  DumpType        = {context.DumpType}");
        Console.WriteLine($"  AutoRestore     = {context.AutoRestore}");
        Console.WriteLine($"  FailDirectory   = \"{failDir}\"");
        Console.WriteLine($"  BackupDirectory = \"{backupDir}\"");

        Console.WriteLine();
        Console.WriteLine("══ Bowl 工作流程 ══");
        Console.WriteLine("  ① 升级完成 → Bowl.Launch(ctx)");
        Console.WriteLine("  ② Bowl 启动 procdump 附加到目标进程");
        Console.WriteLine("  ③ 进程崩溃 → MiniDump → fail/ 目录");
        Console.WriteLine("  ④ 生成 fail.json 故障报告");
        Console.WriteLine("  ⑤ AutoRestore=true → 从 backup/ 恢复上个版本");

        // ── 2. 模拟崩溃 — 用 MiniDumpWriteDump 直接生成 dump ──
        Console.WriteLine();
        Console.WriteLine("══ 模拟崩溃 (MiniDumpWriteDump API) ══");

        // 启动一个子进程让它崩溃
        Console.Write("  启动子进程... ");
        var child = Process.Start(new ProcessStartInfo("powershell.exe",
            "-NoProfile -Command \"[Environment]::FailFast('Bowl_Demo_Crash')\"")
        { UseShellExecute = false, CreateNoWindow = true });
        if (child == null) { Console.WriteLine("失败"); return; }
        int pid = child.Id;
        Console.WriteLine($"PID={pid} ✓");

        Console.Write("  等待 WER 生成 dump... ");
        // 等子进程崩溃 + WER 写 dump
        await child.WaitForExitAsync(ct);
        await Task.Delay(2000);

        // WER dump 在 %LOCALAPPDATA%\CrashDumps\
        var crashDumpsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CrashDumps");
        var werDump = Directory.Exists(crashDumpsDir)
            ? Directory.GetFiles(crashDumpsDir, "powershell*.dmp")
                .OrderByDescending(File.GetLastWriteTime).FirstOrDefault()
            : null;

        // 备用：用 MiniDumpWriteDump 自己写（写当前进程，能验证 API）
        var dumpPath = Path.Combine(failDir, context.DumpFileName);
        if (werDump != null)
        {
            File.Copy(werDump, dumpPath, true);
            Console.WriteLine($"WER 捕获 ✓ ({new FileInfo(dumpPath).Length} bytes)");
        }
        else
        {
            Console.WriteLine("WER 未触发，使用直接 API...");
            // 写当前进程的 mini dump
            using var fs = new FileStream(dumpPath, FileMode.Create);
            var currentProc = Process.GetCurrentProcess();
            var ok = MiniDumpWriteDump(currentProc.Handle, currentProc.Id,
                fs.SafeFileHandle, MiniDumpNormal, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            Console.WriteLine(ok ? $"MiniDumpWriteDump ✓ ({fs.Length} bytes)" : "失败 ✗");
        }

        // ── 3. 生成故障报告 ──
        Console.Write("  生成故障报告... ");
        var reportPath = Path.Combine(failDir, context.FailFileName);
        var crashTime = DateTime.Now;
        var report = $$"""
{
  "process": "MyApp.exe",
  "version": "{{version}}",
  "crashTime": "{{crashTime:O}}",
  "exitCode": -1073740791,
  "exception": "0xC0000409 (STATUS_STACK_BUFFER_OVERRUN / FailFast)",
  "dumpFile": "{{context.DumpFileName}}",
  "workModel": "{{context.WorkModel}}",
  "autoRestore": {{context.AutoRestore.ToString().ToLower()}},
  "restoredFrom": "{{backupDir.Replace("\\", "\\\\")}}"
}
""";
        await File.WriteAllTextAsync(reportPath, report, ct);
        Console.WriteLine("✓");

        // ── 4. 导出 Windows 系统日志 (Bowl 的 export.bat 流程) ──
        Console.Write("  导出系统日志... ");
        var logOk = false;
        try
        {
            // wevtutil — 导出最近24小时系统事件日志
            var evtLog = Path.Combine(failDir, "systemlog.evtx");
            var wevtPsi = new ProcessStartInfo("wevtutil.exe", $"epl System \"{evtLog}\" /q:\"*[System[TimeCreated[timediff(@SystemTime) <= 86400000]]]\"")
            { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            using var wp = Process.Start(wevtPsi)!;
            await wp.WaitForExitAsync(ct);
            logOk = wp.ExitCode == 0 && File.Exists(evtLog);
            if (logOk) Console.Write("evtx ✓, ");
        }
        catch { }

        // driverquery — 已安装驱动列表
        try
        {
            var drvFile = Path.Combine(failDir, "driverInfo.txt");
            var dqPsi = new ProcessStartInfo("driverquery.exe", "/v /fo table")
            { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var dp = Process.Start(dqPsi)!;
            var drvOut = await dp.StandardOutput.ReadToEndAsync(ct);
            await dp.WaitForExitAsync(ct);
            await File.WriteAllTextAsync(drvFile, drvOut, ct);
            Console.Write("drivers ✓, ");
        }
        catch { }

        // systeminfo — 系统信息
        try
        {
            var sysFile = Path.Combine(failDir, "systeminfo.txt");
            var siPsi = new ProcessStartInfo("systeminfo.exe")
            { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var sp = Process.Start(siPsi)!;
            var sysOut = await sp.StandardOutput.ReadToEndAsync(ct);
            await sp.WaitForExitAsync(ct);
            await File.WriteAllTextAsync(sysFile, sysOut, ct);
            Console.Write("systeminfo ✓");
        }
        catch { }

        Console.WriteLine();

        // ── 5/6. 备份恢复模拟 ──
        var restoreMarker = Path.Combine(backupDir, "RESTORED.txt");
        await File.WriteAllTextAsync(restoreMarker,
            $"App restored from backup at {crashTime:HH:mm:ss}\nVersion: {version}\nStatus: OK", ct);

        // ── 6. 结果 ──
        Console.WriteLine();
        Console.WriteLine("══ 结果 — 故障目录内容 ══");
        Console.WriteLine($"  {failDir}");

        var fileLabels = new Dictionary<string, string>
        {
            [".dmp"] = "进程 MiniDump (WinDbg 可分析崩溃堆栈)",
            [".json"] = "故障报告 (Bowl 生成的 crash 元数据)",
            [".evtx"] = "Windows 系统事件日志 (最近24h)",
            ["driverInfo.txt"] = "已安装驱动列表 (driverquery /v)",
            ["systeminfo.txt"] = "系统信息 (OS/硬件/补丁)",
            ["RESTORED.txt"] = "备份恢复标记 (AutoRestore)"
        };

        foreach (var f in Directory.GetFiles(failDir).OrderBy(x => x))
        {
            var fi = new FileInfo(f);
            var desc = fileLabels.FirstOrDefault(kv => f.EndsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Value ?? "";
            Console.WriteLine();
            Console.WriteLine($"  ▸ {fi.Name}  ({fi.Length:N0} bytes)");
            if (desc.Length > 0) Console.WriteLine($"    {desc}");

            if (f.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var line in (await File.ReadAllTextAsync(f)).Split('\n'))
                    Console.WriteLine($"    {line.TrimEnd()}");
            }

            if (f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && fi.Length < 2000)
            {
                // 小文本文件直接展示
                Console.WriteLine("    ── 内容预览 ──");
                foreach (var line in (await File.ReadAllLinesAsync(f)).Take(8))
                    Console.WriteLine($"    {line.TrimEnd()}");
                var totalLines = (await File.ReadAllLinesAsync(f)).Length;
                if (totalLines > 8) Console.WriteLine($"    ... (共 {totalLines} 行)");
            }
        }

        // 备份目录
        Console.WriteLine();
        Console.WriteLine($"  备份目录: {backupDir}");
        foreach (var f in Directory.GetFiles(backupDir).OrderBy(x => x))
        {
            var fi = new FileInfo(f);
            Console.WriteLine($"    ▸ {fi.Name} ({fi.Length} bytes)");
        }

        var allFiles = Directory.GetFiles(failDir);
        Console.WriteLine();
        Console.WriteLine("══ 总结 ══");
        Console.WriteLine($"  MiniDump:     {(allFiles.Any(f => f.EndsWith(".dmp")) ? "✓" : "✗")}");
        Console.WriteLine($"  故障报告:     {(allFiles.Any(f => f.EndsWith(".json")) ? "✓" : "✗")}");
        Console.WriteLine($"  系统事件日志: {(allFiles.Any(f => f.EndsWith(".evtx")) ? "✓" : "✗")}");
        Console.WriteLine($"  驱动列表:     {(allFiles.Any(f => f.EndsWith("driverInfo.txt")) ? "✓" : "✗")}");
        Console.WriteLine($"  系统信息:     {(allFiles.Any(f => f.EndsWith("systeminfo.txt")) ? "✓" : "✗")}");
        Console.WriteLine($"  文件总数:     {allFiles.Length}");
        Console.WriteLine("══ 完成 ══");
    }

    static void KillAll(params string[] names)
    {
        foreach (var n in names)
            try { foreach (var p in Process.GetProcessesByName(n))
            { try { p.Kill(); p.WaitForExit(1000); } catch { } p.Dispose(); } } catch { }
    }

    static void SafeDelete(string p)
    {
        if (!Directory.Exists(p)) return;
        try { Directory.Delete(p, true); } catch { }
    }
}
