using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using AndroidUpdate.ViewModels;
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Abstractions;
using GeneralUpdate.Avalonia.Android.Models;

namespace AndroidUpdate.Android.Services;

public sealed class AndroidUpdateHandler : IAndroidUpdateHandler
{
    private readonly IAndroidBootstrap _bootstrap;
    private readonly UpdatePackageDto _packageInfo;
    private bool _disposed;

    public event EventHandler<double>? ProgressChanged;
    public event EventHandler<string>? StatusChanged;

    public AndroidUpdateHandler(UpdatePackageDto packageInfo, string currentVersion)
    {
        _packageInfo = packageInfo ?? throw new ArgumentNullException(nameof(packageInfo));
        _ = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));

        var options = new AndroidUpdateOptions
        {
            FileProviderAuthority = "com.generalupdate.androidupdate.fileprovider"
        };

        _bootstrap = GeneralUpdateBootstrap.CreateDefault(options);
        WireEvents();
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var updatePackage = new UpdatePackageInfo
            {
                Version = _packageInfo.Version,
                DownloadUrl = _packageInfo.DownloadUrl,
                Sha256 = _packageInfo.Sha256,
                FileSize = _packageInfo.FileSize,
                FileName = $"app_update_{_packageInfo.Version}.apk",
                IsForced = _packageInfo.IsForced
            };

            // Step 1: Download and verify
            StatusChanged?.Invoke(this, "Downloading package...");
            var downloadResult = await _bootstrap.DownloadAndVerifyAsync(updatePackage, cancellationToken);
            if (!downloadResult.Success)
            {
                StatusChanged?.Invoke(this, $"Download failed: {downloadResult.Message} (reason: {downloadResult.FailureReason})");
                return false;
            }

            StatusChanged?.Invoke(this, "Download OK, checking install permission...");

            // Step 2: Check "Install unknown apps" permission (Android 8+)
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var ctx = global::Android.App.Application.Context;
                var pm = ctx.PackageManager;
                if (pm != null && !pm.CanRequestPackageInstalls())
                {
                    StatusChanged?.Invoke(this, "Tap Download again after allowing install permission.");
                    var intent = new Intent(global::Android.Provider.Settings.ActionManageUnknownAppSources)
                        .SetData(global::Android.Net.Uri.Parse("package:" + ctx.PackageName));
                    intent.AddFlags(ActivityFlags.NewTask);
                    ctx.StartActivity(intent);
                    return false;
                }
            }

            // Step 3: Launch Android installer
            StatusChanged?.Invoke(this, "Installing...");
            var installResult = await _bootstrap.LaunchInstallerAsync(updatePackage, downloadResult.FilePath!, cancellationToken);
            if (!installResult.Success)
            {
                StatusChanged?.Invoke(this, $"Install failed: {installResult.Message} (reason: {installResult.FailureReason})");
                return false;
            }
            return true;
        }
        catch (System.OperationCanceledException)
        {
            StatusChanged?.Invoke(this, "Cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, $"Error: {ex.Message}");
            return false;
        }
    }

    private void WireEvents()
    {
        _bootstrap.AddListenerDownloadProgressChanged += (_, args) =>
        {
            ProgressChanged?.Invoke(this, args.ProgressPercentage);
        };

        _bootstrap.AddListenerUpdateFailed += (_, args) =>
        {
            StatusChanged?.Invoke(this, $"Failed: {args.Result.Message}");
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _bootstrap.Dispose();
        _disposed = true;
    }
}
