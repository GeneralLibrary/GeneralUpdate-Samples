using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GeneralUpdate.Samples.Shared.Services;
using GeneralUpdate.Samples.Ursa.Desktop.ViewModels;
using GeneralUpdate.Samples.Ursa.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralUpdate.Samples.Ursa.Desktop;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDownloadService, MockDownloadService>();
        services.AddTransient<MainViewViewModel>();
        ServiceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}