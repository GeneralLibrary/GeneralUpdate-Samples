using Android.App;
using Android.Runtime;
using AndroidUpdate.ViewModels;
using Avalonia;
using Avalonia.Android;

namespace AndroidUpdate.Android;

[global::Android.App.Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
        // Read real app version from package manager as early as possible
        try
        {
            var pkgInfo = PackageManager?.GetPackageInfo(PackageName!, 0);
            if (pkgInfo?.VersionName != null)
                App.DeviceVersion = pkgInfo.VersionName;
        }
        catch { /* fallback to default "1.0.0.0" */ }
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        // Register the Android-specific handler factory before Avalonia starts
        App.HandlerFactory = (packageInfo, currentVersion) =>
            new Services.AndroidUpdateHandler(packageInfo, currentVersion);

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
