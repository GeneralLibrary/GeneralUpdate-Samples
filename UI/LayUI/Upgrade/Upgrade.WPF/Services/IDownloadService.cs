using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upgrade.WPF.Models;

namespace Upgrade.WPF.Services
{
    /// <summary>
    /// 代码参考 SemiUrsa
    /// </summary>
    public interface IDownloadService
    {
        event Action<DownloadStatistics> ProgressChanged;
        event Action<DownloadStatus> StatusChanged;
        event Action DownloadCompleted;

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
}
