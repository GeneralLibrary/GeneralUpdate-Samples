using Avalonia.Controls;
using Avalonia.Interactivity;
using GeneralUpdate.Samples.Semi.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralUpdate.Samples.Semi.Desktop.Views;

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