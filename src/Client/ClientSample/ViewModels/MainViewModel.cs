﻿using Avalonia.Animation.Easings;
using Avalonia.Controls;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Core.Bootstrap;
using GeneralUpdate.Core.Domain.Entity;
using GeneralUpdate.Core.Domain.Enum;
using GeneralUpdate.Core.Events.CommonArgs;
using GeneralUpdate.Core.Events.MultiEventArgs;
using GeneralUpdate.Core.Strategys.PlatformWindows;
using ReactiveUI;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClientSample.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    private const string baseUrl = @"http://127.0.0.1:5001";

    public void Upgrade()
    {
        Task.Run(async () =>
        {
            //ClientStrategy该更新策略将完成1.自动升级组件自更新 2.启动更新组件 3.配置好ClientParameter无需再像之前的版本写args数组进程通讯了。
            //generalClientBootstrap.Config(baseUrl, "B8A7FADD-386C-46B0-B283-C9F963420C7C").
            var configinfo = GetWindowsConfigInfo();
            var generalClientBootstrap = await new GeneralClientBootstrap()
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
            .Config(configinfo)
            .Option(UpdateOption.DownloadTimeOut, 60)
            .Option(UpdateOption.Encoding, Encoding.Default)
            .Option(UpdateOption.Format, Format.ZIP)
            .Strategy<WindowsStrategy>()
            //注入一个func让用户决定是否跳过本次更新，如果是强制更新则不生效
            .SetCustomOption(ShowCustomOption)
            //默认黑名单文件： { "Newtonsoft.Json.dll" } 默认黑名单文件扩展名： { ".patch", ".7z", ".zip", ".rar", ".tar" , ".json" }
            //如果不需要扩展，需要重新传入黑名单集合来覆盖。
            .SetBlacklist(GetBlackFiles(), GetBlackFormats())
            .LaunchTaskAsync();
        });
    }

    /// <summary>
    /// 获取Windows平台所需的配置参数
    /// </summary>
    /// <returns></returns>
    private Configinfo GetWindowsConfigInfo()
    {
        //该对象用于主程序客户端与更新组件进程之间交互用的对象
        var config = new Configinfo();
        //本机的客户端程序应用地址
        config.InstallPath = @"F:\test\Run";
        //更新公告网页
        config.UpdateLogUrl = "https://www.baidu.com/";
        //客户端当前版本号
        config.ClientVersion = "1.1.1.1";
        //客户端类型：1.主程序客户端 2.更新组件
        config.AppType = AppType.ClientApp;
        //指定应用密钥，用于区分客户端应用
        config.AppSecretKey = "B8A7FADD-386C-46B0-B283-C9F963420C7C";
        //更新组件更新包下载地址
        config.UpdateUrl = $"{baseUrl}/versions/{config.AppType}/{config.ClientVersion}/{config.AppSecretKey}";
        //更新程序exe名称
        config.AppName = "AutoUpdate.Core";
        //主程序客户端exe名称
        config.MainAppName = "AutoUpdate.ClientCore";
        //主程序信息
        var mainVersion = "1.1.1.1";
        //主程序客户端更新包下载地址
        config.MainUpdateUrl = $"{baseUrl}/versions/{AppType.ClientApp}/{mainVersion}/{config.AppSecretKey}";
        return config;
    }

    private List<string> GetBlackFiles()
    {
        var blackFiles = new List<string>();
        blackFiles.Add("MainApp");
        return blackFiles;
    }

    private List<string> GetBlackFormats()
    {
        var blackFormats = new List<string>();
        blackFormats.Add(".zip");
        return blackFormats;
    }

    /// <summary>
    /// 让用户决定是否跳过本次更新
    /// </summary>
    /// <returns></returns>
    private async Task<bool> ShowCustomOption()
    {
        return await Task.FromResult(true);
    }

    private void OnMultiDownloadStatistics(object sender, MultiDownloadStatisticsEventArgs e)
    {
        //e.Remaining 剩余下载时间
        //e.Speed 下载速度
        //e.Version 当前下载的版本信息
    }

    private void OnMultiDownloadProgressChanged(object sender, MultiDownloadProgressChangedEventArgs e)
    {
        //e.TotalBytesToReceive 当前更新包需要下载的总大小
        //e.ProgressValue 当前进度值
        //e.ProgressPercentage 当前进度的百分比
        //e.Version 当前下载的版本信息
        //e.Type 当前正在执行的操作  1.ProgressType.Check 检查版本信息中 2.ProgressType.Donwload 正在下载当前版本 3. ProgressType.Updatefile 更新当前版本 4. ProgressType.Done更新完成 5.ProgressType.Fail 更新失败
        //e.BytesReceived 已下载大小
    }

    private void OnException(object sender, ExceptionEventArgs e)
    {
        //DispatchMessage(e.Exception.Message);
    }

    private void OnMultiAllDownloadCompleted(object sender, MultiAllDownloadCompletedEventArgs e)
    {
        //e.FailedVersions; 如果出现下载失败则会把下载错误的版本、错误原因统计到该集合当中。
    }

    private void OnMultiDownloadCompleted(object sender, MultiDownloadCompletedEventArgs e)
    {
        var info = e.Version as VersionInfo;
    }

    private void OnMultiDownloadError(object sender, MultiDownloadErrorEventArgs e)
    {
        var info = e.Version as VersionInfo;
    }

}
