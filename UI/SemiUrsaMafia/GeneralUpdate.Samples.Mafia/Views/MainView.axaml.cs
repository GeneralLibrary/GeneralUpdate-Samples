using Avalonia.Controls;
using Common.Avalonia.Services;
using GeneralUpdate.Samples.Mafia.ViewModels;

namespace GeneralUpdate.Samples.Mafia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        SettingsPageContent.DataContext = new SettingsPageViewModel(new MockDownloadService());
    }
}