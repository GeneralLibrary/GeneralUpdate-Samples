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

NuGet Installation

```shell
NuGet\Install-Package GeneralUpdate.ClientCore -Version 9.1.2
```

### Example

![](imgs/muti_donwload.png)

The following example defines methods involving the use of GeneralClientBootstrap.

```c#
try
{
      Console.WriteLine($"Main program initialization, {DateTime.Now}!");
      Console.WriteLine("Current running directory: " + Thread.GetDomain().BaseDirectory);
      await Task.Delay(2000);
      var configinfo = new Configinfo
      {
            //configinfo.UpdateLogUrl = "https://www.****.com";
            ReportUrl = "http://127.0.0.1:5000/Upgrade/Report",
            UpdateUrl = "http://127.0.0.1:5000/Upgrade/Verification",
            AppName = "GeneralUpdate.Upgrad.exe",
            MainAppName = "GeneralUpdate.Client.exe",
            InstallPath = Thread.GetDomain().BaseDirectory,
            //configinfo.Bowl = "Generalupdate.CatBowl.exe";
            //Current client version
            ClientVersion = "1.0.0.0",
            //Current upgrade client version
            UpgradeClientVersion = "1.0.0.0",
            //Product ID
            ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c***",
            //Application secret key
            AppSecretKey = "dfeb5833-975e-4afb-88f1-6278ee9*****"
       };
       _ = await new GeneralClientBootstrap() //Notification for single or multiple update package downloads
       //Notification for download speed, remaining download time, and current download version info
       .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
       //Completion of single or multiple update package downloads
       .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
       //Notification for the completion of all download tasks
       .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
       //Notification for exceptions during the download process
       .AddListenerMultiDownloadError(OnMultiDownloadError)
       //Notification for any issues during the entire update process
       .AddListenerException(OnException)
       .SetConfig(configinfo)
       .Option(UpdateOption.DownloadTimeOut, 60)
       .Option(UpdateOption.Encoding, Encoding.UTF8)
       .LaunchAsync();
       Console.WriteLine($"Main program started, {DateTime.Now}!");
}
catch (Exception e)
{
     Console.WriteLine(e.Message + "\n" + e.StackTrace);
}

private List<string> GetBlackFiles()
{
  var blackFiles = new List<string>();
  blackFiles.Add("MainApp");
  return blackFiles;
}

private List<string> GetBlackFormats()
{
  var blackFormats = new List<string>();
  blackFormats.Add(".zip");
  return blackFormats;
}

/// <summary>
/// Allow users to decide whether to skip this update
/// </summary>
/// <returns></returns>
private async Task<bool> ShowCustomOption()
{
  return await Task.FromResult(true);
}

private void OnMultiDownloadStatistics(object sender, MultiDownloadStatisticsEventArgs e)
{
  //e.Remaining Remaining download time
  //e.Speed Download speed
  //e.Version Current download version info
  //e.TotalBytesToReceive Total size for the current update package
  //e.ProgressValue Current progress value
  //e.ProgressPercentage Current progress percentage
  //e.Version Current download version info
  //e.BytesReceived Downloaded size
}

private void OnException(object sender, ExceptionEventArgs e)
{
  //DispatchMessage(e.Exception.Message);
}

private void OnMultiAllDownloadCompleted(object sender, MultiAllDownloadCompletedEventArgs e)
{
  //e.FailedVersions; If download fails, the failed version and error reason will be recorded in this collection.
  DispatchMessage($"Is all download completed {e.IsAllDownloadCompleted}.");
}

private void OnMultiDownloadCompleted(object sender, MultiDownloadCompletedEventArgs e)
{
  var info = e.Version as VersionInfo;
  DispatchMessage($"{info.Name} download completed.");
}

private void OnMultiDownloadError(object sender, MultiDownloadErrorEventArgs e)
{
  var info = e.Version as VersionInfo;
  DispatchMessage($"{info.Name} error!");
}
```

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

| Property                                                     |
| ------------------------------------------------------------ |
| **AppType** int 1:ClientApp 2:UpdateApp                      |
| **AppName** string Name of the application to launch.        |
| **AppSecretKey** string Application secret key.              |
| **ClientVersion** string Current client version.             |
| **UpdateUrl** string Update check API address.               |
| **UpdateLogUrl** string URL for the update log webpage.      |
| **InstallPath** string Installation path (used for update file logic). |
| **MainUpdateUrl** string Main update check API address.      |
| **MainAppName** string Main client application name          |

### 🍵UpdateOption

| Enum                                                         |
| ------------------------------------------------------------ |
| **Format** File format of the update package.                |
| **Encoding** Compression encoding.                           |
| **DownloadTimeOut** Timeout duration (in seconds). If not specified, the default timeout is 30 seconds. |

### 🌼LaunchTaskAsync()

**Method**

Asynchronously launch an update task.

```c#
public Task<GeneralUpdateBootstrap> LaunchTaskAsync();
```

### 🌼LaunchAsync()

**Method**

Launch the update.

```c#
public virtual TBootstrap LaunchAsync();
```

### 🌼SetBlacklist()

**Method**

Set the blacklist information that will be ignored during updates to prevent special files from causing update failures when binarydifferential updates cannot be used.

```c#
public virtual TBootstrap SetBlacklist(List<string> files = null, List<string> fileFormats = null);
```

**Parameter Types**

```c#
List<string> Collection of blacklist information.
```

**Parameters**

```c#
files List<string> Collection of file names in the blacklist.

fileFormats List<string> Collection of file extensions in the blacklist.
```

### 🌼Option()

**Method**

Set update configuration.

```c#
public virtual TBootstrap Option<T>(UpdateOption<T> option, T value);
```

**Parameter Types**

T Type for setting update operation UpdateOption.

**Parameters**

```c#
option UpdateOption<T> Configuration action enumeration.

value T The value to set, with the type determined by the UpdateOption enumeration.
```

### 🌼Config()

**Method**

Custom Configuration (Recommended for all platforms).

```c#
public GeneralClientBootstrap Config(Configinfo info);
public GeneralClientBootstrap Config(string url, string appSecretKey, string appName = "GeneralUpdate.Upgrade");
```

**Parameter Types**

Configinfo 

Client-related parameter class (fields like AppType, AppName, AppSecretKey, etc.).

**Parameters**

**info** Configinfo Client-related parameter class.

**url** string Remote server address.

**appSecretKey** string Application key (a secret key agreed upon with the server, used for client version management or specifying client upgrade).

**appName** string Name of the update program, without the extension.

### 🌼GetOption()

**Method**

```c#
public virtual T GetOption<T>(UpdateOption<T> option);
```

**Parameter Types**

T 

Result obtained based on the UpdateOption enumeration.

**Parameters**

```c#
option UpdateOption<T> Specific enumeration content as referenced in the 🍵UpdateOption section of this document.
```

### 🌼SetCustomSkipOption()

**Method**

Allow users to decide whether to proceed with an update in non-mandatory update scenarios.

```c#
public GeneralClientBootstrap SetCustomSkipOption(Func<bool> func);
public GeneralClientBootstrap SetCustomSkipOption(Func<Task<bool>> func);
```

**Parameter Types**

```c#
Func<bool> Inject a synchronous custom callback function, often used to let users decide whether to skip the current version update.
Func<Task<bool>> Inject a Task-based asynchronous custom callback function.
```

**Parameters**

```c#
func Func<bool> Inject a synchronous custom callback function, often used to let users decide whether to skip the current version update.
func Func<Task<bool>> Inject a Task-based asynchronous custom callback function.
```

### 🌼AddCustomOption()

**Method**

Add an asynchronous custom operation. In theory, any custom operation can be completed. It's recommended to register environment check methods to ensure dependencies and environment are intact after the update.

```c#
public GeneralClientBootstrap AddCustomOption(List<Func<bool>> funcs);
public GeneralClientBootstrap AddCustomOption(List<Func<Task<bool>>> funcs);
```

**Parameter Types**

```c#
List<Func<bool>> Inject a set of synchronous custom callback functions with a boolean return value.

List<Func<Task<bool>>> Inject a set of Task-based asynchronous custom callback functions with a boolean return value.
```

**Parameters**

```c#
funcs List<Func<bool>> Inject a set of synchronous custom callback functions, usually for environment checks (e.g., checking for missing VC++ libraries or hardware device drivers like printers, cameras, etc.).

funcs List<Func<Task<bool>>> Inject a set of Task-based asynchronous custom callback functions.
```

### 🌼AddListenerMultiAllDownloadCompleted()

**Method**

```c#
public TBootstrap AddListenerMultiAllDownloadCompleted(Action<object, MultiAllDownloadCompletedEventArgs> callbackAction);
```

**Parameter Types**

**sender** object

Operation handle.

**args** MultiAllDownloadCompletedEventArgs 

Parameters for notification when all versions are downloaded.

**Parameters**

```c#
callbackAction Action<object, MultiAllDownloadCompletedEventArgs> 
```

Listen for events when all update versions are downloaded.

### 🌼AddListenerMultiDownloadCompleted()

**Method**

```c#
public TBootstrap AddListenerMultiDownloadCompleted(Action<object, MultiDownloadCompletedEventArgs> callbackAction);
```

**Parameter Types**

sender object 

Operation handle.

MultiDownloadCompletedEventArgs

Parameters for listening to the completion of each version update package download.

**Parameters**

```c#
callbackAction Action<object, MultiDownloadCompletedEventArgs>
```

Listen for events when each version download is completed.

### 🌼AddListenerMultiDownloadError()

**Method**

```c#
public TBootstrap AddListenerMultiDownloadError(Action<object, MultiDownloadErrorEventArgs> callbackAction);
```

**Parameter Types**

**sender** object 

Operation handle.

**args** MultiDownloadErrorEventArgs

Parameters for notification of download errors.

**Parameters**

```c#
callbackAction Action<object, MultiDownloadErrorEventArgs>
```

Listen for events when each version download encounters an error.

### 🌼AddListenerMultiDownloadStatistics()

**Method**

```c#
public TBootstrap AddListenerMultiDownloadStatistics(Action<object, MultiDownloadStatisticsEventArgs> callbackAction);
```

**Parameter Types**

**sender** object 

Operation handle.

**args** MultiDownloadStatisticsEventArgs

Parameters for download information statistics (download speed, download size, etc.).

**Parameters**

```c#
callbackAction Action<object, MultiDownloadStatisticsEventArgs>
```

Listen for events about download statistics for each version.

### 🌼AddListenerException()

**Method**

```c#
public TBootstrap AddListenerException(Action<object, ExceptionEventArgs> callbackAction);
```

**Parameter Types**

**sender** object 

Operation handle.

**args** ExceptionEventArgs

Exception parameters.

**Parameters**

```c#
callbackAction Action<object, ExceptionEventArgs>
```

Listen for all exceptions within the update component.

### Applicable To

| Product        | Version       |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
