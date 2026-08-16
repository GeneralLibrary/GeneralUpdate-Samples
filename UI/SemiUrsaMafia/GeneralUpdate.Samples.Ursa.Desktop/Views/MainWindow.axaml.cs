using Avalonia.Interactivity;
using GeneralUpdate.Samples.Ursa.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Ursa.Controls;

namespace GeneralUpdate.Samples.Ursa.Desktop.Views;

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