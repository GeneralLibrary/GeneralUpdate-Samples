using GeneralUpdate.Maui.Android.Enums;

namespace MauiUpdate.Models;

/// <summary>
/// Update package info returned by the server version API.
/// Manually parsed from JSON to avoid AOT/trimming issues.
/// </summary>
public sealed class UpdatePackageDto
{
    public string PacketName { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string PubTime { get; set; } = string.Empty;
    public int AppType { get; set; }
    public int Platform { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public bool IsForcibly { get; set; }
    public string Format { get; set; } = ".apk";
    public long Size { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;

    // --- Per-package authentication fields ---
    // Set these when the server requires authentication for update downloads.

    /// <summary>Authentication scheme (Bearer, ApiKey, Basic, Hmac).</summary>
    public AuthScheme? AuthScheme { get; set; }

    /// <summary>Token/key for Bearer or ApiKey authentication.</summary>
    public string? AuthToken { get; set; }

    /// <summary>Secret key for HMAC-SHA256 authentication.</summary>
    public string? AuthSecretKey { get; set; }

    /// <summary>Username for Basic authentication.</summary>
    public string? BasicUsername { get; set; }

    /// <summary>Password for Basic authentication.</summary>
    public string? BasicPassword { get; set; }
}
