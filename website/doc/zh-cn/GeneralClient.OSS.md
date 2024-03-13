## GeneralUpdate.OSS

### 定义

命名空间：GeneralUpdate.ClientCore

程序集：GeneralUpdate.ClientCore.dll



**OSS**

OSS的全称是对象存储服务（Object Storage Service），做服务端技术栈开发的小伙伴肯定对这个不陌生在 各大云服务器厂商都会提供类似的服务,说简单点就是一个文件服务器。例如：阿里云OSS、腾讯云COS、华 为云OBS，其实它们只是名字不一样功能服务都差不多。然后本更新组件新功能的实现思路非常相似那么就 选择了OSS来为该功能命名，而且方便开发者一眼能get到这个功能的作用（需要使用组件 GeneralUpdate.ClientCore、GeneralUpdate.Core）。



**GeneralClientOSS**

GeneralClientOSS类是另外一种更新机制与GeneralClientBootstrap不同的是，GeneralClientOSS不需要编写服务端的代码直接根据OSS文件服务器的version.josn的版本信息进行更新，简化了新手使用本组件的门槛。

```c#
public sealed class GeneralClientOSS
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.ClientCore -Version 1.0.0
```



### 示例

#### OSS流程

<img src="../imgs/oss_flow.png" alt="oss_flow" style="zoom:50%;" />

1.准备version.json版本信息配置文件，更新文件（update.zip）更新文件和之前的打包方式一样。

<img src="../imgs/oss_version_json.png" alt="oss_version_json" style="zoom:50%;" />

2.Client启动时直接请求OSS服务器或文件服务器，下载version.json文件。 

<img src="../imgs/oss_util.png" alt="oss_util" style="zoom:50%;" />

3.下载到本地之后解析版本信息内容判断是否需要更新，如果将信息通过进程启动传递Upgrade（Client自我 关闭）。 

4.Upgrade启动之后直接去下载update.zip，下载到本地之后直接解压覆盖本地文件。 

5.Upgrade更新完成之后把Client启动起来，自我关闭。更新结束。 GeneralUpdateOSS的功能和GeneralUpdateBootstrap功能对比来说，使用的门槛非常低如果公司对自动 更新的要求不高的话可以使用这个功能。一句话概括这个功能就是下载version.json根据文件里的内容去逐 版本下载更新包，下载下来之后直接解压更新就结束了。



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
public static async Task Start(ParamsOSS configParams, string upgradeAppName = "GeneralUpdate.Upgrade");
```



**参数类型**

ParamsOSS 更新配置参数，详细内容参考本文档  <img src="D:\github_project\GeneralUpdate-Samples\website\doc\imgs\property.png" alt="property" style="zoom:12%;" />ParamsOSS。



**参数**

**parameter** ParamsOSS安卓平台的跟新配置参数。

**upgradeAppName** 需要升级的应用名称。



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