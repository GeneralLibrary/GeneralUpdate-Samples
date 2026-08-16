using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GeneralUpdate.Samples.Shared.Models;

public partial class DownloadStatistics : ObservableObject
{
    [ObservableProperty]
    [Description("当前下载版本")]
    public partial object? Version { get; set; }

    [ObservableProperty]
    [Description("下载速度")]
    public partial double Speed { get; set; }

    [ObservableProperty]
    [Description("剩余下载时间")]
    public partial TimeSpan Remaining { get; set; }

    [ObservableProperty]
    [Description("总大小")]
    [NotifyPropertyChangedFor(nameof(TotalBytesToReceiveInMB))]
    public partial long TotalBytesToReceive { get; set; }

    [ObservableProperty]
    [Description("已下载大小")]
    [NotifyPropertyChangedFor(nameof(BytesReceivedInMB))]
    public partial long BytesReceived { get; set; }

    [ObservableProperty]
    [Description("进度百分比")]
    public partial double ProgressPercentage { get; set; }

    public double BytesReceivedInMB => (double)BytesReceived / 1024 / 1024;
    public double TotalBytesToReceiveInMB => (double)TotalBytesToReceive / 1024 / 1024;
}