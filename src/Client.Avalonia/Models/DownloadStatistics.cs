using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Avalonia.Models;

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