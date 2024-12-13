using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Upgrade.Avalonia.ViewModels;
using Ursa.Controls;

namespace Upgrade.Avalonia.Views;

public partial class MainWindow : UrsaWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        DataContext = App.ServiceProvider?.GetRequiredService<MainViewViewModel>();
    }
}