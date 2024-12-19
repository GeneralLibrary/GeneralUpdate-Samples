using System.Windows;
using WPFDevelopers.Controls;

namespace Upgrade.WPF.Views
{
    /// <summary>
    /// UpgradeWindow1.xaml 的交互逻辑
    /// </summary>
    public partial class UpgradeWindow
    {
        private double currentProgress = 0;
        private const double Radius = 100; 
        public UpgradeWindow()
        {
            InitializeComponent();
            Loaded += UpgradeWindow_Loaded;
            
        }

        private void UpgradeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartDownload();
        }

        private async void StartDownload()
        {
            for (double i = 0; i <= 100; i += 1)
            {
                UpdateProgressArc(i);
                await Task.Delay(100); 
            }
        }

        private void UpdateProgressArc(double value)
        {
            PART_Slider.Value = value;
            if (value >= 100)
                NotifyIcon.ShowBalloonTip("提醒", " 已更新到最新版本，当前版本 V9.0", NotifyIconInfoType.Info);
        }
    }
}
