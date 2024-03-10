## GeneralUpdate.Maui.OSS

### 定义

命名空间：GeneralUpdate.Maui.OSS

程序集：GeneralUpdate.Maui.OSS.dll



GeneralUpdate.Maui.OSS组件用来实现.NET MAUI更新的机制，目前只支持安卓平台的自动升级。

```c#
public sealed class GeneralUpdateOSS
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.Maui.OSS -Version 1.0.0
```



### 示例

以下示例定义方法，包含GeneralUpdateOSS使用方法。

```c#
Task.Run(async () =>
{
    var url = "http://192.168.50.203";
    var apk = "com.companyname.generalupdate.ossclient.apk";
    var authority = "com.generalupdate.oss.fileprovider";
    var currentVersion = "1.0.0.0";
    var versionFileName = "version.json";
    GeneralUpdateOSS.AddListenerDownloadProcess(OnOSSDownload);
    GeneralUpdateOSS.AddListenerException(OnException);
    await GeneralUpdateOSS.Start<Strategy>(new ParamsAndroid(url, apk, authority, currentVersion, versionFileName));
});
```



### 注解

GeneralUpdateOSS提供以下能力。

#### 属性

| Properties    |                          |
| ------------- | ------------------------ |
| ParamsAndroid | 安卓平台的跟新配置参数。 |



#### 方法

| Method                       |                                              |
| ---------------------------- | -------------------------------------------- |
| Start()                      | Starting an OSS update for android platform. |
| AddListenerDownloadProcess() | Monitor download progress.                   |
| AddListenerException()       | Listen for internal exception information.   |



### <img src="../imgs/property.png" alt="property" style="zoom:12%;" />ParamsAndroid

**属性**

**Url** string http的api服务端请求地址。

**Apk** string 安卓apk名称。

**CurrentVersion** string 当前版本。

**Authority** string  apk作者。

**VersionFileName** string 版本文件名称。



### <img src="../imgs/func.png" alt="func" style="zoom:10%;" />Start()

**方法**

Starting an OSS update for android platform.

```c#
public static async Task Start<TStrategy>(ParamsAndroid parameter) where TStrategy : AbstractStrategy, new();
```



**参数类型**

ParamsAndroid 安卓平台的跟新配置参数，详细内容参考本文档  <img src="../imgs/property.png" alt="property" style="zoom:12%;" />ParamsAndroid。



**参数**

**parameter** ParamsAndroid 安卓平台的跟新配置参数。



### <img src="../imgs/func.png" alt="func" style="zoom:10%;" />AddListenerDownloadProcess()

**方法**

Monitor download progress.

```c#
public static void AddListenerDownloadProcess(Action<object, OSSDownloadArgs> callbackAction);
```



**参数类型**

**sender** object 

操作句柄。

**args** OSSDownloadArgs

下载进度通知参数。



**参数**

**callbackAction** Action<object, OSSDownloadArgs> 

监听当前版本下载进度事件回传参数。



### <img src="../imgs/func.png" alt="func" style="zoom:10%;" />AddListenerException()

**方法**

Listen for internal exception information.

```c#
public static void AddListenerException(Action<object, ExceptionEventArgs> callbackAction);
```



**参数类型**

**sender** object 

操作句柄。

**args** ExceptionEventArgs

更新异常参数。



**参数**

**callbackAction** Action<object, ExceptionEventArgs> 

监听当前版本下载进度事件回传参数。



### 适用于

| 产品      | 版本             |
| --------- | ---------------- |
| .NET      | 7、8、9          |
| .NET MAUI | .NET 8 - Android |