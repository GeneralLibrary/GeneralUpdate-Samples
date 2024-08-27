using System.Text;
using GeneralUpdate.Core;
using GeneralUpdate.Core.Bootstrap;
using GeneralUpdate.Core.Domain.Enum;
using GeneralUpdate.Core.Events.CommonArgs;
using GeneralUpdate.Core.Events.MultiEventArgs;
using GeneralUpdate.Core.Strategys.PlatformWindows;

Task.Run(async () =>
{
    var bootStrap = await new GeneralUpdateBootstrap()
    //单个或多个更新包下载通知事件
    .AddListenerMultiDownloadProgress(OnMultiDownloadProgressChanged)
    //单个或多个更新包下载速度、剩余下载事件、当前下载版本信息通知事件
    .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
    //单个或多个更新包下载完成
    .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
    //完成所有的下载任务通知
    .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
    //下载过程出现的异常通知
    .AddListenerMultiDownloadError(OnMultiDownloadError)
    //整个更新过程出现的任何问题都会通过这个事件通知
    .AddListenerException(OnException)
    .Strategy<WindowsStrategy>()
    .Option(UpdateOption.Encoding, Encoding.Default)
    .Option(UpdateOption.DownloadTimeOut, 60)
    .Option(UpdateOption.Format, Format.ZIP)
    .LaunchTaskAsync();
});

void OnMultiDownloadStatistics(object sender, MultiDownloadStatisticsEventArgs e)
{
    Console.WriteLine($" {e.Speed} , {e.Remaining.ToShortTimeString()}");
}

void OnMultiDownloadProgressChanged(object sender, MultiDownloadProgressChangedEventArgs e)
{
    switch (e.Type)
    {
        case ProgressType.Check:
            break;

        case ProgressType.Download:
            Console.WriteLine($" {Math.Round(e.ProgressValue * 100, 2)}% ， Receivedbyte：{e.BytesReceived}M ，Totalbyte：{e.TotalBytesToReceive}M");
            break;

        case ProgressType.Updatefile:
            break;

        case ProgressType.Done:
            break;

        case ProgressType.Fail:
            break;
    }
}

void OnMultiDownloadCompleted(object sender, MultiDownloadCompletedEventArgs e)
{
    var info = e.Version as GeneralUpdate.Core.Domain.Entity.VersionInfo;
    Console.WriteLine($"{info.Name} download completed.");
}

void OnMultiAllDownloadCompleted(object sender, MultiAllDownloadCompletedEventArgs e)
{
    Console.WriteLine($"AllDownloadCompleted {e.IsAllDownloadCompleted}");
}

void OnMultiDownloadError(object sender, MultiDownloadErrorEventArgs e)
{
    var info = e.Version as GeneralUpdate.Core.Domain.Entity.VersionInfo;
    Console.WriteLine($"{info.Name},{e.Exception.Message}.");
}

void OnException(object sender, ExceptionEventArgs e)
{
    Console.WriteLine($"{e.Exception.Message}");
}