using Avalonia.Controls;
using Client.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var provider = App.ServiceProvider;
        if (provider is not null)
        {
            DataContext = provider.GetRequiredService<MainViewViewModel>();
        }
    }
}