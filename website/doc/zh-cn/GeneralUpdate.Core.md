## GeneralUpdate.Core

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

以下示例定义方法，

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

| Properties |            |
| ---------- | ---------- |
| Packet     | 更新包信息 |

#### 方法

| Method                                 |                               |
| -------------------------------------- | ----------------------------- |
| LaunchTaskAsync()                      | Task异步启动更新              |
| LaunchAsync()                          | 启动更新                      |
| SetBlacklist()                         | 设置黑名单                    |
| Option()                               | Setting update configuration. |
| GetOption()                            | Getting update configuration. |
| Strategy<T>()                          | 更新策略                      |
| AddListenerMultiAllDownloadCompleted() | 监听所有更新版本下载完成事件  |
| AddListenerMultiDownloadProgress()     | 监听每个版本下载的进度        |
| AddListenerMultiDownloadCompleted()    | 监听每个版本下载完成事件      |
| AddListenerMultiDownloadError()        | 监听每个版本下载异常的事件    |
| AddListenerMultiDownloadStatistics()   | 监听每个版本下载统计信息事件  |
| AddListenerException()                 | 监听所有异常                  |



### LaunchTaskAsync()

**参数**



### LaunchAsync()

**参数**



### SetBlacklist()

**参数**



### Option()

Setting update configuration.

```c#
public virtual TBootstrap Option<T>(UpdateOption<T> option, T value);
```

**参数类型**

T 要设置更新操作UpdateOption。

**参数**

**option** UpdateOption<T> Configuration Action Enumeration.

**value** T 需要设置的值，值类型根据UpdateOption枚举来.



### GetOption()

```c#
public virtual T GetOption<T>(UpdateOption<T> option);
```

**参数类型**

T 根据UpdateOption枚举获取结果。

**参数**

**option**  UpdateOption<T> 



### Strategy<T>()

```c#
Strategy<WindowsStrategy>();
```

**参数类型**

T 符合当前操作系统的更新策略，例如：Windows操作系统使用WindowsStrategy。



### AddListenerMultiAllDownloadCompleted()

**参数类型**

object Sender操作句柄。

MultiAllDownloadCompletedEventArgs 所有版本下载完成通知参数。

**参数**

**callbackAction** Action<object, MultiAllDownloadCompletedEventArgs> 监听所有更新版本下载完成的事件回调函数。



### AddListenerMultiDownloadProgress()

**参数类型**

object Sender操作句柄。

MultiDownloadProgressChangedEventArgs 进度通知参数。

**参数**

**callbackAction** Action<object, MultiDownloadProgressChangedEventArgs> 监听每个版本下载进度事件回调函数。



### AddListenerMultiDownloadCompleted()

**参数**

**callbackAction** Action<object, MultiDownloadCompletedEventArgs>



### AddListenerMultiDownloadError()

**参数**

**callbackAction** Action<object, MultiDownloadErrorEventArgs>



### AddListenerMultiDownloadStatistics()

**参数**

**callbackAction** Action<object, MultiDownloadStatisticsEventArgs>



### AddListenerException()

**参数**

**callbackAction** Action<object, ExceptionEventArgs>



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |

