using LayUI.Wpf.Enum;
using LayUI.Wpf.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Upgrade.WPF.Models;
using Upgrade.WPF.Services;

namespace Upgrade.WPF.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDownloadService _downloadService;
        private DownloadStatistics _Statistics;
        public DownloadStatistics Statistics
        {
            get { return _Statistics; }
            set { SetProperty(ref _Statistics, value); }
        }
        private DownloadStatus _Status;
        public DownloadStatus Status
        {
            get { return _Status; }
            set { SetProperty(ref _Status, value); }
        }
        private bool CanStart => Status is DownloadStatus.NotStarted or DownloadStatus.Downloading or DownloadStatus.Paused;
        public MainWindowViewModel()
        {
            _downloadService = new MockDownloadService();
            _downloadService.ProgressChanged += stats => Statistics = stats;
            _downloadService.StatusChanged += status => Status = status;
            _downloadService.DownloadCompleted += OnDownloadCompleted;
            Statistics = _downloadService.CurrentStatistics;
            Status = _downloadService.Status;
        }
        private DelegateCommand _LoadedCommand;
        public DelegateCommand LoadedCommand =>
            _LoadedCommand ?? (_LoadedCommand = new DelegateCommand(ExecuteLoadedCommand));

        void ExecuteLoadedCommand()
        {
            StartComamnd?.Execute(null);
        }
        private DelegateCommand _StartComamnd;
        public DelegateCommand StartComamnd =>
            _StartComamnd ?? (_StartComamnd = new DelegateCommand(ExecuteStartComamnd));

        void ExecuteStartComamnd()
        {
            _downloadService.Start();
        }
        private DelegateCommand _StopCommand;
        public DelegateCommand StopCommand =>
            _StopCommand ?? (_StopCommand = new DelegateCommand(ExecuteStopCommand));

        void ExecuteStopCommand()
        {
            _downloadService.Stop();
        }
        private DelegateCommand _RestartCommand;
        public DelegateCommand RestartCommand =>
            _RestartCommand ?? (_RestartCommand = new DelegateCommand(ExecuteRestartCommand));

        void ExecuteRestartCommand()
        {
            _downloadService.Restart();
        }
        private void OnDownloadCompleted()
        {
            LayDialog.ShowDialog("Alert", null, res =>
            {
                Application.Current.Shutdown();
            }, "RootDialogToken");
        }
    }
}
