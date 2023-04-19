using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.ViewModels
{
    public class MainViewModel
    {
        /// <summary>
        /// 启动OSS更新
        /// </summary>
        public void LaunchOSS()
        {
            //Task.Run(async () =>
            //{
            //    var url = "http://192.168.50.203";
            //    var apk = "GeneralUpdate.Client.apk";
            //    var authority = "com.generalupdate.oss";
            //    var currentVersion = "1.0.0.0";
            //    var versionFileName = "version.json";
            //    await GeneralUpdateOSS.Start<Strategy>(new ParamsAndroid(url, apk, authority, currentVersion, versionFileName));
            //});
        }

        /// <summary>
        /// 启动自动升级
        /// </summary>
        public void LaunchUpgrade()
        {
            //Task.Run(async () =>
            //{
            //    //ClientStrategy该更新策略将完成1.自动升级组件自更新 2.启动更新组件 3.配置好ClientParameter无需再像之前的版本写args数组进程通讯了。
            //    //generalClientBootstrap.Config(baseUrl, "B8A7FADD-386C-46B0-B283-C9F963420C7C").
            //    var configinfo = GetWindowsConfiginfo();
            //    var generalClientBootstrap = await new GeneralBootstrap()
            //    //单个或多个更新包下载通知事件
            //    .AddListenerMultiDownloadProgress(OnMultiDownloadProgressChanged)
            //    //单个或多个更新包下载速度、剩余下载事件、当前下载版本信息通知事件
            //    .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
            //    //单个或多个更新包下载完成
            //    .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
            //    //完成所有的下载任务通知
            //    .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
            //    //下载过程出现的异常通知
            //    .AddListenerMultiDownloadError(OnMultiDownloadError)
            //    //整个更新过程出现的任何问题都会通过这个事件通知
            //    .AddListenerException(OnException)
            //    .Config(configinfo)
            //    .Option(UpdateOption.DownloadTimeOut, 60)
            //    .Option(UpdateOption.Encoding, Encoding.Default)
            //    .Option(UpdateOption.Format, Format.ZIP)
            //    .Strategy<WindowsStrategy>()
            //    //注入一个func让用户决定是否跳过本次更新，如果是强制更新则不生效
            //    .SetCustomOption(ShowCustomOption)
            //    //默认黑名单文件： { "Newtonsoft.Json.dll" } 默认黑名单文件扩展名： { ".patch", ".7z", ".zip", ".rar", ".tar" , ".json" }
            //    //如果不需要扩展，需要重新传入黑名单集合来覆盖。
            //    .SetBlacklist(GetBlackFiles(), GetBlackFormats())
            //    .LaunchTaskAsync();
            //});
        }
    }
}
