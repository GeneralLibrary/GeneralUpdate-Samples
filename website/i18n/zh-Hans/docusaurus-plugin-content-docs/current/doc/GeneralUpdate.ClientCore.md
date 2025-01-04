---
sidebar_position: 4
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
NuGet\Install-Package GeneralUpdate.ClientCore -Version 9.1.0
```



### 示例

![](imgs/muti_donwload.png)

以下示例定义方法，包含GeneralClientBootstrap使用。

```c#
try
{
      Console.WriteLine($"主程序初始化，{DateTime.Now}！");
      Console.WriteLine("当前运行目录：" + Thread.GetDomain().BaseDirectory);
      await Task.Delay(2000);
      var configinfo = new Configinfo
      {
            //configinfo.UpdateLogUrl = "https://www.****.com";
            ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
            UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
            AppName = "GeneralUpdate.Upgrad.exe",
            MainAppName = "GeneralUpdate.Client.exe",
            InstallPath = Thread.GetDomain().BaseDirectory,
            //configinfo.Bowl = "Generalupdate.CatBowl.exe";
            //当前客户端的版本号
            ClientVersion = "1.0.0.0",
            //当前升级端的版本号
            UpgradeClientVersion = "1.0.0.0",
            //产品id
            ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c***",
            //应用密钥
            AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9*****"
       };
       _ = await new GeneralClientBootstrap() //单个或多个更新包下载通知事件
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
       .SetConfig(configinfo)
       .Option(UpdateOption.DownloadTimeOut, 60)
       .Option(UpdateOption.Encoding, Encoding.UTF8)
       .LaunchAsync();
       Console.WriteLine($"主程序已启动，{DateTime.Now}！");
}
catch (Exception e)
{
     Console.WriteLine(e.Message + "\n" + e.StackTrace);
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
  //e.TotalBytesToReceive 当前更新包需要下载的总大小
  //e.ProgressValue 当前进度值
  //e.ProgressPercentage 当前进度的百分比
  //e.Version 当前下载的版本信息
  //e.BytesReceived 已下载大小
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
```



### 注解

GeneralClientBootstrap提供以下能力。

#### 属性

| Properties   |                                                            |
| ------------ | ---------------------------------------------------------- |
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
| SetCustomSkipOption()                  | 让用户在非强制更新的状态下决定是否进行更新。                 |
| AddCustomOption()                      | 添加一个异步的自定义操作。理论上，任何自定义操作都可以完成。建议注册环境检查方法，以确保更新完成后存在正常的依赖和环境。 |
| AddListenerMultiAllDownloadCompleted() | 完成所有的下载任务通知。                                     |
| AddListenerMultiDownloadCompleted()    | 单个或多个更新包下载完成事件。                               |
| AddListenerMultiDownloadError()        | 监听每个版本下载异常的事件                                   |
| AddListenerMultiDownloadStatistics()   | 单个或多个更新包下载速度、剩余下载事 件、当前下载版本信息通知事件。 |
| AddListenerException()                 | 整个更新过程出现的任何问题都会通过这个回调函数通知。         |



### 🌴Packet

| 属性                                                         |
| ------------------------------------------------------------ |
| **MainUpdateUrl** string 更新检查api地址。                   |
| **AppType** int 1:ClientApp 2:UpdateApp                      |
| **UpdateUrl** string Update 更新检查api地址。                |
| **AppName**  string 需要启动应用程序的名称。                 |
| **MainAppName** string 需要启动主应用程序的名称。            |
| **Format** string 更新包文件格式（默认格式为Zip）。          |
| **IsUpgradeUpdate** bool 是否需要更新来升级应用程序。        |
| **IsMainUpdate** bool 主应用程序是否需要更新。               |
| **UpdateLogUrl** string 更新日志网页地址。                   |
| **UpdateVersions** List 需要更新的版本信息VersionInfo。      |
| **Encoding** Encoding 文件操作的编码格式。                   |
| **DownloadTimeOut** int 下载超时时间。                       |
| **AppSecretKey** string 应用程序密钥，需要和服务器约定好。   |
| **ClientVersion** string 客户端当前版本号。                  |
| **LastVersion** string 最新版本号。                          |
| **InstallPath** string 安装路径（用于更新文件逻辑）。        |
| **TempPath** string 下载文件临时存储路径（用于更新文件逻辑）。 |
| **ProcessBase64** string 升级终端程序的配置参数。            |
| **Platform** string 当前策略所属的平台。（Windows\linux\Mac） |
| **BlackFiles** List 黑名单中的文件将跳过更新。               |
| **BlackFormats** 黑名单中的文件格式将跳过更新。              |
| **DriveEnabled** bool 是否启用驱动升级功能。                 |

### 🌴Configinfo

| **属性**                                              |
| ----------------------------------------------------- |
| **AppType** int 1:ClientApp 2:UpdateApp               |
| **AppName**  string 需要启动应用程序的名称。          |
| **AppSecretKey** string 应用程序密钥。                |
| **ClientVersion** string 客户端当前版本。             |
| **UpdateUrl** string 更新检查api地址。                |
| **UpdateLogUrl** string 更新日志网页地址。            |
| **InstallPath** string 安装路径（用于更新文件逻辑）。 |
| **MainUpdateUrl** string  更新检查api地址。           |
| **MainAppName** string  主客户端应用名称              |



### 🍵UpdateOption

| **枚举**                                                     |
| ------------------------------------------------------------ |
| **Format** 更新包的文件格式。                                |
| **Encoding**  压缩编码。                                     |
| **DownloadTimeOut** 超时时间（单位：秒）。如果未指定此参数，则默认超时时间为30秒。 |



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

**url** string 远程服务器地址。

**appSecretKey** string  application key(与服务端约定好的密钥，用于区分客户端进行版本管理或指定客户端推送升级).

**appName** string 更新程序的名称不需要包含扩展名。



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



### 🌼SetCustomSkipOption()

**方法**

让用户决定是否在非强制更新状态下进行更新。

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

添加一个异步的自定义操作。理论上，任何自定义操作都可以完成。建议注册环境检查方法，以确保更新完成后存在正常的依赖和环境。

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
