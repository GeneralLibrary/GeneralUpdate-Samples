using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiUpdate.Models;
using MauiUpdate.Services;

namespace MauiUpdate.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly MauiUpdateHandler _handler;
    private string _serverUrl = "http://localhost:5000";

    [ObservableProperty]
    private string _statusText = "Ready — tap Check for Updates.";

    [ObservableProperty]
    private string _currentVersion = GetInstalledVersion();

    [ObservableProperty]
    private string? _updateVersion;

    [ObservableProperty]
    private string? _updateDescription;

    [ObservableProperty]
    private bool _hasUpdate;

    [ObservableProperty]
    private bool _isChecking;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private bool _showNoUpdateMessage;

    private UpdatePackageDto? _pendingUpdate;

    public MainViewModel(HttpClient httpClient, MauiUpdateHandler handler)
    {
        _httpClient = httpClient;
        _handler = handler;

        // Initialize the handler once at startup
        _handler.Initialize("com.generalupdate.mauiupdate.fileprovider");

        _handler.ProgressChanged += (_, progress) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressValue = progress;
                ProgressText = $"{progress:F1}%";
            });
        };

        _handler.StatusChanged += (_, status) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusText = status;
            });
        };
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        if (IsChecking || IsDownloading) return;

        IsChecking = true;
        HasUpdate = false;
        ShowNoUpdateMessage = false;
        ProgressValue = 0;
        ProgressText = string.Empty;
        UpdateVersion = null;
        UpdateDescription = null;
        _pendingUpdate = null;

        try
        {
            _serverUrl = Preferences.Get("ServerUrl", "http://localhost:5000");

            using var response = await _httpClient.PostAsync(
                $"{_serverUrl}/Upgrade/Verification",
                new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        Version = CurrentVersion,
                        AppType = 1,
                        Platform = 4,
                        ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a"
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"));

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var packages = ParseBodyPackages(root);
            if (packages is null || packages.Count == 0)
            {
                StatusText = "No updates available.";
                ShowNoUpdateMessage = true;
                return;
            }

            var latest = packages[0];
            var ext = FormatExtension(latest.Format);
            latest.DownloadUrl = $"{_serverUrl}/packages/{latest.PacketName}{ext}";

            var checkResult = await _handler.CheckForUpdateAsync(latest, CurrentVersion);

            if (checkResult.IsUpdateAvailable)
            {
                _pendingUpdate = latest;
                HasUpdate = true;
                UpdateVersion = $"v{latest.Version}";
                UpdateDescription = $"{latest.PacketName}\n{FormatSize(latest.Size)}";
                StatusText = $"Update v{latest.Version} available!";
            }
            else
            {
                ShowNoUpdateMessage = true;
                StatusText = "You're up to date!";
            }
        }
        catch (HttpRequestException ex)
        {
            StatusText = $"Connection: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            StatusText = "Request timed out.";
        }
        catch (JsonException ex)
        {
            StatusText = $"JSON: {ex.Message}";
        }
        catch (Exception ex)
        {
            StatusText = $"{ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            IsChecking = false;
        }
    }

    [RelayCommand]
    private async Task DownloadAndInstallAsync()
    {
        if (_pendingUpdate is null || IsDownloading) return;

        IsDownloading = true;
        ProgressValue = 0;
        ProgressText = "Starting...";
        StatusText = "Downloading...";

        string? handlerError = null;
        void OnStatusChanged(object? s, string msg)
        {
            if (msg.StartsWith("Failed:"))
                handlerError = msg;
        }
        _handler.StatusChanged += OnStatusChanged;

        try
        {
            var dto = new UpdatePackageDto
            {
                PacketName = _pendingUpdate.PacketName,
                Hash = _pendingUpdate.Hash,
                Version = _pendingUpdate.Version,
                PubTime = _pendingUpdate.PubTime,
                AppType = _pendingUpdate.AppType,
                Platform = _pendingUpdate.Platform,
                ProductId = _pendingUpdate.ProductId,
                IsForcibly = _pendingUpdate.IsForcibly,
                Format = _pendingUpdate.Format,
                Size = _pendingUpdate.Size,
                DownloadUrl = $"{_serverUrl}/packages/{_pendingUpdate.PacketName}{FormatExtension(_pendingUpdate.Format)}"
            };

            // The library (AndroidApkInstaller) automatically checks
            // CanRequestPackageInstalls() and throws a descriptive error if denied,
            // guiding the user to enable the permission in system settings.
            var success = await _handler.ExecuteUpdateAsync(dto, CurrentVersion);

            if (success)
            {
                StatusText = "Installer launched! Complete install on device.";
                ProgressText = "Done!";
            }
            else
            {
                StatusText = handlerError ?? "Update failed (no details).";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            _handler.StatusChanged -= OnStatusChanged;
            IsDownloading = false;
        }
    }

    private static List<UpdatePackageDto>? ParseBodyPackages(JsonElement root)
    {
        if (!root.TryGetProperty("body", out var bodyEl) || bodyEl.ValueKind != JsonValueKind.Array)
            return null;

        if (bodyEl.GetArrayLength() == 0)
            return null;

        var result = new List<UpdatePackageDto>();
        foreach (var item in bodyEl.EnumerateArray())
        {
            result.Add(new UpdatePackageDto
            {
                PacketName = GetString(item, "PacketName") ?? string.Empty,
                Hash = GetString(item, "Hash") ?? string.Empty,
                Version = GetString(item, "Version") ?? string.Empty,
                PubTime = GetString(item, "PubTime") ?? string.Empty,
                AppType = GetInt(item, "AppType"),
                Platform = GetInt(item, "Platform"),
                ProductId = GetString(item, "ProductId") ?? string.Empty,
                IsForcibly = item.TryGetProperty("IsForcibly", out var f) && f.ValueKind == JsonValueKind.True,
                Format = GetString(item, "Format") ?? ".apk",
                Size = GetLong(item, "Size")
            });
        }
        return result;
    }

    private static string? GetString(JsonElement el, string n) =>
        el.TryGetProperty(n, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
    private static int GetInt(JsonElement el, string n) =>
        el.TryGetProperty(n, out var p) && p.ValueKind == JsonValueKind.Number ? p.GetInt32() : 0;
    private static long GetLong(JsonElement el, string n) =>
        el.TryGetProperty(n, out var p) && p.ValueKind == JsonValueKind.Number ? p.GetInt64() : 0;

    private static string FormatExtension(string? format) =>
        string.IsNullOrWhiteSpace(format) ? ".apk" : format;

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024.0):F1} MB"
    };

    private static string GetInstalledVersion()
    {
        try
        {
            return Microsoft.Maui.ApplicationModel.AppInfo.VersionString;
        }
        catch
        {
            try
            {
                var ctx = Android.App.Application.Context;
                var pm = ctx.PackageManager!;
                var pkg = pm.GetPackageInfo(ctx.PackageName!, 0);
                return pkg?.VersionName ?? "1.0.0.0";
            }
            catch
            {
                return "1.0.0.0";
            }
        }
    }
}
