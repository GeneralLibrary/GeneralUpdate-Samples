using Android.Content.PM;
using Avalonia.Android;

namespace GeneralUpdate.Samples.Mafia.Android;

[Activity(
    Label = "GeneralUpdate.Samples.Mafia.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    public override void OnBackPressed()
    {
        base.OnBackPressed();
    }
}