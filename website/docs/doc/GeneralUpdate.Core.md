---
sidebar_position: 3
---

### 定义

命名空间：GeneralUpdate.Core

程序集：GeneralUpdate.Core.dll



GeneralUpdate.Core是最核心的组件之一，提供了大量主要功能。当主程序升级操作完成之后会通过进程 启动并传参的方式调用本组件来完成主程序升级操作。（主要职责更新主程序）

```c#
public class GeneralUpdateBootstrap : AbstractBootstrap<GeneralUpdateBootstrap, IStrategy>
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.Core -Version 1.0.0
```



### 示例

![](imgs/muti_donwload.png)

以下示例定义方法，包含GeneralUpdateBootstrap使用方法。

```c#
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

private static void OnMultiDownloadStatistics(object sender, MultiDownloadStatisticsEventArgs e)
{
    Console.WriteLine($" {e.Speed} , {e.Remaining.ToShortTimeString()}");
}

private static void OnMultiDownloadProgressChanged(object sender, MultiDownloadProgressChangedEventArgs e)
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

private static void OnMultiDownloadCompleted(object sender, MultiDownloadCompletedEventArgs e)
{
    //var info = e.Version as GeneralUpdate.Core.Domain.Entity.VersionInfo;
    //Console.WriteLine($"{info.Name} download completed.");
}

private static void OnMultiAllDownloadCompleted(object sender, MultiAllDownloadCompletedEventArgs e)
{
    Console.WriteLine($"AllDownloadCompleted {e.IsAllDownloadCompleted}");
}

private static void OnMultiDownloadError(object sender, MultiDownloadErrorEventArgs e)
{
    //var info = e.Version as GeneralUpdate.Core.Domain.Entity.VersionInfo;
    //Console.WriteLine($"{info.Name},{e.Exception.Message}.");
}

private static void OnException(object sender, ExceptionEventArgs e)
{
    Console.WriteLine($"{e.Exception.Message}");
}
```



### 注解

GeneralUpdateBootstrap提供以下能力。

#### 构造函数

| Constructors             |                                    |
| ------------------------ | ---------------------------------- |
| GeneralUpdateBootstrap() | 当前GeneralUpdateBootstrap构造函数 |
| base:AbstractBootstrap() | 父类AbstractBootstrap构造函数      |

#### 属性

| Properties   |                      |
| ------------ | -------------------- |
| Packet       | 更新包信息           |
| UpdateOption | 更新操作配置设置枚举 |

#### 方法

| Method                                 |                              |
| -------------------------------------- | ---------------------------- |
| LaunchTaskAsync()                      | Task异步启动更新             |
| LaunchAsync()                          | 启动更新                     |
| SetBlacklist()                         | 设置黑名单                   |
| Option()                               | 设置更新配置。               |
| GetOption()                            | 获取更新配置。               |
| Strategy()                             | 更新策略                     |
| AddListenerMultiAllDownloadCompleted() | 监听所有更新版本下载完成事件 |
| AddListenerMultiDownloadProgress()     | 监听每个版本下载的进度       |
| AddListenerMultiDownloadCompleted()    | 监听每个版本下载完成事件     |
| AddListenerMultiDownloadError()        | 监听每个版本下载异常的事件   |
| AddListenerMultiDownloadStatistics()   | 监听每个版本下载统计信息事件 |
| AddListenerException()                 | 监听更新组件内部的所有异常   |



### 🌴Packet

**属性**

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
| **WillMessageEnabled** bool 是否开启遗言功能，如果想要启动需要同步部署'GeneralUpdate. SystemService'服务。 |



### 🍵UpdateOption

**枚举**

**Format** 更新包的文件格式。

**Encoding**  压缩编码。

**MainApp** 主程序名称。

**DownloadTimeOut** 超时时间（单位：秒）。如果未指定此参数，则默认超时时间为30秒。

**Drive** 是否启用驱动升级功能。

**WillMessage** 是否开启遗言功能，如果想要启动需要同步部署'GeneralUpdate. SystemService'服务。



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



### 🌼GetOption()

**方法**

```c#
public virtual T GetOption<T>(UpdateOption<T> option);
```

**参数类型**

T 根据UpdateOption枚举获取结果。



**参数**

```c#
option  UpdateOption<T> 具体枚举内容参考本文档中的 🍵UpdateOption。
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

```
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
