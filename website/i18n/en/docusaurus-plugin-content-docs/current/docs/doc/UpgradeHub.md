---
sidebar_position: 7
---

### Definition

Namespace: GeneralUpdate.ClientCore.Hubs

Assembly: GeneralUpdate.ClientCore.dll



The `UpgradeHubService` is a mechanism based on SignalR for pushing updated version information. It supports both one-to-one and one-to-many push notifications.

```c#
public class UpgradeHubService : IUpgradeHubService
```

NuGet Installation

```shell
NuGet\Install-Package GeneralUpdate.ClientCore -Version 9.1.0
```

### Example

The following example defines methods that include the use of `VersionHub`.

```c#
//1. General usage
var hub = new UpgradeHubService("http://localhost:5000/UpgradeHub"
                , null,"dfeb5833-975e-4afb-88f1-6278ee9aeff6");
    hub.AddListenerReceive((message) =>
    {
        // The message is currently limited to a JSON string of the Packet object
        Debug.WriteLine(message);
    });
await hub.StartAsync();

//2. In projects with dependency injection capability, you can also use dependency injection, for example: Prism
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    // Register Services
    containerRegistry.Register<IUpgradeHubService, UpgradeHubService>();
}

public MainWindowViewModel(IUpgradeHubService service) 
{
    service.StartAsync();
    //...
}
```

**(1) Point-to-point push**

![](imgs/maui_windows_push_version.png)

**(2) Push updates to multiple clients at once**

![push_version_mutil](imgs/push_version_mutil.png)

### Notes

`UpgradeHubService` provides the functionality to receive server push messages.

#### Methods

| Method                   | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| AddListenerReceive()     | Subscribe to the latest version info pushed by the server in real-time. |
| AddListenerOnline()      | Listen for online and offline notifications                  |
| AddListenerReconnected() | Notification for reconnection                                |
| AddListenerClosed()      | Notification for connection closure                          |
| StartAsync()             | Start the connection                                         |
| StopAsync()              | Pause the connection                                         |
| DisposeAsync()           | Release the Hub object instance                              |

### 🌼UpgradeHubService()

**Constructor**

Initializes the Hub constructor.

```c#
UpgradeHubService(string url, string? token = null, string? appkey = null)
```

**Parameters**

```c#
url string The subscription address of the Hub.

token string The token string used in the Id4 authentication process.

appkey string The client key, uniquely identified, recommended value is a Guid, which can be randomly generated.
```

### Applicable to

| Product        | Version       |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
| ASP.NET        | Any           |