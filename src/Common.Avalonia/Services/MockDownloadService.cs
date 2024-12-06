using Avalonia.Threading;
using Common.Avalonia.Models;

namespace Common.Avalonia.Services;

public class MockDownloadService : IDownloadService
{
    public event Action<DownloadStatistics>? ProgressChanged;
    public event Action<DownloadStatus>? StatusChanged;
    public event Action? DownloadCompleted;

    private readonly DispatcherTimer _timer;
    private readonly Random _random;

    public DownloadStatistics CurrentStatistics { get; private set; }
    public DownloadStatus Status { get; private set; }

    public MockDownloadService()
    {
        CurrentStatistics = new DownloadStatistics
        {
            Version = "11.2.2",
            Speed = 0,
            Remaining = TimeSpan.Zero,
            TotalBytesToReceive = 1024 * 1024 * 10,
            BytesReceived = 0,
            ProgressPercentage = 0
        };
        Status = DownloadStatus.NotStarted;

        _random = new Random();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
        _timer.Tick += UpdateProgress;
    }

    public void Start()
    {
        if (Status is DownloadStatus.Downloading)
        {
            Pause();
            return;
        }

        _timer.Start();
        UpdateStatus(DownloadStatus.Downloading);
    }

    public void Pause()
    {
        if (Status is DownloadStatus.Downloading)
        {
            _timer.Stop();
            UpdateStatus(DownloadStatus.Paused);
            CurrentStatistics.Speed = 0;
            ProgressChanged?.Invoke(CurrentStatistics);
        }
    }

    public void Stop()
    {
        _timer.Stop();
        UpdateStatus(DownloadStatus.NotStarted);

        CurrentStatistics.BytesReceived = 0;
        CurrentStatistics.ProgressPercentage = 0;
        CurrentStatistics.Speed = 0;
        CurrentStatistics.Remaining = TimeSpan.Zero;

        ProgressChanged?.Invoke(CurrentStatistics);
    }

    public void Restart()
    {
        Stop();
        Start();
    }

    private void UpdateProgress(object? sender, EventArgs e)
    {
        var stats = CurrentStatistics;
        var increment = _random.Next(1024 * 1024 * 2, 1024 * 1024 * 3);
        stats.BytesReceived = Math.Min(stats.BytesReceived + increment, stats.TotalBytesToReceive);

        stats.ProgressPercentage = (double)stats.BytesReceived / stats.TotalBytesToReceive * 100;
        stats.Speed = increment / _timer.Interval.TotalSeconds / 1024 / 1024;
        stats.Remaining = stats.BytesReceived >= stats.TotalBytesToReceive
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((stats.TotalBytesToReceive - stats.BytesReceived) / (stats.Speed * 1024 * 1024));

        ProgressChanged?.Invoke(stats);

        if (stats.BytesReceived >= stats.TotalBytesToReceive)
        {
            _timer.Stop();
            UpdateStatus(DownloadStatus.Completed);
            DownloadCompleted?.Invoke();
        }
    }

    private void UpdateStatus(DownloadStatus newStatus)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
            StatusChanged?.Invoke(newStatus);
        }
    }
}