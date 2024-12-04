using Client.Avalonia.Models;
using Client.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.Avalonia.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    private readonly IDownloadService _downloadService;

    [ObservableProperty] private DownloadStatistics _statistics;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(StopCommand))]
    private DownloadStatus _status;

    public MainViewViewModel(IDownloadService downloadService)
    {
        _downloadService = downloadService;
        _downloadService.ProgressChanged += stats => Statistics = stats;
        _downloadService.StatusChanged += status => Status = status;

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
}