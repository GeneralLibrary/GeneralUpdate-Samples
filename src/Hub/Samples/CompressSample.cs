using System.Text;
using GeneralUpdate.Core.Compress;
using GeneralUpdate.Core.Configuration;

namespace Hub.Samples;

public class CompressSample : ISample
{
    public int Index => 10;
    public string Name => "压缩工具 — 压缩·解压·校验";
    public bool RequiresServer => false;
    public string[] CleanPaths => new[] { Path.Combine(Path.GetTempPath(), "compress_demo") };

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        var demoDir = Path.Combine(Path.GetTempPath(), "compress_demo", Guid.NewGuid().ToString("N")[..8]);
        var srcDir = Path.Combine(demoDir, "source");
        var outDir = Path.Combine(demoDir, "extracted");
        var zipPath = Path.Combine(demoDir, "test.zip");

        // ── 1. 准备 ──
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(outDir);
        Directory.CreateDirectory(Path.Combine(srcDir, "config"));

        var files = new Dictionary<string, string>
        {
            ["readme.txt"] = "[GeneralUpdate]\nVersion=1.0.0\nAuthor=JusterZhu",
            ["config/app.json"] = "{\"theme\":\"dark\",\"lang\":\"zh-CN\",\"cache\":true}",
            ["data/user.dat"] = "uid=1001\nname=demo\nrole=admin",
            ["logs/app.log"] = "[2024-01-01 00:00:00] App started\n[2024-01-01 00:01:00] OK"
        };

        Console.WriteLine();
        Console.WriteLine("══ 准备 — 创建测试文件 ══");
        foreach (var (name, content) in files)
        {
            var fp = Path.Combine(srcDir, name);
            Directory.CreateDirectory(Path.GetDirectoryName(fp)!);
            File.WriteAllText(fp, content);
            Console.WriteLine($"  {name} ({content.Length} chars)");
            Console.WriteLine($"    └─ {content.Split('\n')[0]}");
        }

        // ── 2. 压缩 ──
        Console.WriteLine();
        Console.Write("══ 压缩中... ");
        await Task.Run(() => CompressProvider.Compress(Format.Zip, srcDir, zipPath, false, Encoding.UTF8), ct);
        var zipSize = new FileInfo(zipPath).Length;
        var srcSize = Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
        Console.WriteLine($"{srcSize} → {zipSize} bytes ({(double)zipSize / srcSize * 100:F0}%) ✓");

        // ── 3. 解压 ──
        Console.Write("══ 解压中... ");
        await Task.Run(() => CompressProvider.Decompress(Format.Zip, zipPath, outDir, Encoding.UTF8), ct);
        Console.WriteLine("✓");

        // ── 4. 验证 ──
        Console.WriteLine();
        Console.WriteLine("══ 验证 — 逐文件比对 ══");
        var allOk = true;
        foreach (var (name, expected) in files)
        {
            var extracted = Path.Combine(outDir, name);
            var actual = File.Exists(extracted) ? File.ReadAllText(extracted) : "<<MISSING>>";
            var ok = actual == expected;
            if (!ok) allOk = false;
            Console.WriteLine($"  {(ok ? "✓" : "✗")} {name}");
            if (!ok) Console.WriteLine($"      期望: {expected}\n      实际: {actual}");
        }
        Console.WriteLine(allOk ? "\n  结果: 全部一致 ✓" : "\n  结果: 存在差异 ✗");

        SafeDelete(demoDir);
    }

    static void SafeDelete(string p) { try { if (Directory.Exists(p)) Directory.Delete(p, true); } catch { } }
}
