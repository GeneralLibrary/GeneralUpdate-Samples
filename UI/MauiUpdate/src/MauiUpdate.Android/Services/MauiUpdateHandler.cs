using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;
using MauiUpdate.Models;

namespace MauiUpdate.Services;

/// <summary>
/// Platform-specific update handler that wraps GeneralUpdate.Maui.Android.
/// Supports optional authentication via HttpDownloadOptions.
/// </summary>
public sealed class MauiUpdateHandler : IDisposable
{
    private IAndroidBootstrap? _bootstrap;
    private string? _fileProviderAuthority;
    private bool _disposed;

    /// <summary>Fires with download progress percentage (0.0 to 100.0).</summary>
    public event EventHandler<double>? ProgressChanged;

    /// <summary>Fires with status text for UI updates.</summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// Initializes the handler with the given FileProvider authority.
    /// Uses default HTTP settings (no auth, system SSL).
    /// </summary>
    public void Initialize(string fileProviderAuthority)
    {
        Initialize(fileProviderAuthority, httpOptions: null);
    }

    /// <summary>
    /// Initializes the handler with FileProvider authority and optional HTTP configuration.
    /// </summary>
    /// <param name="fileProviderAuthority">The FileProvider authority from AndroidManifest.</param>
    /// <param name="httpOptions">
    /// Optional HTTP configuration (SSL validation, proxy, timeouts, authentication).
    /// When null, default HttpClient settings are used (no auth, system SSL).
    /// </param>
    public void Initialize(string fileProviderAuthority, HttpDownloadOptions? httpOptions)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_bootstrap is not null)
            return;

        _fileProviderAuthority = fileProviderAuthority;
        _bootstrap = GeneralUpdateBootstrap.CreateDefault(
            httpClient: null,
            logger: null,
            httpOptions: httpOptions);
        WireEvents();
    }

    /// <summary>
    /// Phase 1: Check if the given package version is newer than the current version.
    /// </summary>
    public async Task<UpdateCheckResult> CheckForUpdateAsync(
        UpdatePackageDto dto,
        string currentVersion,
        CancellationToken ct = default)
    {
        EnsureInitialized();
        var packageInfo = MapToPackageInfo(dto);
        var options = CreateOptions(currentVersion);
        return await _bootstrap!.ValidateAsync(packageInfo, options, ct);
    }

    /// <summary>
    /// Phase 2: Download, verify SHA256, and trigger installation.
    /// </summary>
    public async Task<bool> ExecuteUpdateAsync(
        UpdatePackageDto dto,
        string currentVersion,
        CancellationToken ct = default)
    {
        EnsureInitialized();

        var packageInfo = MapToPackageInfo(dto);
        var options = CreateOptions(currentVersion);

        var result = await _bootstrap!.ExecuteUpdateAsync(packageInfo, options, ct);
        return result.IsSuccess;
    }

    private UpdateOptions CreateOptions(string currentVersion)
    {
        return new UpdateOptions
        {
            CurrentVersion = currentVersion,
            InstallOptions = new AndroidInstallOptions
            {
                FileProviderAuthority = _fileProviderAuthority ?? string.Empty
            }
        };
    }

    private static UpdatePackageInfo MapToPackageInfo(UpdatePackageDto dto)
    {
        return new UpdatePackageInfo
        {
            Version = dto.Version,
            DownloadUrl = dto.DownloadUrl,
            Sha256 = dto.Hash,
            PackageSize = dto.Size > 0 ? dto.Size : null,
            ApkFileName = $"{dto.PacketName}.apk",
            // Per-package auth (set by the consumer if needed)
            AuthScheme = dto.AuthScheme,
            AuthToken = dto.AuthToken,
            AuthSecretKey = dto.AuthSecretKey,
            BasicUsername = dto.BasicUsername,
            BasicPassword = dto.BasicPassword
        };
    }

    private void WireEvents()
    {
        if (_bootstrap is null) return;

        _bootstrap.AddListenerDownloadProgressChanged += (_, args) =>
        {
            ProgressChanged?.Invoke(this, args.Statistics.ProgressPercentage);
        };

        _bootstrap.AddListenerUpdateFailed += (_, args) =>
        {
            StatusChanged?.Invoke(this, $"Failed: {args.Message}");
        };

        _bootstrap.AddListenerUpdateCompleted += (_, args) =>
        {
            StatusChanged?.Invoke(this, $"Stage: {args.Stage}");
        };

        _bootstrap.AddListenerValidate += (_, args) =>
        {
            StatusChanged?.Invoke(this, $"Update found: v{args.PackageInfo.Version}");
        };
    }

    private void EnsureInitialized()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_bootstrap is null)
            throw new InvalidOperationException("Handler not initialized. Call Initialize() first.");
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_bootstrap is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _disposed = true;
    }
}
