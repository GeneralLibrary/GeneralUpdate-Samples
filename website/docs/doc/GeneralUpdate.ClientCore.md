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

GeneralClientBootstrap uses code examples [[View]](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/Program.cs)。

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

Certainly! Here's the translated content:

| **Attribute**        | Type   | Notes                                                        |
| -------------------- | ------ | ------------------------------------------------------------ |
| UpdateUrl            | string | API address for update checks.                               |
| ReportUrl            | string | API address for reporting update status.                     |
| AppName              | string | Name of the application that needs to be launched.           |
| MainAppName          | string | Name of the main application that needs to be launched.      |
| UpdateLogUrl         | string | Web address for the update log.                              |
| AppSecretKey         | string | Application secret key, agreed upon with the server for authentication and branching. |
| ClientVersion        | string | Current version number of the client.                        |
| UpgradeClientVersion | string | Current version number of the upgrade client.                |
| InstallPath          | string | Installation path (used for update file logic).              |
| BlackFiles           | List   | Files in the blacklist will be skipped during updates.       |
| BlackFormats         | List   | File formats in the blacklist will be skipped during updates. |
| SkipDirectorys       | List   | Directory paths to be skipped that do not require updates.   |
| ProductId            | string | Unique ID of the current product branch.                     |
| Bowl                 | string | Path to the Bowl monitoring process, which starts after updates to check if the Bowl client starts normally. If an exception occurs after starting, the exception information will be captured. |
| Scheme               | string | Used for passing a token in HTTP requests for authentication. |
| Token                | string | Token used in HTTP requests for authentication.              |

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
