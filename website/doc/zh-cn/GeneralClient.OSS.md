## GeneralUpdate.OSS

### 定义

命名空间：GeneralUpdate.ClientCore

程序集：GeneralUpdate.ClientCore.dll



GeneralClientOSS类是另外一种更新机制与GeneralClientBootstrap不同的是，GeneralClientOSS不需要编写服务端的代码直接根据OSS文件服务器的version.josn的版本信息进行更新，简化了新手使用本组件的门槛。

```c#
public sealed class GeneralClientOSS
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.ClientCore -Version 1.0.0
```



### 示例

以下示例定义方法，包含GeneralClientOSS使用方法。

```c#
Task.Run(async () =>
{
    var url = "http://192.168.50.203";
    var apk = "com.companyname.generalupdate.ossclient.apk";
    var authority = "com.generalupdate.oss.fileprovider";
    var currentVersion = "1.0.0.0";
    var versionFileName = "version.json";
    GeneralClientOSS.AddListenerDownloadProcess(OnOSSDownload);
    GeneralClientOSS.AddListenerException(OnException);
    await GeneralClientOSS.Start<Strategy>(new ParamsOSS(url, apk, authority, currentVersion, versionFileName));
});
```



### 注解

GeneralUpdateOSS提供以下能力。

#### 属性

| Properties |                   |
| ---------- | ----------------- |
| ParamsOSS  | OSS更新配置参数。 |



#### 方法

| Method                 |                                            |
| ---------------------- | ------------------------------------------ |
| Start()                | Starting an OSS update .                   |
| AddListenerException() | Listen for internal exception information. |



### <img src="D:\github_project\GeneralUpdate-Samples\website\doc\imgs\property.png" alt="property" style="zoom:12%;" />ParamsOSS

**属性**

**Url** string http的api服务端请求地址。

**AppName** string 应用名称。

**CurrentVersion** string 当前版本。

**VersionFileName** string 版本文件名称。



### <img src="D:\github_project\GeneralUpdate-Samples\website\doc\imgs\func.png" alt="func" style="zoom:10%;" />Start()

**方法**

Starting an OSS update.

```c#
public static async Task Start(ParamsOSS configParams, string upgradeAppName = "GeneralUpdate.Upgrade")
```



**参数类型**

ParamsOSS 更新配置参数，详细内容参考本文档  <img src="D:\github_project\GeneralUpdate-Samples\website\doc\imgs\property.png" alt="property" style="zoom:12%;" />ParamsOSS。



**参数**

**parameter** ParamsOSS安卓平台的跟新配置参数。



### <img src="D:\github_project\GeneralUpdate-Samples\website\doc\imgs\func.png" alt="func" style="zoom:10%;" />AddListenerDownloadProcess()

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



### <img src="D:\github_project\GeneralUpdate-Samples\website\doc\imgs\func.png" alt="func" style="zoom:10%;" />AddListenerException()

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

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |