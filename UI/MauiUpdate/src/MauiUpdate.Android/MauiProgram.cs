using MauiUpdate.Services;
using MauiUpdate.ViewModels;
using MauiUpdate.Views;

namespace MauiUpdate;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { });

        // Register HttpClient
        builder.Services.AddSingleton<HttpClient>(_ =>
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            return client;
        });

        // Register services
        builder.Services.AddSingleton<MauiUpdateHandler>();

        // Register ViewModels
        builder.Services.AddSingleton<MainViewModel>();

        // Register Pages
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
