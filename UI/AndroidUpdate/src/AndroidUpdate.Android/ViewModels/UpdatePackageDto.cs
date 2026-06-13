namespace AndroidUpdate.ViewModels;

/// <summary>
/// Describes an update package returned by the server's verification endpoint.
/// </summary>
public sealed record UpdatePackageDto(
    string Version,
    string DownloadUrl,
    string Sha256,
    long FileSize,
    string? Description,
    bool IsForced);
