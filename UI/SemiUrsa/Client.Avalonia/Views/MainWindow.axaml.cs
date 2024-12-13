using Avalonia.Controls;
using Avalonia.Interactivity;
using Client.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Avalonia.Views;

public partial class MainWindow : Window
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