using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Hub.Samples;

namespace Hub;

class Program
{
    private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string SrcDir = Path.GetFullPath(Path.Combine(BaseDir, "..", "..", "..", ".."));
    private static readonly string ServerDir = Path.Combine(SrcDir, "Server");
    private static readonly string ServerProj = Path.Combine(ServerDir, "ServerSample.csproj");
    private static readonly string PackagesDir = Path.Combine(ServerDir, "wwwroot", "packages");

    private static Process? _serverProcess;
    private static List<ISample> _samples = new();
    private static AppConfig _config = new();

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine();
        Console.WriteLine("  ╔══════════════════════════════════════╗");
        Console.WriteLine("  ║    GeneralUpdate 示例浏览器          ║");
        Console.WriteLine("  ╚══════════════════════════════════════╝");
        Console.WriteLine();

        // 加载配置
        _config = LoadConfig();
        Console.WriteLine($"  配置: {_config.ServerUrl}");
        Console.WriteLine();

        // 发现所有 Sample
        _samples = DiscoverSamples();
        if (_samples.Count == 0)
        {
            Console.WriteLine("  [错误] 未发现任何示例");
            return;
        }

        // 确保 Server 已构建
        await EnsureServerBuiltAsync();

        // 确保更新包存在
        EnsurePackagesExist();

        // 主循环
        while (true)
        {
            PrintMenu();
            Console.Write(">> ");
            var line = Console.ReadLine()?.Trim();
            Console.WriteLine();

            // EOF (pipe closed) — exit gracefully
            if (line == null)
            {
                Console.WriteLine("  再见！");
                break;
            }

            if (string.IsNullOrEmpty(line)) continue;

            if (line == "0" || line.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("  再见！");
                break;
            }

            if (!int.TryParse(line, out var index) || index < 1 || index > _samples.Count)
            {
                Console.WriteLine("  无效选项，请重新选择");
                continue;
            }

            await RunSampleAsync(_samples[index - 1]);
        }
    }

    // ══════════════════════════════════════════════════════
    // 菜单
    // ══════════════════════════════════════════════════════

    static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("  ═══════════════════════════════════");
        for (int i = 0; i < _samples.Count; i++)
        {
            var s = _samples[i];
            Console.WriteLine($"    {s.Index}. {s.Name}");
        }
        Console.WriteLine("  ───────────────────────────────────");
        Console.WriteLine($"    0. 退出 (Exit)");
        Console.WriteLine("  ═══════════════════════════════════");
    }

    // ══════════════════════════════════════════════════════
    // 执行示例
    // ══════════════════════════════════════════════════════

    static async Task RunSampleAsync(ISample sample)
    {
        // 1. Clean (带超时, 绝不会卡死)
        foreach (var path in sample.CleanPaths)
        {
            var resolved = ResolvePath(path);
            if (!Directory.Exists(resolved)) continue;

            var done = await Task.WhenAny(
                Task.Run(() => { try { Directory.Delete(resolved, true); } catch { } }),
                Task.Delay(3000));
        }

        // 2. Start Server if needed
        if (sample.RequiresServer)
        {
            Console.WriteLine();
            var started = await StartServerAsync();
            if (!started)
            {
                Console.WriteLine("  [Server] 启动失败，无法运行需要 Server 的示例");
                return;
            }
        }

        // 3. Run
        Console.WriteLine();
        var cts = new CancellationTokenSource();
        try
        {
            await sample.RunAsync(_config, cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"  [错误] {ex.Message}");
        }
        finally
        {
            cts.Dispose();
        }

        // 4. Stop Server
        if (sample.RequiresServer)
        {
            Console.WriteLine();
            await StopServerAsync();
        }

        Console.WriteLine();
        Console.Write("  按任意键返回菜单 ────────");
        try { Console.ReadKey(intercept: true); }
        catch (InvalidOperationException) { Console.ReadLine(); }
        Console.WriteLine();
    }

    // ══════════════════════════════════════════════════════
    // Server 生命周期
    // ══════════════════════════════════════════════════════

    static async Task EnsureServerBuiltAsync()
    {
        if (!File.Exists(ServerProj))
        {
            Console.WriteLine("  [Server] 项目未找到，跳过构建");
            return;
        }

        // 检查是否已构建
        var serverExe = FindServerExe();
        if (serverExe != null) return;

        Console.Write("  [Server] 首次运行，正在构建... ");
        var psi = new ProcessStartInfo("dotnet", $"build \"{ServerProj}\" -c Release")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        var proc = Process.Start(psi)!;
        await proc.WaitForExitAsync();
        Console.WriteLine(proc.ExitCode == 0 ? "✓" : "✗");
    }

    static async Task<bool> StartServerAsync()
    {
        if (_serverProcess is { HasExited: false })
        {
            Console.WriteLine("  [Server] 已在运行");
            return true;
        }

        var exePath = FindServerExe();
        if (exePath == null)
        {
            Console.WriteLine("  [Server] 未找到 ServerSample.exe，请先构建");
            return false;
        }

        Console.Write("  [Server] 启动中... ");

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo(exePath)
            {
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? ServerDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _serverProcess.Start();

        // 轮询等待就绪
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(500);
            try
            {
                var resp = await client.GetAsync(_config.ServerUrl);
                if (resp.IsSuccessStatusCode || (int)resp.StatusCode >= 400)
                {
                    Console.WriteLine($"✓ (localhost:5000, {i * 500}ms)");
                    return true;
                }
            }
            catch { /* not ready yet */ }
        }

        Console.WriteLine("✗ (超时)");
        return false;
    }

    static async Task StopServerAsync()
    {
        if (_serverProcess is { HasExited: false })
        {
            Console.Write("  [Server] 关闭中... ");
            _serverProcess.Kill(entireProcessTree: true);
            await _serverProcess.WaitForExitAsync();
            _serverProcess.Dispose();
            _serverProcess = null;
            Console.WriteLine("✓");
        }
    }

    static string? FindServerExe()
    {
        // 检查构建输出目录
        var candidates = new[]
        {
            Path.Combine(ServerDir, "bin", "Release", "net10.0", "ServerSample.exe"),
            Path.Combine(ServerDir, "bin", "Release", "net10.0", "ServerSample"),
            Path.Combine(ServerDir, "bin", "Release", "net9.0", "ServerSample.exe"),
            Path.Combine(ServerDir, "bin", "Release", "net8.0", "ServerSample.exe")
        };
        foreach (var c in candidates)
        {
            if (File.Exists(c)) return c;
        }

        // 搜索
        var searchDir = Path.Combine(ServerDir, "bin", "Release");
        if (Directory.Exists(searchDir))
        {
            var found = Directory.GetFiles(searchDir, "ServerSample.exe", SearchOption.AllDirectories);
            if (found.Length > 0) return found[0];
        }
        return null;
    }

    // ══════════════════════════════════════════════════════
    // 更新包保障
    // ══════════════════════════════════════════════════════

    static void EnsurePackagesExist()
    {
        // 确保 packages 目录下有数据
        if (!Directory.Exists(PackagesDir))
            Directory.CreateDirectory(PackagesDir);

        var versionsJson = Path.Combine(PackagesDir, "versions.json");
        if (!File.Exists(versionsJson))
        {
            Console.WriteLine("  [提示] 未找到更新包，请运行 gen_packages.ps1 生成测试包");
            Console.WriteLine();
        }
    }

    // ══════════════════════════════════════════════════════
    // 工具方法
    // ══════════════════════════════════════════════════════

    static AppConfig LoadConfig()
    {
        var configPath = Path.Combine(BaseDir, "appsettings.json");
        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch { }
        }
        Console.WriteLine("  [警告] 未找到 appsettings.json，使用默认配置");
        return new AppConfig();
    }

    static List<ISample> DiscoverSamples()
    {
        var samples = new List<ISample>();
        var iface = typeof(ISample);

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (iface.IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
            {
                if (Activator.CreateInstance(type) is ISample sample)
                    samples.Add(sample);
            }
        }

        return samples.OrderBy(s => s.Index).ToList();
    }

    static string ResolvePath(string path)
    {
        return path
            .Replace("{BaseDir}", BaseDir)
            .Replace("%TEMP%", Path.GetTempPath());
    }
}
