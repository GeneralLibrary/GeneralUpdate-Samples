---
sidebar_position: 5
---

### Definition

Namespace: GeneralUpdate.Core

Assembly: GeneralUpdate.Core.dll



GeneralUpdate.Core is one of the most essential components, providing a wide range of primary functionalities. Once the main program upgrade operation is completed, this component is invoked through process startup and parameter passing to complete the main program upgrade operation. (Main responsibility is updating the main program)

```c#
public class GeneralUpdateBootstrap : AbstractBootstrap<GeneralUpdateBootstrap, IStrategy>
```



### Example

GeneralUpdateBootstrap uses code examples [[View]](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs)。

![](imgs/muti_donwload.png)



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



### Applicable to

| Product        | Versions      |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
