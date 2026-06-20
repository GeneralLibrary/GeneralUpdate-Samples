using GeneralUpdate.Core.Differential;
using GeneralUpdate.Core.Models;
using GeneralUpdate.Core.Pipeline;
using GeneralUpdate.Differential.Differ;

namespace Hub.Samples;

public class DifferentialSample : ISample
{
    public int Index => 5;
    public string Name => "二进制差分 — 生成·应用·校验";
    public bool RequiresServer => false;
    public string[] CleanPaths => new[] {
        Path.Combine(Path.GetTempPath(), "gu_patch"),
        Path.Combine(Path.GetTempPath(), "gu_verify")
    };

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var srcDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var v1Dir = Path.Combine(srcDir, "content_client", "v1.0.0.0");
        var v2Dir = Path.Combine(srcDir, "content_client", "v2.0.0.0");
        var patchDir = Path.Combine(Path.GetTempPath(), "gu_patch", Guid.NewGuid().ToString("N"));
        var verifyDir = Path.Combine(Path.GetTempPath(), "gu_verify", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(patchDir);

        if (!Directory.Exists(v1Dir) || !Directory.Exists(v2Dir))
        {
            Console.WriteLine("[错误] 缺少测试数据 content_client/v1.0.0.0 和 v2.0.0.0");
            return;
        }

        // ── 准备 — 展示新旧版本差异 ──
        var v1Files = ListFiles(v1Dir);
        var v2Files = ListFiles(v2Dir);
        var added = v2Files.Keys.Except(v1Files.Keys).ToList();
        var removed = v1Files.Keys.Except(v2Files.Keys).ToList();
        var common = v1Files.Keys.Intersect(v2Files.Keys).ToList();
        var changed = common.Where(f => v1Files[f] != v2Files[f]).ToList();

        Console.WriteLine();
        Console.WriteLine("══ 版本差异分析 ══");
        Console.WriteLine($"  v1.0.0.0 ({v1Files.Count} 文件)  →  v2.0.0.0 ({v2Files.Count} 文件)");
        foreach (var f in added)   Console.WriteLine($"  + 新增: {f}");
        foreach (var f in removed) Console.WriteLine($"  - 删除: {f}");
        foreach (var f in changed) Console.WriteLine($"  ~ 修改: {f}");
        Console.WriteLine();

        // ── Clean — 生成补丁 ──
        Console.Write("══ Clean — 生成差分补丁... ");
        var pipeline = new DiffPipelineBuilder()
            .UseDiffer(new BsdiffDiffer())
            .UseCleanMatcher(new DefaultCleanMatcher())
            .UseDirtyMatcher(new DefaultDirtyMatcher())
            .WithParallelism(2)
            .WithProgress(new Progress<DiffProgress>(p =>
            {
                if (!string.IsNullOrEmpty(p.CurrentFile))
                    Console.Write($"\n    → {p.CurrentFile} ({p.Completed}/{p.Total})");
            }))
            .Build();

        await pipeline.CleanAsync(v1Dir, v2Dir, patchDir, cancellationToken: ct);

        var patchFiles = Directory.GetFiles(patchDir, "*", SearchOption.AllDirectories);
        var patchSize = patchFiles.Sum(f => new FileInfo(f).Length);
        var fullSize = v2Files.Values.Sum(c => c.Length);
        Console.WriteLine($"\n  生成 {patchFiles.Length} 个补丁文件, {patchSize} bytes ✓");

        // ── Dirty — 应用补丁 ──
        Console.WriteLine();
        Console.Write("══ Dirty — 将补丁应用到 v1... ");
        CopyDir(v1Dir, verifyDir);
        await pipeline.DirtyAsync(verifyDir, patchDir, cancellationToken: ct);

        var resultFiles = ListFiles(verifyDir);
        Console.WriteLine($"还原出 {resultFiles.Count} 个文件 ✓");

        // ── 验证 — 逐文件比对 ──
        Console.WriteLine();
        Console.WriteLine("══ 验证 — v2(期望) vs Dirty(实际) ══");
        var allOk = true;
        foreach (var (name, expected) in v2Files.OrderBy(x => x.Key))
        {
            var actual = resultFiles.TryGetValue(name, out var a) ? a : "<<MISSING>>";
            var ok = actual == expected;
            if (!ok) allOk = false;
            Console.WriteLine($"  {(ok ? "✓" : "✗")} {name}");
        }

        foreach (var removedFile in removed)
        {
            if (resultFiles.ContainsKey(removedFile))
            {
                Console.WriteLine($"  ✗ {removedFile} (应删除但仍存在)");
                allOk = false;
            }
        }

        Console.WriteLine(allOk ? "\n  结果: 全部一致 ✓" : "\n  结果: 存在差异 ✗");

        var savings = fullSize > 0 ? (1.0 - (double)patchSize / fullSize) * 100 : 0;
        Console.WriteLine($"  差分比: {patchSize} / {fullSize} bytes = {(savings > 0 ? $"节省 {savings:F0}%" : "补丁开销")}");

        SafeDelete(patchDir);
        SafeDelete(verifyDir);
    }

    static Dictionary<string, string> ListFiles(string dir) =>
        Directory.Exists(dir)
            ? Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
                .ToDictionary(f => Path.GetRelativePath(dir, f), File.ReadAllText)
            : new();

    static void CopyDir(string src, string dst)
    {
        foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(src, f);
            var dest = Path.Combine(dst, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(f, dest, true);
        }
    }

    static void SafeDelete(string p) { try { if (Directory.Exists(p)) Directory.Delete(p, true); } catch { } }
}
