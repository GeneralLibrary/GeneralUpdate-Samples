using System;
using System.ComponentModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private DownloadStatistics _statistics;
    [ObservableProperty] private bool _isCompleted;
    private readonly DispatcherTimer _timer;

    public MainWindowViewModel()
    {
        Statistics = new DownloadStatistics
        {
            Version = "1.0.0",
            Speed = "1MB/s",
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
        _timer.Start();
    }

    private void UpdateProgress(object? sender, EventArgs e)
    {
        if (Statistics.BytesReceived < Statistics.TotalBytesToReceive)
        {
            Statistics.BytesReceived += 1024 * 1024;
            Statistics.ProgressPercentage =
                (double)Statistics.BytesReceived / Statistics.TotalBytesToReceive * 100;

            Statistics.Remaining = TimeSpan.FromSeconds(
                (Statistics.TotalBytesToReceive - Statistics.BytesReceived) / (1024 * 1024));

            OnPropertyChanged(nameof(Statistics));

            if (Statistics.BytesReceived >= Statistics.TotalBytesToReceive)
            {
                _timer.Stop();
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