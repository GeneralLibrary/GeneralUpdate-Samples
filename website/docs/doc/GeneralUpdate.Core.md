### Definition

Namespace: GeneralUpdate.Core

Assembly: GeneralUpdate.Core.dll



GeneralUpdate.Core is one of the most essential components, providing a wide range of primary functionalities. Once the main program upgrade operation is completed, this component is invoked through process startup and parameter passing to complete the main program upgrade operation. (Main responsibility is updating the main program)

```c#
public class GeneralUpdateBootstrap : AbstractBootstrap<GeneralUpdateBootstrap, IStrategy>
```

NuGet Installation

```shell
NuGet\Install-Package GeneralUpdate.Core -Version 9.1.2
```

### Example

![](imgs/muti_donwload.png)

The following example defines methods, including the usage of GeneralUpdateBootstrap.

```c#
try
{
     Console.WriteLine($"Upgrade program initialization, {DateTime.Now}!");
     Console.WriteLine("Current running directory: " + Thread.GetDomain().BaseDirectory);
     await Task.Delay(2000);
     await new GeneralUpdateBootstrap()
     // Notification event for download speed, remaining download time, and current version information of single or multiple update packages
     .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
     // Completion of download for single or multiple update packages
     .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
     // Notification for completion of all download tasks
     .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
     // Notification for exceptions during the download process
     .AddListenerMultiDownloadError(OnMultiDownloadError)
     // Any issues during the entire update process will be notified through this event
     .AddListenerException(OnException)
     // Set field mappings for parsing information strings of all driver packages
     //.SetFieldMappings(fieldMappingsCN)
     // Enable driver updates
     //.Option(UpdateOption.Drive, true)
     .LaunchAsync();
     Console.WriteLine($"Upgrade program has started, {DateTime.Now}!");
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

### Annotations

GeneralUpdateBootstrap provides the following capabilities.

#### Constructors

| Constructors             | Description                                |
| ------------------------ | ------------------------------------------ |
| GeneralUpdateBootstrap() | Current GeneralUpdateBootstrap constructor |
| base:AbstractBootstrap() | Parent class AbstractBootstrap constructor |

#### Properties

| Properties   | Description                                |
| ------------ | ------------------------------------------ |
| Packet       | Update package information                 |
| UpdateOption | Update operation configuration enumeration |

#### Methods

| Method                                 | Description                                                |
| -------------------------------------- | ---------------------------------------------------------- |
| LaunchTaskAsync()                      | Asynchronously launch update with Task                     |
| LaunchAsync()                          | Launch update                                              |
| Option()                               | Set update configuration                                   |
| GetOption()                            | Get update configuration                                   |
| AddListenerMultiAllDownloadCompleted() | Listen for completion of all version downloads             |
| AddListenerMultiDownloadCompleted()    | Listen for completion of each version download             |
| AddListenerMultiDownloadError()        | Listen for download errors for each version                |
| AddListenerMultiDownloadStatistics()   | Listen for download statistics/progress for each version   |
| AddListenerException()                 | Listen for all internal exceptions in the update component |

### 🍵UpdateOption

**Enumeration**

**Drive** Whether to enable driver upgrade functionality.

### 🌼LaunchTaskAsync()

**Method**

Asynchronously launch update with Task.

```c#
public Task<GeneralUpdateBootstrap> LaunchTaskAsync();
```

### 🌼LaunchAsync()

**Method**

Launch update.

```c#
public virtual TBootstrap LaunchAsync();
```

### 🌼SetBlacklist()

**Method**

Set information for blacklisted items to be ignored during updates to avoid failures due to special files not being usable in binary differential updates.

```c#
public virtual TBootstrap SetBlacklist(List<string> files = null, List<string> fileFormats = null);
```

**Parameter Types**

```c#
List<string> Collection of blacklist information.
```

**Parameters**

```c#
files List<string> Collection of blacklisted file names.

fileFormats List<string> Collection of blacklisted file extensions.
```

### 🌼Option()

**Method**

Set update configuration.

```c#
public virtual TBootstrap Option<T>(UpdateOption<T> option, T value);
```

**Parameter Types**

T The type for setting update operations in UpdateOption.

**Parameters**

```c#
option UpdateOption<T> Configuration action enumeration.

value T The value to be set, with the type determined by the UpdateOption enumeration.
```

### 🌼GetOption()

**Method**

```c#
public virtual T GetOption<T>(UpdateOption<T> option);
```

**Parameter Types**

T Result obtained based on UpdateOption enumeration.

**Parameters**

```c#
option  UpdateOption<T> Refer to 🍵UpdateOption in this document for specific enumeration content.
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

Parameters for notification of completion of all version downloads.

**Parameters**

```c#
callbackAction Action<object, MultiAllDownloadCompletedEventArgs> 
```

Parameters for the event callback when all update versions have been downloaded.

### 🌼AddListenerMultiDownloadCompleted()

**Method**

```c#
public TBootstrap AddListenerMultiDownloadCompleted(Action<object, MultiDownloadCompletedEventArgs> callbackAction);
```

**Parameter Types**

sender object 

Operation handle.

MultiDownloadCompletedEventArgs

Parameters for the callback when each version update package download is completed.

**Parameters**

```c#
callbackAction Action<object, MultiDownloadCompletedEventArgs>
```

Parameters for the callback when each version download encounters an error.

### 🌼AddListenerMultiDownloadError()

**Method**

```c#
public TBootstrap AddListenerMultiDownloadError(Action<object, MultiDownloadErrorEventArgs> callbackAction);
```

**Parameter Types**

**sender** object 

Operation handle.

**args** MultiDownloadErrorEventArgs

Parameters for download error notifications.

**Parameters**

```c#
callbackAction Action<object, MultiDownloadErrorEventArgs>
```

Parameters for the callback when each version download encounters an error.

### 🌼AddListenerMultiDownloadStatistics()

**Method**

```c#
public TBootstrap AddListenerMultiDownloadStatistics(Action<object, MultiDownloadStatisticsEventArgs> callbackAction);
```

**Parameter Types**

**sender** object 

Operation handle.

**args** MultiDownloadStatisticsEventArgs

Parameters for download statistics (download speed, download size, etc.).

**Parameters**

```c#
callbackAction Action<object, MultiDownloadStatisticsEventArgs>
```

Parameters for the event callback for download statistics for each version.

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

Parameters for the callback for all internal exceptions in the update component.

### Applicable to

| Product        | Versions      |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
