---
sidebar_position: 2
---

### 定义

命名空间：GeneralUpdate.ClientCore

程序集：GeneralUpdate.ClientCore.dll



GeneralUpdate.ClientCore是最核心的组件之一，提供了大量主要功能。本质和Core没有区别，但是有职责上的区别ClientCore用于主程序中，更新升级助手然后关闭主程序启动升级助手。

```c#
public class GeneralClientBootstrap : AbstractBootstrap<GeneralClientBootstrap, IStrategy>
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.ClientCore -Version 1.0.0
```



### 示例

![](imgs/muti_donwload.png)

以下示例定义方法，包含GeneralClientBootstrap使用。

```c#
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
//开启驱动更新
//.Option(UpdateOption.Drive, true)
//开启遗言功能，需要部署GeneralUpdate.SystemService Windows服务。
.Option(UpdateOption.WillMessage, true)
.Strategy<WindowsStrategy>()
//注入一个func让用户决定是否跳过本次更新，如果是强制更新则不生效
//.SetCustomSkipOption(ShowCustomOption)
//注入一个自定义方法集合，该集合会在更新启动前执行。执行自定义方法列表如果出现任何异常，将通过异常订阅通知。（推荐在更新之前检查当前软件环境）
//.AddCustomOption(new List<Func<bool>>() { () => Check1(), () => Check2() })
//默认黑名单文件： { "Newtonsoft.Json.dll" } 默认黑名单文件扩展名： { ".patch", ".7z", ".zip", ".rar", ".tar" , ".json" }
//如果不需要扩展，需要重新传入黑名单集合来覆盖。
//.SetBlacklist(GetBlackFiles(), GetBlackFormats())
.LaunchTaskAsync();

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
/// 获取Windows平台所需的配置参数
/// </summary>
/// <returns></returns>
private Configinfo GetWindowsConfigInfo()
{
  //该对象用于主程序客户端与更新组件进程之间交互用的对象
  var config = new Configinfo();
  //本机的客户端程序应用地址
  config.InstallPath = @"D:\packet\source";
  //更新公告网页
  config.UpdateLogUrl = "https://www.baidu.com/";
  //客户端当前版本号
  config.ClientVersion = "1.1.1.1";
  //客户端类型：1.主程序客户端 2.更新组件
  config.AppType = AppType.UpgradeApp;
  //指定应用密钥，用于区分客户端应用
  config.AppSecretKey = "B8A7FADD-386C-46B0-B283-C9F963420C7C";
  //更新组件更新包下载地址
  config.UpdateUrl = $"{baseUrl}/versions/{config.AppType}/{config.ClientVersion}/{config.AppSecretKey}";
  //更新程序exe名称
  config.AppName = "GeneralUpdate.Core";
  //主程序客户端exe名称
  config.MainAppName = "GeneralUpdate.ClientCore";
  //主程序信息
  var mainVersion = "1.1.1.1";
  //主程序客户端更新包下载地址
  config.MainUpdateUrl = $"{baseUrl}/versions/{AppType.ClientApp}/{mainVersion}/{config.AppSecretKey}";
  return config;
}

/// <summary>
/// 获取Android平台所需要的参数
/// </summary>
/// <returns></returns>
private Configinfo GetAndroidConfigInfo()
{
  var config = new Configinfo();
  config.InstallPath = System.Threading.Thread.GetDomain().BaseDirectory;
  //主程序客户端当前版本号
  config.ClientVersion = "1.0.0.0"; //VersionTracking.Default.CurrentVersion.ToString();
  config.AppType = AppType.ClientApp;
  config.AppSecretKey = "41A54379-C7D6-4920-8768-21A3468572E5";
  //主程序客户端exe名称
  config.MainAppName = "GeneralUpdate.ClientCore";
  //主程序信息
  var mainVersion = "1.1.1.1";
  config.MainUpdateUrl = $"{baseUrl}/versions/{AppType.ClientApp}/{mainVersion}/{config.AppSecretKey}";
  return config;
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
  DispatchMessage($"{e.ProgressPercentage}%");
  //MyProgressBar.ProgressTo(e.ProgressValue, 100, Easing.Default);
}

private void OnException(object sender, ExceptionEventArgs e)
{
  //DispatchMessage(e.Exception.Message);
}

private void OnMultiAllDownloadCompleted(object sender, MultiAllDownloadCompletedEventArgs e)
{
  //e.FailedVersions; 如果出现下载失败则会把下载错误的版本、错误原因统计到该集合当中。
  DispatchMessage($"Is all download completed {e.IsAllDownloadCompleted}.");
}

private void OnMultiDownloadCompleted(object sender, MultiDownloadCompletedEventArgs e)
{
  var info = e.Version as VersionInfo;
  DispatchMessage($"{info.Name} download completed.");
}

private void OnMultiDownloadError(object sender, MultiDownloadErrorEventArgs e)
{
  var info = e.Version as VersionInfo;
  DispatchMessage($"{info.Name} error!");
}

private void DispatchMessage(string message)
{
    ShowMessage(message);
}
```



### 注解

GeneralClientBootstrap提供以下能力。

#### 构造函数

| Constructors             |                                    |
| ------------------------ | ---------------------------------- |
| GeneralClientBootstrap() | 当前GeneralUpdateBootstrap构造函数 |
| base:AbstractBootstrap() | 父类AbstractBootstrap构造函数      |

#### 属性

| Properties   |                                                            |
| ------------ | ---------------------------------------------------------- |
| Packet       | 更新包信息                                                 |
| UpdateOption | 更新操作配置设置枚举                                       |
| Configinfo   | 客户端相关参数类（AppType、AppName、AppSecretKey等字段）。 |

#### 方法

| Method                                 |                                                              |
| -------------------------------------- | ------------------------------------------------------------ |
| LaunchTaskAsync()                      | Task异步启动更新                                             |
| LaunchAsync()                          | 启动更新                                                     |
| SetBlacklist()                         | 设置更新文件黑名单，如果不需要更新文件 名则传入即可。        |
| Option()                               | 设置更新配置。                                               |
| Config()                               | 更新相关内容配置参数，url 服务器地址及 端口号, appSecretKey客户端唯一标识用于 区分产品分支。 |
| GetOption()                            | 获取更新配置。                                               |
| Strategy()                             | 设置当前更新策略，例如：如果是Windows 平台则使用WindowsStrategy， linux...mac...android以此类推。 |
| SetCustomSkipOption()                  | Let the user decide whether to update in the state of non-mandatory update. |
| AddCustomOption()                      | Add an asynchronous custom operation. In theory, any custom operation can be done. It is recommended to register the environment check method to ensure that there are normal dependencies and environments after the update is completed. |
| AddListenerMultiAllDownloadCompleted() | 完成所有的下载任务通知。                                     |
| AddListenerMultiDownloadProgress()     | 单个或多个更新包下载通知事件。                               |
| AddListenerMultiDownloadCompleted()    | 单个或多个更新包下载完成事件。                               |
| AddListenerMultiDownloadError()        | 监听每个版本下载异常的事件                                   |
| AddListenerMultiDownloadStatistics()   | 单个或多个更新包下载速度、剩余下载事 件、当前下载版本信息通知事件。 |
| AddListenerException()                 | 整个更新过程出现的任何问题都会通过这个回调函数通知。         |



### 🌴Packet

| 属性                                                         |
| ------------------------------------------------------------ |
| **MainUpdateUrl** string Update check api address.           |
| **AppType** int 1:ClientApp 2:UpdateApp                      |
| **UpdateUrl ** string Update check api address.              |
| **AppName**  string Need to start the name of the app.       |
| **MainAppName** string Need to start the name of the app.    |
| **Format** string Update package file format(Defult format is Zip). |
| **IsUpgradeUpdate** bool Whether an update is required to upgrade the application. |
| **IsMainUpdate** bool Whether the main application needs to be updated. |
| **UpdateLogUrl** string Update log web address.              |
| **UpdateVersions** List VersionInfo Version information that needs to be updated. |
| **Encoding** Encoding The encoding format for file operations. |
| **DownloadTimeOut** int Time-out event for file download.    |
| **AppSecretKey **string application key.                     |
| **ClientVersion** string Client current version.             |
| **LastVersion** string The latest version.                   |
| **InstallPath **string installation path (for update file logic). |
| **TempPath **string Download file temporary storage path (for update file logic). |
| **ProcessBase64** string Configuration parameters for upgrading the terminal program. |
| **Platform** string The platform to which the current strategy belongs. |
| **BlackFiles** List string Files in the blacklist will skip the update. |
| **BlackFormats** List string File formats in the blacklist will skip the update. |
| **DriveEnabled **bool Whether to enable the driver upgrade function. |
| **WillMessageEnabled** bool Whether open note function, if you want to start needs to be synchronized to deploy 'GeneralUpdate. SystemService' service. |



### 🌴Configinfo

| **属性**                                                     |
| ------------------------------------------------------------ |
| **AppType** int 1:ClientApp 2:UpdateApp                      |
| **AppName**  string Need to start the name of the app.       |
| **AppSecretKey **string application key.                     |
| **ClientVersion ** string Client current version.            |
| **UpdateUrl** string Update check api address.               |
| **UpdateLogUrl** string Update log web address.              |
| **InstallPath** string installation path (for update file logic). |
| **MainUpdateUrl** string  Update check api address.          |
| **MainAppName** string  Client app name.                     |



### 🍵UpdateOption

| **枚举**                                                     |
| ------------------------------------------------------------ |
| **Format** Update the file format of the package.            |
| **Encoding**  Compress encoding.                             |
| **MainApp** Main program name.                               |
| **DownloadTimeOut** Timeout period (unit: second). If this parameter is not specified, the default timeout period is 30 seconds. |
| **Drive **Whether to enable the driver upgrade function.     |
| **WillMessage** Whether open note function, if you want to start needs to be synchronized to deploy 'GeneralUpdate. SystemService' service. |



### 🌼LaunchTaskAsync()

**方法**

Task异步启动更新。

```c#
public Task<GeneralUpdateBootstrap> LaunchTaskAsync();
```



### 🌼LaunchAsync()

**方法**

启动更新。

```c#
public virtual TBootstrap LaunchAsync();
```



### 🌼SetBlacklist()

**方法**

设置更新时会忽略的黑名单信息，避免特殊文件二进制差分更新时无法使用导致更新失败。

```c#
public virtual TBootstrap SetBlacklist(List<string> files = null, List<string> fileFormats = null);
```



**参数类型**

```c#
List<string> 黑名单信息集合。
```



**参数**

```c#
files List<string> 黑名单文件名称集合。

fileFormats List<string> 黑名单文件后缀集合。
```



### 🌼Option()

**方法**

设置更新配置。

```c#
public virtual TBootstrap Option<T>(UpdateOption<T> option, T value);
```



**参数类型**

T 要设置更新操作UpdateOption。



**参数**

```c#
option UpdateOption<T> 配置动作枚举。

value T 需要设置的值，值类型根据UpdateOption枚举来。
```



### 🌼Config()

**方法**

Custom Configuration (Recommended : All platforms).

```c#
public GeneralClientBootstrap Config(Configinfo info);
public GeneralClientBootstrap Config(string url, string appSecretKey, string appName = "GeneralUpdate.Upgrade");
```



**参数类型**

Configinfo 

客户端相关参数类（AppType、AppName、AppSecretKey等字段）。



**参数**

**info** Configinfo 客户端相关参数类。

**url** string Remote server address.

**appSecretKey** string  application key(与服务端约定好的密钥，用于区分客户端进行版本管理或指定客户端推送升级).

**appName** string The updater name does not need to contain an extension.



### 🌼GetOption()

**方法**

```c#
public virtual T GetOption<T>(UpdateOption<T> option);
```

**参数类型**

T 

根据UpdateOption枚举获取结果。



**参数**

```c#
option UpdateOption<T> 具体枚举内容参考本文档中的 🍵UpdateOption。
```



### 🌼Strategy()

**方法**

指定更新策略。

```c#
public virtual TBootstrap Strategy<T>() where T : TStrategy, new();
```

**参数类型**

T 

设置符合当前操作系统的更新策略，例如：Windows操作系统使用WindowsStrategy。



### 🌼SetCustomSkipOption()

**方法**

Let the user decide whether to update in the state of non-mandatory update.

```c#
public GeneralClientBootstrap SetCustomSkipOption(Func<bool> func);
public GeneralClientBootstrap SetCustomSkipOption(Func<Task<bool>> func);
```



**参数类型**

```c#
Func<bool> 注入一个同步的自定义回调函数，通常用来做用户决定是否跳过本次版本更新。
Func<Task<bool>> 注入一个Task异步的自定义回调函数。
```



**参数**

```c#
func Func<bool>  注入一个同步的自定义回调函数，通常用来做用户决定是否跳过本次版本更新。
func Func<Task<bool>>  注入一个Task异步的自定义回调函数。
```



### 🌼AddCustomOption()

**方法**

Add an asynchronous custom operation. In theory, any custom operation can be done. It is recommended to register the environment check method to ensure that there are normal dependencies and environments after the update is completed.

```c#
public GeneralClientBootstrap AddCustomOption(List<Func<bool>> funcs);
public GeneralClientBootstrap AddCustomOption(List<Func<Task<bool>>> funcs);
```



**参数类型**

```c#
List<Func<bool>> 注入一组同步的带bool返回值的自定义回调函数。

List<Func<Task<bool>>> 注入一组Task异步的带bool返回值的自定义回调函数。
```



**参数**

```c#
funcs List<Func<bool>> 注入一组同步的自定义回调函数，通常用来做环境检查工作（例如：检查是否缺少VC++的环境库、或硬件设备驱动程序打印机、摄像头等）。

funcs List<Func<Task<bool>>> 注入一组Task异步的自定义回调函数。
```



### 🌼AddListenerMultiAllDownloadCompleted()

**方法**

```c#
public TBootstrap AddListenerMultiAllDownloadCompleted(Action<object, MultiAllDownloadCompletedEventArgs> callbackAction);
```



**参数类型**

**sender** object

操作句柄。

**args** MultiAllDownloadCompletedEventArgs 

所有版本下载完成通知参数。



**参数**

```c#
callbackAction Action<object, MultiAllDownloadCompletedEventArgs> 
```

监听所有更新版本下载完成的事件回传参数。



### 🌼AddListenerMultiDownloadProgress()

**方法**

```c#
public TBootstrap AddListenerMultiDownloadProgress(Action<object, MultiDownloadProgressChangedEventArgs> callbackAction);
```



**参数类型**

**sender** object 

操作句柄。

**args** MultiDownloadProgressChangedEventArgs 

进度通知参数。



**参数**

```c#
callbackAction Action<object, MultiDownloadProgressChangedEventArgs> 
```

监听每个版本下载进度事件回传参数。



### 🌼AddListenerMultiDownloadCompleted()

**方法**

```c#
public TBootstrap AddListenerMultiDownloadCompleted(Action<object, MultiDownloadCompletedEventArgs> callbackAction);
```



**参数类型**

sender object 

操作句柄。

MultiDownloadCompletedEventArgs

监听每个版本更新包下载完成回传参数。



**参数**

```c#
callbackAction Action<object, MultiDownloadCompletedEventArgs>
```

监听每个版本下载异常回传参数。



### 🌼AddListenerMultiDownloadError()

**方法**

```c#
public TBootstrap AddListenerMultiDownloadError(Action<object, MultiDownloadErrorEventArgs> callbackAction);
```



**参数类型**

**sender** object 

操作句柄。

**args** MultiDownloadErrorEventArgs

下载异常通知参数。



**参数**

```c#
callbackAction Action<object, MultiDownloadErrorEventArgs>
```

监听每个版本下载异常回传参数。



### 🌼AddListenerMultiDownloadStatistics()

**方法**

```c#
public TBootstrap AddListenerMultiDownloadStatistics(Action<object, MultiDownloadStatisticsEventArgs> callbackAction);
```



**参数类型**

**sender** object 

操作句柄。

**args** MultiDownloadStatisticsEventArgs

下载信息统计（下载速度、下载大小等）参数。



**参数**

```c#
callbackAction Action<object, MultiDownloadStatisticsEventArgs>
```

监听每个版本下载统计信息事件。



### 🌼AddListenerException()

**方法**

```c#
public TBootstrap AddListenerException(Action<object, ExceptionEventArgs> callbackAction);
```



**参数类型**

**sender** object 

操作句柄。

**args** ExceptionEventArgs

异常参数。



**参数**

```c#
callbackAction Action<object, ExceptionEventArgs>
```

监听更新组件内部的所有异常。



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
