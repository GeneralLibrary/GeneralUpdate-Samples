using System.IO.Compression;
using GeneralUpdate.Extension.Common.DTOs;
using GeneralUpdate.Extension.Common.Enums;
using GeneralUpdate.Extension.Common.Models;
using GeneralUpdate.Extension.Core;
using Newtonsoft.Json;

namespace Hub.Samples;

public class ExtensionSample : ISample
{
    public int Index => 7;
    public string Name => "插件系统 — 安装·管理·兼容性";
    public bool RequiresServer => false;
    public string[] CleanPaths => new[] { "{BaseDir}/extensions" };

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var extDir = Path.Combine(baseDir, "extensions");
        Directory.CreateDirectory(extDir);

        // ── 1. 准备 — 创建模拟扩展包 ──
        var meta = new ExtensionMetadata
        {
            Id = Guid.NewGuid().ToString(),
            Name = "hello-world",
            DisplayName = "Hello World 插件",
            Version = "1.0.0",
            Description = "一个演示插件 — 展示 Extension 系统能力",
            Publisher = "GeneralUpdate Team",
            Format = ".zip",
            Status = true,
            SupportedPlatforms = TargetPlatform.All,
            MinHostVersion = "1.0.0",
            MaxHostVersion = "2.0.0",
            Categories = "Tool,Demo",
            License = "MIT"
        };

        var zipPath = Path.Combine(extDir, $"{meta.Name}_{meta.Version}.zip");
        await CreatePackageAsync(zipPath, meta);

        Console.WriteLine();
        Console.WriteLine("══ 准备 — 模拟扩展包 ══");
        Console.WriteLine($"  名称: {meta.DisplayName}");
        Console.WriteLine($"  版本: v{meta.Version}");
        Console.WriteLine($"  兼容: 主机 v{meta.MinHostVersion} ~ v{meta.MaxHostVersion}");
        Console.WriteLine($"  平台: {meta.SupportedPlatforms}");
        Console.WriteLine($"  文件: {Path.GetFileName(zipPath)} ({new FileInfo(zipPath).Length} bytes)");
        Console.WriteLine($"  内容: extension.json, extension.dll, README.md");

        // ── 2. 初始化主机 ──
        var options = new ExtensionHostOptions
        {
            ServerUrl = $"{config.ServerUrl}/Extension",
            HostVersion = "1.0.0",
            ExtensionsDirectory = extDir,
            CatalogPath = Path.Combine(extDir, "catalog.json")
        };
        var host = new GeneralExtensionHost(options);

        Console.WriteLine();
        Console.WriteLine("══ 安装 ══");
        Console.Write("  正在安装... ");

        host.ExtensionUpdateStatusChanged += (_, e) =>
            Console.WriteLine($"\n  → 事件: {e.ExtensionName} 状态={e.Status}");

        var installed = await host.InstallExtensionAsync(zipPath, rollbackOnFailure: true);
        Console.WriteLine(installed ? "成功 ✓" : "失败 ✗");

        // ── 3. 验证 — 列出安装的文件 ──
        var extractedDir = Path.Combine(extDir, meta.Name);
        Console.WriteLine();
        Console.WriteLine("══ 验证 — 安装后的文件 ══");
        if (Directory.Exists(extractedDir))
        {
            foreach (var f in Directory.GetFiles(extractedDir))
            {
                var fi = new FileInfo(f);
                Console.WriteLine($"  {fi.Name} ({fi.Length} bytes)");
            }
        }

        // ── 4. 目录管理 ──
        host.ExtensionCatalog.LoadInstalledExtensions();
        var list = host.ExtensionCatalog.GetInstalledExtensions();
        Console.WriteLine();
        Console.WriteLine("══ 已安装扩展列表 ══");
        foreach (var ext in list)
        {
            var compat = host.IsExtensionCompatible(ext);
            Console.WriteLine($"  • {ext.DisplayName} v{ext.Version}");
            Console.WriteLine($"    兼容: {(compat ? "✓" : "✗")}  状态: {(ext.Status == true ? "启用" : "禁用")}");
        }

        // ── 5. 自动更新 ──
        host.SetAutoUpdate(meta.Id, true);
        Console.WriteLine();
        Console.WriteLine($"  自动更新: {(host.IsAutoUpdateEnabled(meta.Id) ? "已启用 ✓" : "已禁用")}");

        Console.WriteLine();
        Console.WriteLine("══ 完成 ✓ ══");
    }

    static async Task CreatePackageAsync(string zipPath, ExtensionMetadata meta)
    {
        await Task.Run(() =>
        {
            var tmp = Path.Combine(Path.GetTempPath(), $"ext-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tmp);
            try
            {
                File.WriteAllText(Path.Combine(tmp, "extension.json"),
                    JsonConvert.SerializeObject(meta, Formatting.Indented));
                File.WriteAllText(Path.Combine(tmp, "extension.dll"),
                    $"// {meta.Name} v{meta.Version} — 模拟插件入口\npublic class Entry {{ public void Init() {{ /* 插件初始化 */ }} }}");
                File.WriteAllText(Path.Combine(tmp, "README.md"),
                    $"# {meta.DisplayName}\n\n{meta.Description}\n\n- Version: {meta.Version}\n- Publisher: {meta.Publisher}");

                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(tmp, zipPath);
            }
            finally { if (Directory.Exists(tmp)) Directory.Delete(tmp, true); }
        });
    }
}
