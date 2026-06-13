using System;
using System.Net.Http;
using AndroidUpdate.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AndroidUpdate;

public partial class App : Avalonia.Application
{
    /// <summary>
    /// Static factory for creating platform-specific update handlers.
    /// Set by the Android project (or other platform projects) during startup.
    /// </summary>
    public static Func<UpdatePackageDto, string, IAndroidUpdateHandler>? HandlerFactory { get; set; }

    /// <summary>
    /// The device's currently installed app version.
    /// Set by the platform project on startup; falls back to "1.0.0.0".
    /// </summary>
    public static string DeviceVersion { get; set; } = "1.0.0.0";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        MainViewViewModel CreateViewModel() =>
            new(httpClient, (pkg, ver) =>
            {
                if (HandlerFactory == null)
                    throw new InvalidOperationException(
                        "HandlerFactory not set. Ensure the platform project initializes it.");
                return HandlerFactory(pkg, ver);
            });

        if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactory)
        {
            // Android: use MainViewFactory for Activity-based lifetime
            singleViewFactory.MainViewFactory = () =>
                new Views.MainView { DataContext = CreateViewModel() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new Views.MainView
            {
                DataContext = CreateViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
