using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using System.Web;
using ServerSample.DTOs;

// ================================================================
// GeneralUpdate Sample Server — Minimal API for update delivery
//
// Endpoints:
//   POST /Upgrade/Verification  — version validation & update discovery
//   POST /Upgrade/Report        — update status reporting
//   GET  /File/Download/{hash}  — package download with Range support
//
// The server reads package metadata from wwwroot/packages/versions.json
// and serves the corresponding .zip files.
// ================================================================

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration.GetValue<string>("Urls") ?? "http://0.0.0.0:5000");

var app = builder.Build();

var versionStore = LoadVersionStore(app.Environment);
var reportStore = new ConcurrentDictionary<int, ReportDTO>();
var recordIdCounter = 0;

var contentRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(contentRoot))
    Directory.CreateDirectory(contentRoot);

// ──  Request logging  ────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    var start = DateTime.UtcNow;
    Console.WriteLine($"[{start:O}] {ctx.Request.Method} {ctx.Request.Path}{ctx.Request.QueryString}");
    await next();
    Console.WriteLine($"[{DateTime.UtcNow:O}] {ctx.Request.Method} {ctx.Request.Path} -> {ctx.Response.StatusCode} ({(DateTime.UtcNow - start).TotalMilliseconds:F0}ms)");
});

// ──  Verification handler (shared across URL variants)  ──────────
var handleVerification = (VerifyDTO request) =>
{
    Console.WriteLine($"[Verification] AppKey={request.AppKey}, Version={request.Version}, AppType={request.AppType}, Platform={request.Platform}, ProductId={request.ProductId}, UpgradeMode={request.UpgradeMode}");

    if (!Version.TryParse(request.Version ?? "0.0.0.0", out var currentVersion))
        return Results.Ok(HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(
            Array.Empty<VerificationResultDTO>(), "Invalid version format."));

    var requestUpgradeMode = request.UpgradeMode ?? 1;

    var available = versionStore
        .Where(v =>
        {
            if (!Version.TryParse(v.Version, out var storeVersion)) return false;
            if (storeVersion <= currentVersion) return false;
            if (request.AppType.HasValue && v.AppType != request.AppType) return false;
            if (request.Platform.HasValue && v.Platform != request.Platform) return false;
            if (!string.IsNullOrEmpty(request.ProductId) && !string.IsNullOrEmpty(v.ProductId) &&
                !string.Equals(v.ProductId, request.ProductId, StringComparison.OrdinalIgnoreCase))
                return false;
            if (requestUpgradeMode == 1 && v.IsCrossVersion == true) return false;
            if (requestUpgradeMode == 2)
            {
                if (v.IsCrossVersion != true) return false;
                if (!string.Equals(v.FromVersion, request.Version, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        })
        .OrderByDescending(v => new Version(v.Version!))
        .ThenBy(v => v.IsCrossVersion == true ? 1 : 0)
        .ToList();

    if (available.Count == 0)
        return Results.Ok(HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(
            Array.Empty<VerificationResultDTO>(), "Already up to date."));

    var results = new List<VerificationResultDTO>();
    foreach (var v in available)
    {
        var recordId = Interlocked.Increment(ref recordIdCounter);
        var downloadUrl = $"{GetBaseUrl(app)}File/Download/{HttpUtility.UrlEncode(v.Hash!)}";
        results.Add(new VerificationResultDTO
        {
            RecordId = recordId, Name = v.PacketName, Hash = v.Hash,
            ReleaseDate = v.PubTime, Url = downloadUrl,
            UrlExpireTimeUtc = DateTime.UtcNow.AddHours(24),
            Version = v.Version, AppType = v.AppType, Platform = v.Platform,
            ProductId = v.ProductId, IsForcibly = v.IsForcibly,
            Format = v.Format ?? ".zip", Size = v.Size, IsFreeze = v.IsFreeze,
            UpgradeMode = requestUpgradeMode,
            IsCrossVersion = v.IsCrossVersion ?? false,
            FromVersion = v.FromVersion
        });
    }

    var modeLabel = requestUpgradeMode == 2 ? "CrossVersion" : "VersionChain";
    Console.WriteLine($"[Verification] Returning {results.Count} packages (Mode: {modeLabel})");
    foreach (var r in results)
        Console.WriteLine($"    {r.Version} — {r.Name} ({(r.IsCrossVersion == true ? $"Cross {r.FromVersion}" : "Full")})");

    return Results.Ok(HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(results,
        $"Found {results.Count} update(s)."));
};

// Register both URL variants (different clients use different paths)
app.MapPost("/Upgrade/Verification", handleVerification);
app.MapPost("/Update/Verification", handleVerification);

// ──  Report handler (shared across URL variants)  ─────────────────
var handleReport = (ReportDTO request) =>
{
    reportStore[request.RecordId] = request;
    var statusText = request.Status switch { 1 => "updating", 2 => "success", 3 => "failed", _ => "unknown" };
    var typeText = request.Type switch { 1 => "upgrade", 2 => "push", _ => "unknown" };
    Console.WriteLine($"[Report] Record {request.RecordId} -> {statusText} (type={typeText})");
    return Results.Ok(HttpResponseDTO<bool>.Success(true, $"Report accepted: {statusText}."));
};

app.MapPost("/Update/Report", handleReport);
app.MapPost("/Upgrade/Report", handleReport);

// ──  GET /File/Download/{hash}  ───────────────────────────────────
// Serves package files by their SHA256 hash. Supports HTTP Range
// requests for download resume.
app.MapGet("/File/Download/{hash}", (string hash, HttpContext http) =>
{
    var entry = versionStore.FirstOrDefault(v =>
        string.Equals(v.Hash, hash, StringComparison.OrdinalIgnoreCase));

    var fileName = entry != null ? $"{entry.PacketName}.zip" : $"{hash}.zip";
    var filePath = Path.Combine(contentRoot, "packages", fileName);

    if (!File.Exists(filePath))
    {
        // Try finding by hash as filename
        var candidates = Directory.GetFiles(Path.Combine(contentRoot, "packages"), "*.zip");
        var matched = candidates.FirstOrDefault(f =>
        {
            using var sha = SHA256.Create();
            using var fs = File.OpenRead(f);
            var computed = Convert.ToHexStringLower(sha.ComputeHash(fs));
            return string.Equals(computed, hash, StringComparison.OrdinalIgnoreCase);
        });
        if (matched != null) filePath = matched;
        else
            return Results.NotFound(new { Code = 404, Message = $"File not found for hash: {hash}" });
    }

    var fileInfo = new FileInfo(filePath);
    Console.WriteLine($"[Download] {hash}: {fileInfo.Name} ({fileInfo.Length} bytes)");
    return Results.File(filePath, contentType: "application/octet-stream",
        enableRangeProcessing: true, fileDownloadName: Path.GetFileName(filePath));
});

// ──  Static files fallback  ──────────────────────────────────────
app.UseStaticFiles();

// ──  Startup banner  ─────────────────────────────────────────────
Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║     GeneralUpdate Sample Upgrade Server          ║");
Console.WriteLine("╠══════════════════════════════════════════════════╣");
Console.WriteLine($"║  Verification: {GetBaseUrl(app)}Upgrade/Verification".PadRight(51) + "║");
Console.WriteLine($"║  Report:       {GetBaseUrl(app)}Upgrade/Report".PadRight(51) + "║");
Console.WriteLine($"║  Download:     {GetBaseUrl(app)}File/Download/{{hash}}".PadRight(51) + "║");
Console.WriteLine($"║  Packages:     {versionStore.Count} version(s) loaded".PadRight(51) + "║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
if (versionStore.Count > 0)
{
    foreach (var v in versionStore.OrderBy(v => new Version(v.Version!)))
        Console.WriteLine($"  {v.Version,-12} AppType={v.AppType} {(v.IsCrossVersion == true ? $"Cross {v.FromVersion}" : "Full")}  {v.PacketName}");
}
else
{
    Console.WriteLine("  [Warn] No packages loaded. Run generate_packages.ps1 to create sample packages.");
}

app.Run();

// ═══════════════════════════════════════════════════════════════════
// Helpers
// ═══════════════════════════════════════════════════════════════════

static string GetBaseUrl(IHost host)
{
    var config = host.Services.GetRequiredService<IConfiguration>();
    var url = config.GetValue<string>("BaseUrl");
    if (!string.IsNullOrEmpty(url)) return url.EndsWith('/') ? url : url + "/";
    return "http://localhost:5000/";
}

static List<VersionEntry> LoadVersionStore(IWebHostEnvironment env)
{
    var jsonPath = Path.Combine(env.ContentRootPath, "wwwroot", "packages", "versions.json");
    if (!File.Exists(jsonPath))
    {
        Console.WriteLine($"[Warn] versions.json not found at {jsonPath}, using empty store.");
        return new List<VersionEntry>();
    }

    var json = File.ReadAllText(jsonPath);
    var entries = JsonSerializer.Deserialize<List<VersionEntry>>(json) ?? new List<VersionEntry>();

    var packagesDir = Path.Combine(env.ContentRootPath, "wwwroot", "packages");
    foreach (var e in entries)
    {
        var zipPath = Path.Combine(packagesDir, $"{e.PacketName}.zip");
        if (File.Exists(zipPath))
        {
            e.Size ??= new FileInfo(zipPath).Length;
            if (string.IsNullOrEmpty(e.Hash))
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(zipPath);
                e.Hash = Convert.ToHexStringLower(sha256.ComputeHash(stream));
            }
        }
    }

    return entries;
}

// ═══════════════════════════════════════════════════════════════════
// Internal model for versions.json deserialization
// ═══════════════════════════════════════════════════════════════════
record VersionEntry
{
    public string? PacketName { get; set; }
    public string? Hash { get; set; }
    public string? Version { get; set; }
    public string? Url { get; set; }
    public DateTime? PubTime { get; set; }
    public int? AppType { get; set; }
    public int? Platform { get; set; }
    public string? ProductId { get; set; }
    public bool? IsForcibly { get; set; }
    public string? Format { get; set; }
    public long? Size { get; set; }
    public bool? IsFreeze { get; set; }
    public bool? IsCrossVersion { get; set; }
    public string? FromVersion { get; set; }
}
