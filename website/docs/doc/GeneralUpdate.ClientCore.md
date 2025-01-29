---
sidebar_position: 4
---

### Definition

Namespace: GeneralUpdate.ClientCore

Assembly: GeneralUpdate.ClientCore.dll



GeneralUpdate.ClientCore is one of the core components, offering a wide range of primary functionalities. Its essence is similar to Core, but it has a different role: ClientCore is used in the main program, where it assists in updates and upgrades and then closes the main program to launch the upgrade assistant.

```c#
public class GeneralClientBootstrap : AbstractBootstrap<GeneralClientBootstrap, IStrategy>
```



### Example

GeneralClientBootstrap uses code examples [View](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/Program.cs)。

![](imgs/muti_donwload.png)



### Annotations

GeneralClientBootstrap provides the following capabilities.

#### Properties

| Properties   | Description                                                  |
| ------------ | ------------------------------------------------------------ |
| UpdateOption | Enum for update operation configuration settings             |
| Configinfo   | Client-related parameter class (fields like AppType, AppName, AppSecretKey, etc.) |

#### Methods

| Method                                 | Description                                                  |
| -------------------------------------- | ------------------------------------------------------------ |
| LaunchTaskAsync()                      | Task-based asynchronous update launch                        |
| LaunchAsync()                          | Launch update                                                |
| SetBlacklist()                         | Set blacklist for update files; pass in if certain files should not be updated. |
| Option()                               | Set update configuration.                                    |
| Config()                               | Configure update-related parameters, such as server URL and port, appSecretKey for client identification and product branching. |
| GetOption()                            | Get update configuration.                                    |
| SetCustomSkipOption()                  | Allow users to decide whether to proceed with an update in non-mandatory update scenarios. |
| AddCustomOption()                      | Add an asynchronous custom operation. In theory, any custom operation can be completed. It's recommended to register environment check methods to ensure dependencies and environment are intact after the update. |
| AddListenerMultiAllDownloadCompleted() | Notification for the completion of all download tasks.       |
| AddListenerMultiDownloadCompleted()    | Event for the completion of single or multiple update package downloads. |
| AddListenerMultiDownloadError()        | Listen for errors during each version download               |
| AddListenerMultiDownloadStatistics()   | Notification for download speed, remaining download time, and current download version info. |
| AddListenerException()                 | Notification for any issues during the entire update process. |

### 🌴Packet

| Property                                                     |
| ------------------------------------------------------------ |
| **MainUpdateUrl** string Main update check API address.      |
| **AppType** int 1:ClientApp 2:UpdateApp                      |
| **UpdateUrl** string Update check API address.               |
| **AppName** string Name of the application to be launched.   |
| **MainAppName** string Name of the main application to be launched. |
| **Format** string Update package file format (default is Zip). |
| **IsUpgradeUpdate** bool Indicates if the update is needed to upgrade the application. |
| **IsMainUpdate** bool Indicates if the main application needs an update. |
| **UpdateLogUrl** string URL for the update log webpage.      |
| **UpdateVersions** List Version information that needs updating. |
| **Encoding** Encoding File operation encoding format.        |
| **DownloadTimeOut** int Download timeout duration.           |
| **AppSecretKey** string Application secret key, agreed upon with the server. |
| **ClientVersion** string Current client version.             |
| **LastVersion** string Latest version.                       |
| **InstallPath** string Installation path (used for update file logic). |
| **TempPath** string Temporary storage path for downloaded files (used for update file logic). |
| **ProcessBase64** string Configuration parameters for the upgrade terminal program. |
| **Platform** string Platform to which the current strategy belongs (Windows\Linux\Mac). |
| **BlackFiles** List Files in the blacklist will be skipped during updates. |
| **BlackFormats** File formats in the blacklist will be skipped during updates. |
| **DriveEnabled** bool Indicates if the driver upgrade feature is enabled. |



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

| Enum                                                         |
| ------------------------------------------------------------ |
| **Format** File format of the update package.                |
| **Encoding** Compression encoding.                           |
| **DownloadTimeOut** Timeout duration (in seconds). If not specified, the default timeout is 30 seconds. |



### Applicable To

| Product        | Version       |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
