---
sidebar_position: 5
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
NuGet\Install-Package GeneralUpdate.Core -Version 9.1.2
```



### 示例

![](imgs/muti_donwload.png)

以下示例定义方法，包含GeneralUpdateBootstrap使用方法。

```c#
try
{
     Console.WriteLine($"升级程序初始化，{DateTime.Now}！");
     Console.WriteLine("当前运行目录：" + Thread.GetDomain().BaseDirectory);
     await Task.Delay(2000);
     await new GeneralUpdateBootstrap()
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
}
catch (Exception e)
{
     Console.WriteLine(e.Message + "\n" + e.StackTrace);
}

private static void OnMultiDownloadStatistics(object sender, MultiDownloadStatisticsEventArgs e)
{
    Console.WriteLine($" {e.Speed} , {e.Remaining.ToShortTimeString()}");
    Console.WriteLine($" {Math.Round(e.ProgressValue * 100, 2)}% ， Receivedbyte：{e.BytesReceived}M ，Totalbyte：{e.TotalBytesToReceive}M");
}

private static void OnMultiDownloadCompleted(object sender, MultiDownloadCompletedEventArgs e)
{
    //var info = e.Version as VersionInfo;
    //Console.WriteLine($"{info.Name} download completed.");
}

private static void OnMultiAllDownloadCompleted(object sender, MultiAllDownloadCompletedEventArgs e)
{
    Console.WriteLine($"AllDownloadCompleted {e.IsAllDownloadCompleted}");
}

private static void OnMultiDownloadError(object sender, MultiDownloadErrorEventArgs e)
{
    //var info = e.Version as VersionInfo;
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

| Method                                 |                                       |
| -------------------------------------- | ------------------------------------- |
| LaunchTaskAsync()                      | Task异步启动更新                      |
| LaunchAsync()                          | 启动更新                              |
| Option()                               | 设置更新配置。                        |
| GetOption()                            | 获取更新配置。                        |
| AddListenerMultiAllDownloadCompleted() | 监听所有更新版本下载完成事件          |
| AddListenerMultiDownloadCompleted()    | 监听每个版本下载完成事件              |
| AddListenerMultiDownloadError()        | 监听每个版本下载异常的事件            |
| AddListenerMultiDownloadStatistics()   | 监听每个版本下载统计信息\下载进度事件 |
| AddListenerException()                 | 监听更新组件内部的所有异常            |



### 🍵UpdateOption

**枚举**

**Drive** 是否启用驱动升级功能。



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
