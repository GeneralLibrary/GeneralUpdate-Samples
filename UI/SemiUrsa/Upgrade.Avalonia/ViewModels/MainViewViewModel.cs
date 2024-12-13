using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Common.Avalonia.Models;
using Common.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ursa.Controls;

namespace Upgrade.Avalonia.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    private readonly IDownloadService _downloadService;
    private readonly WindowToastManager _notificationManager;

    [ObservableProperty] private DownloadStatistics _statistics;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(StopCommand))]
    private DownloadStatus _status;

    public MainViewViewModel(IDownloadService downloadService)
    {
        _downloadService = downloadService;
        _notificationManager = new WindowToastManager(ResolveDefaultTopLevel());

        _downloadService.ProgressChanged += stats => Statistics = stats;
        _downloadService.StatusChanged += status => Status = status;
        _downloadService.DownloadCompleted += OnDownloadCompleted;

        Statistics = _downloadService.CurrentStatistics;
        Status = _downloadService.Status;
    }

    #region Buttons

    private bool CanStart => Status is DownloadStatus.NotStarted or DownloadStatus.Downloading or DownloadStatus.Paused;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start() => _downloadService.Start();

    private bool CanStop => Status is DownloadStatus.Downloading or DownloadStatus.Paused;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _downloadService.Stop();

    [RelayCommand]
    private void Restart() => _downloadService.Restart();

    #endregion

    private void OnDownloadCompleted()
    {
        _notificationManager.Show(
            new Toast($"已更新到最新版本，版本号：{Statistics.Version}"),
            type: NotificationType.Success,
            classes: ["Light"]);
    }

    private static TopLevel? ResolveDefaultTopLevel()
    {
        return Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null
        };
    }
}