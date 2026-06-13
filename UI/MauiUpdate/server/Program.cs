using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Allow all origins for local testing
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();

var packagesDir = Path.Combine(app.Environment.ContentRootPath, "packages");
Directory.CreateDirectory(packagesDir);

// GET /packages/{filename} - Serve APK files with range support
app.MapGet("/packages/{filename}", (string filename) =>
{
    var sanitized = Path.GetFileName(filename);
    var filePath = Path.Combine(packagesDir, sanitized);
    if (!File.Exists(filePath))
    {
        return Results.NotFound(new { error = "Package not found.", filename = sanitized });
    }

    return Results.File(
        filePath,
        contentType: "application/vnd.android.package-archive",
        fileDownloadName: sanitized,
        enableRangeProcessing: true);
});

// POST /Upgrade/Verification - Version check API (matches GeneralUpdate server format)
app.MapPost("/Upgrade/Verification", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        // Parse the request to extract current version
        using var doc = JsonDocument.Parse(body);
        var requestVersion = doc.RootElement.GetProperty("Version").GetString() ?? "0.0.0.0";

        // Read versions.json
        var versionsPath = Path.Combine(packagesDir, "versions.json");
        if (!File.Exists(versionsPath))
        {
            return Results.Ok(new { code = 1, message = "No versions file.", body = new List<PackageEntry>() });
        }

        var json = await File.ReadAllTextAsync(versionsPath);
        var packageOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var packages = JsonSerializer.Deserialize<List<PackageEntry>>(json, packageOptions);

        if (packages is null || packages.Count == 0)
        {
            return Results.Ok(new { code = 1, message = "No packages configured.", body = new List<PackageEntry>() });
        }

        // Find the latest version that is newer than the request
        var current = Version.TryParse(requestVersion, out var cv) ? cv : new Version(0, 0, 0, 0);
        var available = packages
            .Select(p => new { Package = p, ParsedVersion = Version.TryParse(p.Version, out var v) ? v : new Version(0, 0, 0, 0) })
            .Where(x => x.ParsedVersion > current)
            .OrderByDescending(x => x.ParsedVersion)
            .ToList();

        if (available.Count == 0)
        {
            return Results.Ok(new { code = 1, message = "No updates available.", body = new List<PackageEntry>() });
        }

        var latest = available[0].Package;
        return Results.Ok(new { code = 0, message = "Success", body = new[] { latest } });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { code = -1, message = ex.Message, body = new List<PackageEntry>() });
    }
});

// POST /Upgrade/Report - Report update result
app.MapPost("/Upgrade/Report", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine($"[Report] {body}");
    return Results.Ok(new { code = 0, message = "Report received." });
});

// Health check
app.MapGet("/", () => Results.Ok(new { status = "running", server = "MauiUpdate Server" }));

Console.WriteLine("MauiUpdate Server running on http://0.0.0.0:5000");
app.Run();

/// <summary>
/// Package entry matching the versions.json schema.
/// JsonPropertyName attributes ensure correct serialization for the client.
/// </summary>
internal sealed record PackageEntry
{
    [JsonPropertyName("PacketName")] public string PacketName { get; init; } = string.Empty;
    [JsonPropertyName("Hash")] public string Hash { get; init; } = string.Empty;
    [JsonPropertyName("Version")] public string Version { get; init; } = string.Empty;
    [JsonPropertyName("PubTime")] public string PubTime { get; init; } = string.Empty;
    [JsonPropertyName("AppType")] public int AppType { get; init; }
    [JsonPropertyName("Platform")] public int Platform { get; init; }
    [JsonPropertyName("ProductId")] public string ProductId { get; init; } = string.Empty;
    [JsonPropertyName("IsForcibly")] public bool IsForcibly { get; init; }
    [JsonPropertyName("Format")] public string Format { get; init; } = ".apk";
    [JsonPropertyName("Size")] public long Size { get; init; }
}
