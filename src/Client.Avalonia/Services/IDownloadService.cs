using System;
using System.ComponentModel;
using Client.Avalonia.Models;

namespace Client.Avalonia.Services;

public interface IDownloadService
{
    event Action<DownloadStatistics> ProgressChanged;
    event Action<DownloadStatus> StatusChanged;

    DownloadStatistics CurrentStatistics { get; }
    DownloadStatus Status { get; }

    void Start();
    void Pause();
    void Stop();
    void Restart();
}

[Flags]
public enum DownloadStatus
{
    [Description("未开始")] NotStarted,
    [Description("下载中")] Downloading,
    [Description("已暂停")] Paused,
    [Description("已完成")] Completed
}