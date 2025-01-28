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



### 示例

GeneralClientBootstrap使用代码示例 [进入](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/Program.cs)。

![](imgs/muti_donwload.png)



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



### 🌴Configinfo

| **属性**             | 类型   | 注释                                                         |
| -------------------- | ------ | ------------------------------------------------------------ |
| UpdateUrl            | string | 更新检查api地址。                                            |
| ReportUrl            | string | 上报更新状态api地址。                                        |
| AppName              | string | 需要启动应用程序的名称。                                     |
| MainAppName          | string | 需要启动主应用程序的名称。                                   |
| UpdateLogUrl         | string | 更新日志网页地址。                                           |
| AppSecretKey         | string | 应用程序密钥，需要和服务端约定好验证身份和分支。             |
| ClientVersion        | string | 客户端当前版本号。                                           |
| UpgradeClientVersion | string | 升级端当前版本号。                                           |
| InstallPath          | string | 安装路径（用于更新文件逻辑）。                               |
| BlackFiles           | List   | 黑名单中的文件将跳过更新。                                   |
| BlackFormats         | List   | 黑名单中的文件格式将跳过更新。                               |
| SkipDirectorys       | List   | 跳过不需要更新的文件夹目录。                                 |
| ProductId            | string | 当前产品分支的唯一id。                                       |
| Bowl                 | string | Bowl监控进程路径，更新完成之后启动Bowl监控客户端是否正常启动。如果启动后异常会捕捉异常信息。 |
| Scheme               | string | 用于HTTP请求传入token进行身份验证。                          |
| Token                | string | 用于HTTP请求传入token进行身份验证。                          |



### 🍵UpdateOption

| **枚举**                                                     |
| ------------------------------------------------------------ |
| **Format** 更新包的文件格式。                                |
| **Encoding**  压缩编码。                                     |
| **DownloadTimeOut** 超时时间（单位：秒）。如果未指定此参数，则默认超时时间为30秒。 |



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
