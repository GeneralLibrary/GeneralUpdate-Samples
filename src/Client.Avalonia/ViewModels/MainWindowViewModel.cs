using System;
using System.ComponentModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private DownloadStatistics _statistics;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(StopCommand))]
    [NotifyPropertyChangedFor(nameof(IsDownloading), nameof(IsCompleted))]
    private DownloadStatus _status;

    public bool IsDownloading => Status is DownloadStatus.Downloading;
    public bool IsCompleted => Status is DownloadStatus.Completed;

    private readonly DispatcherTimer _timer;
    private readonly Random _random;

    public MainWindowViewModel()
    {
        Statistics = new DownloadStatistics
        {
            Version = "11.2.2",
            Speed = 0,
            Remaining = TimeSpan.Zero,
            TotalBytesToReceive = 1024 * 1024 * 10,
            BytesReceived = 1024 * 1024 * 0,
            ProgressPercentage = 0
        };
        Status = DownloadStatus.NotStarted;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        _timer.Tick += UpdateProgress;

        _random = new Random();
    }

    #region Buttons

    private bool CanStart => Status is DownloadStatus.NotStarted or DownloadStatus.Downloading or DownloadStatus.Paused;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
            Status = DownloadStatus.Downloading;
        }
        else
        {
            _timer.Stop();
            Status = DownloadStatus.Paused;
            Statistics.Speed = 0;
            Statistics.Remaining = TimeSpan.Zero;
        }
    }

    private bool CanStop => Status is DownloadStatus.Downloading or DownloadStatus.Paused;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        _timer.Stop();
        Status = DownloadStatus.NotStarted;
        Statistics.BytesReceived = 0;
        Statistics.ProgressPercentage = 0;
        Statistics.Speed = 0;
        Statistics.Remaining = TimeSpan.Zero;
        OnPropertyChanged(nameof(Statistics));
    }

    [RelayCommand]
    private void Restart()
    {
        Stop();
        Start();
    }

    #endregion


    private void UpdateProgress(object? sender, EventArgs e)
    {
        var received = Statistics.BytesReceived;
        var total = Statistics.TotalBytesToReceive;
        if (received < total)
        {
            var increment = _random.Next(1024 * 1024 * 2, 1024 * 1024 * 3);
            received += increment;

            if (received > total)
            {
                received = total;
            }

            Statistics.BytesReceived = received;
            Statistics.ProgressPercentage = (double)received / total * 100;

            var currentSpeed = increment / _timer.Interval.TotalSeconds;
            Statistics.Speed = currentSpeed / 1024 / 1024;

            var remainingBytes = total - received;
            Statistics.Remaining = TimeSpan.FromSeconds(remainingBytes / currentSpeed);

            OnPropertyChanged(nameof(Statistics));

            if (received >= total)
            {
                _timer.Stop();
                Status = DownloadStatus.Completed;
                Statistics.Speed = 0;
                Statistics.Remaining = TimeSpan.Zero;
            }
        }
    }
}

public partial class DownloadStatistics : ObservableObject
{
    [ObservableProperty] [Description("当前下载版本")]
    private object? _version;

    [ObservableProperty] [Description("下载速度")]
    private double _speed;

    [ObservableProperty] [Description("剩余下载时间")]
    private TimeSpan _remaining;

    [ObservableProperty] [Description("总大小")] [NotifyPropertyChangedFor(nameof(TotalBytesToReceiveInMB))]
    private long _totalBytesToReceive;

    [ObservableProperty] [Description("已下载大小")] [NotifyPropertyChangedFor(nameof(BytesReceivedInMB))]
    private long _bytesReceived;

    [ObservableProperty] [Description("进度百分比")]
    private double _progressPercentage;

    public double BytesReceivedInMB => (double)BytesReceived / 1024 / 1024;
    public double TotalBytesToReceiveInMB => (double)TotalBytesToReceive / 1024 / 1024;
}

[Flags]
public enum DownloadStatus
{
    NotStarted,
    Downloading,
    Paused,
    Completed
}