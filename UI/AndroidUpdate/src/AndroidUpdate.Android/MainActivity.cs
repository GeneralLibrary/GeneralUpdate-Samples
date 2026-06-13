using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace AndroidUpdate.Android;

[Activity(
    Label = "AndroidUpdate",
    Theme = "@style/MyTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
