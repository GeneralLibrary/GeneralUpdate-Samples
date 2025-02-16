---
sidebar_position: 9
---

### 定义

命名空间：GeneralUpdate.ClientCore

程序集：GeneralUpdate.ClientCore.dll



**OSS**

OSS的全称是对象存储服务（Object Storage Service），做服务端技术栈开发的小伙伴肯定对这个不陌生在 各大云服务器厂商都会提供类似的服务,说简单点就是一个文件服务器。例如：阿里云OSS、腾讯云COS、华为云OBS，其实它们只是名字不一样功能服务都差不多。然后本更新组件新功能的实现思路非常相似那么就 选择了OSS来为该功能命名，而且方便开发者一眼能get到这个功能的作用（需要使用组件 GeneralUpdate.ClientCore、GeneralUpdate.Core）。



**GeneralClientOSS**

GeneralClientOSS类是另外一种更新机制与GeneralClientBootstrap不同的是，GeneralClientOSS不需要编写服务端的代码直接根据OSS文件服务器的version.josn的版本信息进行更新，简化了新手使用本组件的门槛。

```c#
public sealed class GeneralClientOSS
```



### 示例

| 说明                      | 地址                                                         |
| ------------------------- | ------------------------------------------------------------ |
| 代码示例地址              | [进入](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/OSS) |
| 哈希算法（sha256）        | [进入](https://github.com/GeneralLibrary/GeneralUpdate/blob/master/src/c%23/GeneralUpdate.Core/Pipeline/HashMiddleware.cs) |
| 测试脚本（oss_start.cmd） | [进入](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src) |
| versions.json 制作工具    | [进入](https://github.com/GeneralLibrary/GeneralUpdate.Tools) |



#### OSS流程

![](imgs/oss_flow.png)

1.准备version.json版本信息配置文件，更新文件（update.zip）更新文件和之前的打包方式一样。

```json
[
  {
    "PacketName": "packet_20250102230201638_1.0.0.1",
    "Hash": "ad1a85a9169ca0083ab54ba390e085c56b9059efc3ca8aa1ec9ed857683cc4b1",
    "Version": "1.0.0.1",
    "Url": "http://localhost:5000/packages/packet_20250102230201638_1.0.0.1.zip",
    "PubTime": "2025-01-02T23:48:21"
  }
]
```

2.Client启动时直接请求OSS服务器或文件服务器，下载version.json文件。 

![](imgs/oss_util.png)

3.下载到本地之后解析版本信息内容判断是否需要更新，如果将信息通过进程启动传递Upgrade（Client自我 关闭）。 

4.Upgrade启动之后直接去下载update.zip，下载到本地之后直接解压覆盖本地文件。 

5.Upgrade更新完成之后把Client启动起来，自我关闭。更新结束。 GeneralUpdateOSS的功能和GeneralUpdateBootstrap功能对比来说，使用的门槛非常低如果公司对自动 更新的要求不高的话可以使用这个功能。一句话概括这个功能就是下载version.json根据文件里的内容去逐 版本下载更新包，下载下来之后直接解压更新就结束了。

6.由于OSS功能没有包含黑名单机制，制作update.zip时需要注意请勿将以下组件内部使用的文件添加到包内，或直接发布成single file避免占用降低更新复杂度。

```json
"Microsoft.Bcl.AsyncInterfaces.dll",
"System.Collections.Immutable.dll", 
"System.IO.Pipelines.dll", 
"System.Text.Encodings.Web.dll",
"System.Text.Json.dll"
```



以下示例定义方法，包含GeneralClientOSS、GeneralUpdateOSS 使用示例：

GeneralClientOSS：

```c#
using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Shared.Object;

var paramsOSS = new GlobalConfigInfoOSS
{
    Url = "http://localhost:5000/packages/versions.json",
    CurrentVersion = "1.0.0.0",
    VersionFileName = "versions.json",
    AppName = "OSSClientSample.exe",
    Encoding = Encoding.UTF8.WebName
};

await GeneralClientOSS.Start(paramsOSS, "OSSUpgradeSample.exe");
```



GeneralUpdateOSS：

```
using GeneralUpdate.Core;

/*
 * GeneralUpdateOSS will by default read the JSON content of GlobalConfigInfoOSS stored in the system environment variables by GeneralClientOSS
 * , and developers do not need to be concerned with the entire process.
 * 
 * Environment.GetEnvironmentVariable("GlobalConfigInfoOSS", EnvironmentVariableTarget.User);
 * 
 * Typically, GeneralClientOSS and GeneralUpdateOSS appear as a pair.
 */
try
{
    await GeneralUpdateOSS.Start();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |