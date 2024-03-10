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

<img src="../imgs/oss_maui_flow.png" alt="oss_maui_flow" style="zoom:50%;" />

1.准备version.json版本信息配置文件，更新文件（update.apk）更新文件就直接是新版本的apk了 (或.abb)。 

2.Client启动时直接请求OSS服务器或文件服务器，下载version.json文件。

```json
{ 
    "PubTime": 1680444916,
    "Name": "com.companyname.generalupdate.ossclient",
    "MD5": "9bf414990a67e74f11752d03f49b15d8", 
    "Version": "1.0.5", 
    "Url": "http://192.168.50.203/com.companyname.generalupdate.ossclient.apk" 
} 
```

3.下载到本地之后解析版本信息内容，判断是否需要更新。 4.需要更新则下载update.apk。 5.下载完成之后执行安装，这一步就交给了安卓操作系统执行。执行完成之后运行新版本app。



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