using LayUI.Wpf.Global;
using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Windows;
using Upgrade.WPF.ViewModels;
using Upgrade.WPF.Views;

namespace Upgrade.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RegisterDialogs();
            base.OnStartup(e);
        }

        private void RegisterDialogs()
        {
            LayDialog.RegisterDialog<Alert>(nameof(Alert));
        }
    } 
}
