using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Common.Avalonia.Services;
using Microsoft.Extensions.DependencyInjection;
using Upgrade.Avalonia.ViewModels;
using Upgrade.Avalonia.Views;

namespace Upgrade.Avalonia;

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