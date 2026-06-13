using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidUpdate.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient;
    private readonly Func<UpdatePackageDto, string, IAndroidUpdateHandler> _handlerFactory;

    private IAndroidUpdateHandler? _currentHandler;
    private CancellationTokenSource? _cts;

    // ── Bindable properties ────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckUpdateCommand))]
    private string _currentVersion = AndroidUpdate.App.DeviceVersion;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private string _progressText = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckUpdateCommand))]
    private bool _isChecking;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckUpdateCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadUpdateCommand))]
    private bool _isDownloading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadUpdateCommand))]
    private bool _hasUpdate;

    [ObservableProperty]
    private bool _updateChecked;

    [ObservableProperty]
    private bool _showNoUpdateMessage;

    [ObservableProperty]
    private string _newVersion = "";

    [ObservableProperty]
    private string _updateDescription = "";

    // ── Dialog ─────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isDialogVisible;

    [ObservableProperty]
    private string _dialogTitle = "";

    [ObservableProperty]
    private string _dialogMessage = "";

    private TaskCompletionSource<bool>? _dialogTcs;

    private UpdatePackageDto? _pendingUpdate;

    public MainViewViewModel(HttpClient httpClient, Func<UpdatePackageDto, string, IAndroidUpdateHandler> handlerFactory)
    {
        _httpClient = httpClient;
        _handlerFactory = handlerFactory;
    }

    /// <summary>
    /// Configurable server URL for the sample. Defaults to localhost:5000;
    /// override via environment variable ANDROID_UPDATE_SERVER_URL.
    /// </summary>
    private static string ServerBaseUrl =>
        Environment.GetEnvironmentVariable("ANDROID_UPDATE_SERVER_URL")
        ?? "http://localhost:5000";

    // ── Dialog helpers ─────────────────────────────────────────────

    [RelayCommand]
    private void DialogConfirm()
    {
        IsDialogVisible = false;
        _dialogTcs?.TrySetResult(true);
    }

    [RelayCommand]
    private void DialogCancel()
    {
        IsDialogVisible = false;
        _dialogTcs?.TrySetResult(false);
    }

    private Task<bool> ShowConfirmAsync(string title, string message)
    {
        DialogTitle = title;
        DialogMessage = message;
        IsDialogVisible = true;
        _dialogTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return _dialogTcs.Task;
    }

    // ── Commands ───────────────────────────────────────────────────

    private bool CanCheck => !IsChecking && !IsDownloading;

    [RelayCommand(CanExecute = nameof(CanCheck))]
    private async Task CheckUpdateAsync()
    {
        var confirmed = await ShowConfirmAsync(
            "GeneralUpdate.Avalonia",
            "This app uses GeneralUpdate.Avalonia to check for Android APK updates.\n\n" +
            "The library provides version comparison, resumable download with SHA256 verification, and automated APK installation.\n\n" +
            "Do you want to check for updates now?");

        if (!confirmed) return;

        IsChecking = true;
        HasUpdate = false;
        UpdateChecked = false;
        ShowNoUpdateMessage = false;
        StatusText = "Checking for updates...";
        _cts = new CancellationTokenSource();

        try
        {
            var result = await CheckServerAsync(_cts.Token);

            if (result == null)
            {
                StatusText = "No update available.";
                UpdateChecked = true;
                ShowNoUpdateMessage = true;
                return;
            }

            NewVersion = result.Version;
            UpdateDescription = result.Description ?? $"Version {result.Version} is available.";
            _pendingUpdate = result;
            HasUpdate = true;
            UpdateChecked = true;
            StatusText = $"Update v{result.Version} available!";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Cancelled.";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsChecking = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanDownload => !IsDownloading && HasUpdate;

    [RelayCommand(CanExecute = nameof(CanDownload))]
    private async Task DownloadUpdateAsync()
    {
        if (_pendingUpdate == null) return;

        var confirmed = await ShowConfirmAsync(
            "GeneralUpdate.Avalonia",
            $"GeneralUpdate.Avalonia will now download version {_pendingUpdate.Version} APK,\n" +
            $"verify its SHA256 checksum, and launch the Android package installer.\n\n" +
            $"Target version: {_pendingUpdate.Version}\n" +
            $"Package size: {_pendingUpdate.FileSize / 1024 / 1024:F1} MB\n\n" +
            "Do you want to proceed with the update?");

        if (!confirmed) return;

        IsDownloading = true;
        HasUpdate = false;
        StatusText = "Downloading...";
        DownloadProgress = 0;
        ProgressText = "0%";
        _cts = new CancellationTokenSource();

        try
        {
            _currentHandler = _handlerFactory(_pendingUpdate, CurrentVersion);

            _currentHandler.ProgressChanged += (_, p) =>
            {
                DownloadProgress = p;
                ProgressText = $"{p:F1}%";
            };

            _currentHandler.StatusChanged += (_, s) =>
            {
                StatusText = s;
            };

            var success = await _currentHandler.ExecuteAsync(_cts.Token);

            if (success)
            {
                CurrentVersion = _pendingUpdate.Version;
                _pendingUpdate = null; // clear so retry won't use stale data
                StatusText = $"Updated to v{CurrentVersion}!";
                DownloadProgress = 100;
                ProgressText = "Done";
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Cancelled.";
        }
        finally
        {
            IsDownloading = false;
            _cts?.Dispose();
            _cts = null;
            _currentHandler?.Dispose();
            _currentHandler = null;

            // Restore the Download button if an update is still available for retry
            if (_pendingUpdate != null)
                HasUpdate = true;
        }
    }

    // ── Server interaction ─────────────────────────────────────────

    private async Task<UpdatePackageDto?> CheckServerAsync(CancellationToken ct)
    {
        var request = new
        {
            Version = CurrentVersion,
            AppType = 1,
            Platform = 4,
            ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a"
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{ServerBaseUrl}/Upgrade/Verification",
            request, ct);

        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        var body = doc.RootElement.GetProperty("body");
        if (body.GetArrayLength() == 0) return null;

        var entry = body[0];
        return new UpdatePackageDto(
            Version: entry.GetProperty("version").GetString()!,
            DownloadUrl: entry.GetProperty("url").GetString()!,
            Sha256: entry.GetProperty("hash").GetString()!,
            FileSize: entry.TryGetProperty("size", out var s) ? s.GetInt64() : 0,
            Description: entry.TryGetProperty("name", out var n) ? n.GetString() : null,
            IsForced: entry.TryGetProperty("isForcibly", out var f) && f.GetBoolean()
        );
    }
}
