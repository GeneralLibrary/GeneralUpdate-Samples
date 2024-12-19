using System.Windows;
using Upgrade.WPF.Views;
using WPFDevelopers.Helpers;
using MessageBox = WPFDevelopers.Controls.MessageBox;

namespace Upgrade.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void BtnUpgrade1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            new UpgradeWindow().MaskShowDialog();
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("是否退出当前系统?", "询问", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                e.Cancel = true;
            }
        }
    }
}