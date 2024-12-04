using System;
using System.ComponentModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private DownloadStatistics _statistics;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private bool _isCompleted;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(StopCommand))]
    private bool _isUpdating;

    private readonly DispatcherTimer _timer;
    private readonly Random _random;

    public MainWindowViewModel()
    {
        Statistics = new DownloadStatistics
        {
            Version = "11.2.2",
            Speed = "0MB/s",
            Remaining = TimeSpan.Zero,
            TotalBytesToReceive = 1024 * 1024 * 10,
            BytesReceived = 1024 * 1024 * 0,
            ProgressPercentage = 0
        };

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        _timer.Tick += UpdateProgress;

        _random = new Random();
    }

    private bool CanStart => !IsCompleted;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
            IsUpdating = true;
        }
        else
        {
            _timer.Stop();
            IsUpdating = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsUpdating))]
    private void Stop()
    {
        _timer.Stop();
        Statistics.BytesReceived = 0;
        Statistics.ProgressPercentage = 0;
        Statistics.Speed = "0 MB/s";
        Statistics.Remaining = TimeSpan.Zero;
        IsCompleted = false;
        IsUpdating = false;
        OnPropertyChanged(nameof(Statistics));
    }

    [RelayCommand]
    private void Restart()
    {
        Stop();
        Start();
    }

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
            Statistics.Speed = $"{currentSpeed / 1024 / 1024:F1} MB/s";

            var remainingBytes = total - received;
            Statistics.Remaining = TimeSpan.FromSeconds(remainingBytes / currentSpeed);

            OnPropertyChanged(nameof(Statistics));

            if (received >= total)
            {
                _timer.Stop();
                IsUpdating = false;
                IsCompleted = true;
            }
        }
    }
}

public partial class DownloadStatistics : ObservableObject
{
    [ObservableProperty] [Description("当前下载版本")]
    private object? _version;

    [ObservableProperty] [Description("下载速度")]
    private string? _speed;

    [ObservableProperty] [Description("剩余下载时间")]
    private TimeSpan _remaining;

    [ObservableProperty] [Description("已下载大小")]
    private long _totalBytesToReceive;

    [ObservableProperty] [Description("总大小")]
    private long _bytesReceived;

    [ObservableProperty] [Description("进度百分比")]
    private double _progressPercentage;
}