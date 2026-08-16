using Avalonia.Controls;
using Common.Avalonia.Services;
using Sandbox.ViewModels;

namespace Sandbox.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        SettingsPageContent.DataContext = new SettingsPageViewModel(new MockDownloadService());
    }
}