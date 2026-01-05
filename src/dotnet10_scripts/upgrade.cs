#:package GeneralUpdate.Core @9.5.10

using GeneralUpdate.Common.Download;
using GeneralUpdate.Common.Internal;
using GeneralUpdate.Common.Shared.Object;
using GeneralUpdate.Core;

try
{
    Console.WriteLine($"升级程序初始化，{DateTime.Now}！");
    Console.WriteLine("当前运行目录：" + Context.ScriptDirectory);
    _ = await new GeneralUpdateBootstrap()
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
        //设置字段映射表，用于解析所有驱动包的信息的字符串
        //.SetFieldMappings(fieldMappingsCN)
        //是否开启驱动更新
        //.Option(UpdateOption.Drive, true)
        .LaunchAsync();
    Console.WriteLine($"升级程序已启动，{DateTime.Now}！");
    await Task.Delay(2000);
}
catch (Exception e)
{
    Console.WriteLine(e.Message + "\n" + e.StackTrace);
}

void OnMultiDownloadError(object arg1, MultiDownloadErrorEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine($"{version.Version} {arg2.Exception}");
}

void OnMultiAllDownloadCompleted(object arg1, MultiAllDownloadCompletedEventArgs arg2)
{
    Console.WriteLine(arg2.IsAllDownloadCompleted ? "所有的下载任务已完成！" : $"下载任务已失败！{arg2.FailedVersions.Count}");
}

void OnMultiDownloadCompleted(object arg1, MultiDownloadCompletedEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine(arg2.IsComplated ? $"当前下载版本：{version.Version}, 下载完成！" : $"当前下载版本：{version.Version}, 下载失败！");
}

void OnMultiDownloadStatistics(object arg1, MultiDownloadStatisticsEventArgs arg2)
{
    var version = arg2.Version as VersionInfo;
    Console.WriteLine($"当前下载版本：{version.Version}，下载速度：{arg2.Speed}，剩余下载时间：{arg2.Remaining}，已下载大小：{arg2.BytesReceived}，总大小：{arg2.TotalBytesToReceive}, 进度百分比：{arg2.ProgressPercentage}%");
}

void OnException(object arg1, ExceptionEventArgs arg2)
{
    Console.WriteLine($"{arg2.Exception}");
}