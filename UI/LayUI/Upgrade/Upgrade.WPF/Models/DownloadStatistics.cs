using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upgrade.WPF.Models
{
    /// <summary>
    /// 代码参考 SemiUrsa
    /// </summary>
    public class DownloadStatistics: BindableBase
    {
        private object? _Version;
        [Description("当前下载版本")]
        public object? Version
        {
            get { return _Version; }
            set { SetProperty(ref _Version, value); }
        }
        private double _Speed;
        [Description("下载速度")]
        public double Speed
        {
            get { return _Speed; }
            set { SetProperty(ref _Speed, value); }
        }
        private TimeSpan _Remaining;
        [Description("剩余下载时间")]
        public TimeSpan Remaining
        {
            get { return _Remaining; }
            set { SetProperty(ref _Remaining, value); }
        }
        private long _TotalBytesToReceive;
        [Description("总大小")]
        public long TotalBytesToReceive
        {
            get { return _TotalBytesToReceive; }
            set 
            { 
                SetProperty(ref _TotalBytesToReceive, value);
                RaisePropertyChanged(nameof(TotalBytesToReceiveInMB));
            }
        }
        private long _BytesReceived;
        [Description("已下载大小")]
        public long BytesReceived
        {
            get { return _BytesReceived; }
            set 
            { 
                SetProperty(ref _BytesReceived, value);
                RaisePropertyChanged(nameof(BytesReceivedInMB));
            }
        }
        private double _ProgressPercentage;
        [Description("进度百分比")]
        public double ProgressPercentage
        {
            get { return _ProgressPercentage; }
            set { SetProperty(ref _ProgressPercentage, value); }
        } 
        public double BytesReceivedInMB => (double)BytesReceived / 1024 / 1024;  
        public double TotalBytesToReceiveInMB => (double)TotalBytesToReceive / 1024 / 1024;
    }
}
