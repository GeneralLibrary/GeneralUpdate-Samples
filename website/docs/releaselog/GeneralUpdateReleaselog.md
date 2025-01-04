---
slug: releaselog
title: GeneralUpdate
authors: juster
tags: [log]
---

# 📒Release log



## 📍2025-01-04 9.1.0

- Add a feature to skip specified folders (specified parameters will be passed through Client and Upgrade).
- Add Scheme and Token parameters to the HTTP request headers.
- Fix the issue of OSS Samples failing to update.
- Modify the OSS feature to not delete the update package upon completion.
- Fix the issue in Samples where only the Client updates, not the Upgrade. (The looping update occurred because the local version number was not changed)
- Upgrade all Samples from version 9.0.0 to 9.1.0.



## 📍2024-11-28 9.0.0

- Upgrade all components to C# 13 syntax.

- Optimize, refactor, and simplify code to achieve functionality with minimal code, reducing the difficulty of code reading.

- Simplified the complexity of input parameters for GeneralClientBootstrap.

- Removed Strategys parameter settings; these are now built into the component, automatically deducing the operating system platform to switch update strategies, so developers no longer need to manage update strategy settings.

- Added features for driver updates, backups, and installations.

- Optimized the automatic upgrade process with four types of update workflows:

  1. Client needs an upgrade, update server needs an upgrade.
  
  2. Client does not need an upgrade, update server does not need an upgrade.
  
  3. Client does not need an upgrade, update server needs an upgrade.
  
  4. Client needs an upgrade, update server does not need an upgrade.

- If an update fails, the version will be stored locally, and upon rollback, the failed version will be skipped on subsequent attempts.

- GeneralUpdate's OSS feature currently supports only Windows and only the zip compression format.

- Removed all event notifications from GeneralUpdateOSS.

- Introduced the GeneralUpdate.Bowl component.

- GeneralUpdate.Bowl includes rollback, monitoring, and dump export functionalities (Only for Windows; Linux support will be added gradually).

- Introduced the GeneralUpdate.Common component.

- Removed the GeneralUpdate.Zip component.

- Removed the GeneralUpdate.AspnetCore component.

- Removed the MultiDownloadProgressChangedEvent, merging its notification content into the MultiDownloadStatisticsEvent.

- Removed the legacy feature, replaced by GeneralUpdate.Bowl.

- Removed support for the 7z compression format; only zip compression is supported now.

- Removed ProgressType notification event parameters for various working modes.

- Replaced VersionHub with UpgradeHubService (push functionality).

- Updated all hash value-related checks and generation within components to the SHA256 algorithm, removing the MD5 algorithm.

- Added functionality to back up all current program files before updating.

- Added support for the Ubuntu operating system.

- Compatible with and supports AOT compilation, removing or refactoring all code detrimental to AOT compilation or usage.

- All component version numbers follow the .NET Core framework version number, sharing a unified version number instead of maintaining separate ones.

- Updated Sample example updates, allowing for one-click generation using bat scripts.

- Fixed several bugs reported in issues.

- Moved GeneralUpdate.Maui.OSS and GeneralUpdate.OSS libraries to a new repository GeneralUpdate.Maui.

- GeneralUpdate.Differential removed the GeneralUpdate.Zip reference and all compression package handling capabilities.

- Added reporting of the upgrade process to the server with statuses like pending update, update failed, and update successful.

- Refactored GeneralUpdate.Tools to an Avalonia version, adapting it for creating differential patch packages on Linux operating systems.



## 📍2023-08-05

Before releasing an enterprise product to the market, it's crucial to test its features. In many companies, the auto-update feature often lacks documentation such as requirement documents or business specifications, unlike other product features. Typically, the main requirement is that the product can be updated smoothly or incrementally to save bandwidth. For testers with insufficient experience, this can lead to inadequate testing or overlooked issues, causing numerous problems. (This is just a testing approach and not professional testing guidance, so please consider it critically.)

1. **Testing Version Upgrade Order**

   Assume all customers are currently using version v1.0.0.0, and we are about to release v2.0.0.0. Before release, both versions need to be tested in a test environment.

   - While downloading the update package
   - During file updates
   - Simulate program interruption, network disconnection, power outages, weak network conditions, crashes, etc.

2. **Encrypted Files Cannot Be Upgraded**

   In GeneralUpdate, when extracting binary differential update patches between two versions, encrypted files may not be recognized by the differential algorithm. Consider adding encrypted files to a blacklist (SetBlacklist) or directly including them in the compressed package.

3. **Failure Rollback or Re-upgrade?**

   This is a major concern in enterprises. While auto-updates offer many benefits, a failed update can render the client unusable, which is critical. I plan to add two strategies in GeneralUpdate to address this issue.

   - **Strategy One:** 
     Back up files or directories to be updated before the upgrade. If the update fails, the backup files will be restored upon the second startup, and the auto-update will be disabled to prevent further updates after restoration.

   - **Strategy Two:** 
     This is my preferred method. The essence of auto-updates is to upgrade, not roll back. The initial idea is to introduce a "last words" mechanism. To handle exceptions during updates that may corrupt files, the program should report the update status to the server after each update. If the client doesn't respond within 30 minutes, it is considered a destructive update failure (file corruption). Upon each startup, the upgrade program reads the last update's status. If the previous update failed, it automatically downloads and installs the client package (compressed package). Alternatively, introduce an update daemon process to receive real-time push notifications for automated downloads and installations.

4. **Project Structure Adjustments**

   If developers make structural changes to the project, such as moving or renaming modules in a client program based on IoC principles (e.g., Prism framework), it may lead to exceptions where the IoC container can't find the DLL after an update. Consider file path changes during updates, and testers should routinely inquire about such changes.

5. **Final Testing as a Closed-Loop Process**

   This is crucial. If auto-updates are tested early, subsequent bug fixes or changes may introduce unknown issues. Therefore, conducting tests as the final closed-loop process is recommended (adjust as needed for special cases).

6. **Weak Network Environment Testing**

   Use tools like NetLimiter and Clumsy to simulate weak network conditions. For detailed usage, refer to my article: [Link](https://mp.weixin.qq.com/s?__biz=MzI5MTg4NzIyNg==&mid=2247486705&idx=1&sn=b2eac543ea7f738bdbe8c3cc63da3488&chksm=ec088de4db7f04f25e5603d2309d61279b0f94ee6eef34129801755a86cf406f863896214b3c&token=1809721461&lang=zh_CN#rd).

7. **Gray Release**

   Before releasing a version, thorough testing is necessary.

   - For large customer bases, conduct a small-scale gray release before full release to all customers.
   - For gray upgrades, avoid setting the version as mandatory; let users choose whether to upgrade.

8. **Targeted Upgrades**

   GeneralUpdate currently supports receiving push notifications of the latest version updates via Signal R. If a client with multiple devices has one device experiencing issues due to hardware or software environment problems, targeted fixes are necessary. In this case, use one-to-one upgrades for precise updates on the problematic device.

9. **Compatibility of Old Configurations with New Versions**

   When different branches of a version exist in the market, consider local configuration files during auto-update upgrades. If the new version's program requires reading configuration files, incompatible old version configurations may cause issues. Testers should be aware of this scenario.

   To avoid such issues, store frequently changing configurations on the server to be read at each login, while storing stable configurations locally.

10. **Automated Testing**

   Writing scripts for automated testing of auto-updates is crucial. It saves time and increases accuracy in multi-branch and multi-version upgrade testing.



## 📍2023-07-23

Developer Questions:

1. **What should be done if unexpected situations like network disconnection, power outage, computer crash, sudden blue screen, or program freeze occur during the update process, causing the application to fail to update or start properly? Can the update program roll back to the previous version?**

   **Answer:** This question can be divided into two parts: 
   
   (1) In the event of unexpected situations, you can try restarting the application to resume the update download from the breakpoint. Currently, if files become corrupted during the update, reinstallation is the only option. In the future, we plan to introduce a mechanism to handle situations where the client application cannot start due to update anomalies.
   
   (2) Rolling back is not possible because the core purpose of the auto-update program is to facilitate updates. Rolling back could negate the purpose of the update. It is preferable to quickly identify issues and urgently release a safe update package to allow clients to update incrementally until successful.

2. **What if the update package itself contains issues (e.g., abnormal files), causing the program to fail to start after the update? Can it roll back or back up?**

   **Answer:** No, comprehensive testing must be conducted before the version is released. For a large customer base, a small-scale gray release should be conducted first. If no issues arise, then the release can be extended to all customers in the market.

3. **If the client locally stores some data files and needs to add new content while retaining previous data (e.g., updating a SQLite .db file without overwriting the old .db file), how can this be resolved? Can incremental updates handle such files?**

   **Answer:** We previously developed such a feature, but it was ineffective and has been temporarily discontinued. It will require a redesign before being reinstated. Currently, the component's functionality cannot address this issue, and the only option is to overwrite. Incremental updates are also unlikely to handle this situation effectively.



## 📍2023-04-23

### Technical Update Overview

**New Features: Blacklist Functionality and OSS Support for Windows and .NET MAUI Android Versions**

This update focuses on bug fixes and a complete refactoring of the event management notification system across components.

#### Release Details

| Component Name             | Version (Old) | Version (New) | Status  |
| -------------------------- | ------------- | ------------- | ------- |
| GeneralUpdate.AspNetCore   | 1.4.1         | -             | -       |
| GeneralUpdate.ClientCore   | 2.8.9         | 2.12.9        | Updated |
| GeneralUpdate.Core         | 4.11.18       | 4.14.18       | Updated |
| GeneralUpdate.Differential | 1.3.0         | 1.4.1         | Updated |
| GeneralUpdate.Zip          | 1.3.0         | -             | -       |
| GeneralUpdate.Tool         | 2.2.5         | 2.3.5         | Updated |
| GeneralUpdate.Single       | 1.0.0         | -             | -       |
| GeneralUpdate.Maui.OSS     | 1.0.0         | 1.0.0         | New     |

#### Component Updates

**1. GeneralUpdate.ClientCore**

- Added OSS update functionality.
- Introduced blacklist functionality.
- Refactored events with a new event management mechanism.
- Fixed issue where incremental packages could only recognize additions, not deletions.

**2. GeneralUpdate.Core**

- Added OSS update functionality.
- Refactored events with a new event management mechanism.
- Fixed issue where incremental packages could only recognize additions, not deletions.

**3. GeneralUpdate.Differential**

- Fixed issue where incremental packages could only recognize additions, not deletions.

#### Changes in GeneralUpdate.ClientCore and GeneralUpdate.Core

1. **Event Subscription Mechanism**
   - The event subscription mechanism is refactored. Instead of using `event +=`, events are now managed through an `EventManager`. This requires minimal code changes for developers and reduces the component's codebase by favoring composition over inheritance.

2. **Blacklist Management**
   - Added the `SetBlacklist` method to manage files you don't want to update. The component has built-in default blacklist files: `{ "Newtonsoft.Json.dll" }` and default blacklist file extensions: `{ ".patch", ".7z", ".zip", ".rar", ".tar" , ".json" }`.

```csharp
Task.Run(async () =>
{
    var configinfo = GetWindowsConfiginfo();
    var generalClientBootstrap = await new GeneralClientBootstrap()
        .AddListenerMultiDownloadProgress(OnMultiDownloadProgressChanged)
        .AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics)
        .AddListenerMultiDownloadCompleted(OnMultiDownloadCompleted)
        .AddListenerMultiAllDownloadCompleted(OnMultiAllDownloadCompleted)
        .AddListenerMultiDownloadError(OnMultiDownloadError)
        .AddListenerException(OnException)
        .Config(configinfo)
        .Option(UpdateOption.DownloadTimeOut, 60)
        .Option(UpdateOption.Encoding, Encoding.Default)
        .Option(UpdateOption.Format, Format.ZIP)
        .Strategy<WindowsStrategy>()
        .SetCustomOption(ShowCustomOption)
        .SetBlacklist(GetBlackFiles(), GetBlackFormats())
        .LaunchTaskAsync();
});
```

#### New OSS Functionality

**OSS (Object Storage Service)** is a familiar concept for backend developers, provided by major cloud service providers like Alibaba Cloud OSS, Tencent Cloud COS, and Huawei Cloud OBS. The update component adopts OSS for its naming and functionality, making it intuitive for developers to understand its purpose.

**GeneralUpdate.OSS Operation Principle:**

1. Prepare a `version.json` configuration file and an update file (`update.zip`) as usual.

```json
[
  {
    "PubTime": 1680443321,
    "Name": "generalupdate.ossclient",
    "MD5": "9bf414990a67e74f11752d03f49b15d8",
    "Version": "1.0.4",
    "Url": "http://192.168.50.203/update.zip"
  },
  {
    "PubTime": 1680444916,
    "Name": "generalupdate.ossclient",
    "MD5": "JXC122DFXCZXZNMRFf11752d03f49b15d8",
    "Version": "1.0.5",
    "Url": "http://192.168.50.203/update2.zip"
  }
]
```

2. The client requests the OSS server or file server to download the `version.json` file at startup.
3. Upon downloading, it parses the version information to decide if an update is necessary. If so, it passes the info through the process to start `Upgrade` (the client self-closes).
4. `Upgrade` then downloads `update.zip`, extracts it, and overwrites local files.
5. Once the update completes, `Upgrade` restarts the client and self-closes. The update is finished.

**GeneralUpdateOSS** is easier to use compared to GeneralUpdateBootstrap. It downloads `version.json` and updates incrementally based on its content.

#### Quick Start

**Client (Main Client) Code Example:**

```csharp
Task.Run(async () =>
{
    var url = "http://192.168.50.203";
    var appName = "GeneralUpdate.Client";
    var version = "1.0.0.0";
    var versionFileName = "version.json";
    ParamsOSS @params = new ParamsOSS(url, appName, version, versionFileName);
    await GeneralClientOSS.Start(@params);
});
```

**Upgrade (Upgrade Assistant) Code Example:**

```csharp
private static void Main(string[] args)
{
    Task.Run(async () =>
    {
        ParamsOSS @params = SerializeUtil.Deserialize<ParamsOSS>(args[0]);
        await GeneralUpdateOSS.Start<OSSStrategy>(@params, Encoding.Default);
    });
}
```

**Event Notification Subscription:**

```csharp
GeneralUpdateOSS.AddListenerMultiDownloadStatistics(OnMultiDownloadStatistics);

private static void OnMultiDownloadStatistics(object sender, MultiDownloadStatisticsEventArgs e)
{
    Console.WriteLine($" {e.Speed} , {e.Remaining.ToShortTimeString()}");
}
```

#### .NET MAUI OSS

The MAUI OSS functionality is similar to the OSS described above but is specifically for .NET MAUI, focusing on the Android platform.

1. Prepare a `version.json` configuration file and an update file (`update.apk`), which is the new version's APK.

```json
{
  "PubTime": 1680444916,
  "Name": "com.companyname.generalupdate.ossclient",
  "MD5": "9bf414990a67e74f11752d03f49b15d8",
  "Version": "1.0.5",
  "Url": "http://192.168.50.203/com.companyname.generalupdate.ossclient.apk"
}
```

2. The client requests the OSS server or file server to download the `version.json` file at startup.
3. After downloading, it parses the version information to decide if an update is necessary.
4. If an update is needed, it downloads `update.apk`.
5. Upon download completion, the Android OS handles the installation, and the new app version runs.

**Quick Start:**

```csharp
string url = "http://192.168.50.203";
string appName = "MainApplication.exe";
string currentVersion = "1.1.1.1";
string versionFileName = "versions.json";
GeneralUpdateOSS.AddListenerDownloadProcess(OnOSSDownload);
GeneralUpdateOSS.AddListenerException(OnException);
await GeneralUpdateOSS.Start<Strategy>(new ParamsAndroid(url, appName, "123456789", currentVersion, versionFileName));
```

**Event Notification Subscription:**

```csharp
GeneralUpdateOSS.AddListenerDownloadProcess(OnOSSDownload);
GeneralUpdateOSS.AddListenerException(OnException);

private void OnOSSDownload(object sender, OSSDownloadArgs e)
{
    Console.WriteLine($"{e.ReadLength},{e.TotalLength}");
}

private void OnException(object sender, ExceptionEventArgs exception)
{
    Console.WriteLine(exception.Exception.Message);
}
```

**Guidance Articles for .NET MAUI Android Related Queries:**

- [Blog Post on .NET MAUI Android](https://www.cnblogs.com/MASA/p/16612541.html)
- [Microsoft Documentation on .NET MAUI Android Deployment](https://learn.microsoft.com/zh-cn/dotnet/maui/android/deployment/?view=net-maui-7.0)
- [Stack Overflow: Android 8 Cleartext HTTP Traffic Not Permitted](https://stackoverflow.com/questions/45940861/android-8-cleartext-http-traffic-not-permitted)
- [Youdao Note](https://note.youdao.com/ynoteshare/mobile.html?id=5c5d5cf8fe1d67419b09024255ff239c)

**Current Testing Devices and Platforms:**

1. Huawei Honor Px30 (non-HarmonyOS) - operational.
2. Visual Studio 2022 Preview on Pixel 5 - API 33 (Android 13.0 - API 33) - operational.



## 📍2023-01-17

### Open Source Project Reorganization

We have established the "General" open source organization on both GitHub and Gitee platforms. Contributors with significant code contributions will be invited to join the organization and receive rewards.

The original GeneralUpdate project has been split into three separate projects:

1. **GeneralLibrary on Gitee:**
   - Main organization page: [GeneralLibrary](https://gitee.com/GeneralLibrary)

2. **Projects under GeneralLibrary:**
   - **Automatic Update Project:** [GeneralUpdate](https://gitee.com/GeneralLibrary/GeneralUpdate)
   - **Differential Patch Packaging Tool:** [GeneralUpdate.Tools](https://gitee.com/GeneralLibrary/GeneralUpdate.Tools)
   - **Usage Examples:** [GeneralUpdate-Samples](https://gitee.com/GeneralLibrary/GeneralUpdate-Samples)



## 📍2022-10-09

Release Notes

| Component Name             | Version (old) | Version (new) | Status |
| -------------------------- | ------------- | ------------- | ------ |
| GeneralUpdate.AspNetCore   | 1.1.0         | 1.4.1         | Normal |
| GeneralUpdate.ClientCore   | 2.3.6         | 2.8.9         | Normal |
| GeneralUpdate.Core         | 4.10.12       | 4.11.18       | Normal |
| GeneralUpdate.Differential | 1.0.0         | 1.3.0         | Normal |
| GeneralUpdate.Zip          | 1.0.0         | 1.3.0         | Normal |
| GeneralUpdate.Tool         | 1.0.0         | 2.1.5         | Normal |
| GeneralUpdate.Single       | 1.0.0         | -             | Normal |

**[1] Component GeneralUpdate.AspNetCore**

1. Refactor: Merged previous version and validate methods into a single Update method to perform version validation and return update information.
2. New: Added an upload method to support the automatic upload release function of GeneralUpdate.PacktTool. This method completes the storage of version information and the placement of update packages on the file server.
3. New: Added a push method to support immediate push of important versions or updates between server services to the client, using Signal R to push update information after a single call.

**[2] Component GeneralUpdate.ClientCore**

1. Fix: Resolved file conflict issue between AutoUpdate.Core and AutoUpdate.ClientCore.
2. Fix: Resolved error when components do not require an update.
3. Refactor: Updated mechanism to support multiple operating system platforms.
4. Refactor: Refactored version validation mechanism.
5. New: Configuration adaptation for MAUI - Windows machine.
6. Fix: When the main program is already the latest, should it first check to avoid launching the upgrade software unnecessarily?
7. Refactor: Changed the method signature for configuration from `public GeneralClientBootstrap Config(ClientParmeter info)` to `public GeneralClientBootstrap Config(Configinfo info)`, removing the fields validaterul and versionurl.
8. New: Added a user-defined method to decide whether to skip the current update using a Task version method.
9. New: Added an enumeration to the update configuration, changing the original string ".zip" to the enumeration Format.ZIP to prevent user input errors.

**[3] Component GeneralUpdate.Core**

1. Refactor: Updated mechanism to support multiple operating system platforms.
2. Refactor: Refactored version validation mechanism.
3. Fix: When the main program is already the latest, should it first check to avoid launching the upgrade software unnecessarily?
4. Fix: Bug in "GeneralUpdate.Core\Utils\FileUtil.cs" GetFileMD5 function.
5. Fix: Issue with multi-level folder structure updates failing.
6. New: Added a unique client identifier in the update system.
7. Offline: JSON configuration file update function will be directly overwritten. Will be back online after refactoring. Affected areas include:
   - Suggestions from users.
   - Error reports.
8. Research: Feasibility of running normally in a Mac environment.
9. Fix: Bug in "GeneralUpdate\src\GeneralUpdate.Core\Download\AbstractTask.cs" at line 143.

**[4] Component GeneralUpdate.Differential**

- Refactor: Recursively search for files to update to prevent missing update detections. Added a tree structure to manage the update file directory structure.
- Fix: Bug where only the first layer of files could be updated. Now all directories and subdirectories can be recursively updated.
- Fix: Error when the packaging tool includes two files with the same name but different extensions.

**[5] Component GeneralUpdate.Zip**

1. Fix: Several issues related to file compression.
2. Fix: Garbled characters in Word files.
3. Fix: Permission issues when extracting downloaded packages in C:\Program Files (x86).

**[6] GeneralUpdate.PacketTool**

1. Fix: Packaging tool did not consider subfolder issues during packaging.
2. Refactor: Refactored the file functionality system.
3. Refactor: Migrated packaging tool to MAUI.
4. New: Added upload functionality to GeneralUpdate.Tool.
5. Fix: PacketTool's packaging function was ineffective when only file content was modified.
6. Fix: Garbled characters in Chinese filenames after file extraction.

**[7] Test Cases and Example Code**

Example Code:
- src/c#/GeneralUpdate.Api/Program.cs
- src/c#/GeneralUpdate.Client/MainPage.xaml.cs
- src/c#/GeneralUpdate.Upgrad/Program.cs

Test Cases (still in progress):
- src/c#/TestClientCore/UnitTest1.cs
- src/c#/TestDifferential/UnitTest1.cs
- src/c#/TestMD5/UnitTest1.cs
- src/c#/TestService/Program.cs
- src/c#/TestZIP/UnitTest1.cs



## 📍2022-04-03

(1) Does the main program support mutual upgrades with the update program?

Answer: Yes, it does.

(2) Do developers need to write code to manage the timing of process closure or any other code?

Answer: No, they don't. The component already considers the entire update process, so developers don't need to write any additional auxiliary code beyond the component code.

(3) Does the update program need to be in the same directory as the main program?

Answer: Yes, it does. However, make sure that the update program does not reference any code from the main program; otherwise, the update will fail.

(4) Will the update package's patch files be deleted after the update is complete?

Answer: Yes, they will. The component ensures a clean file list after the update, avoiding redundant files and unnecessary disk space usage.

(5) Can this be used on the server side, for instance, for upgrades between services?

Answer: Theoretically, it is supported, although the author has not used it this way personally. Feedback indicates that some users have done this. This sharing is focused on C/S architecture scenarios.

(6) How can I get the MD5 checksum of the update package?

Answer: Use the AutoUpdate.MD5 project from the source code.

(7) How do I create an update package?

Answer: Use the GeneralUpdate.PacketTool to generate it. You can find the packaged installation program in the release section of the source code repository.

(8) How can I learn more about other aspects of the component?

Answer: You can learn more through the official website, relevant QQ groups, or by discussing with me on Gitee or GitHub issues.

(9) How to handle lack of permissions when extracting the download package in C:\Program Files (x86)?

Answer: [Link to issue](https://gitee.com/Juster-zhu/GeneralUpdate/issues/I4ZKQ4)

(10) When the update file is small, the download speed shows as 0B/S.

Answer: [Link to issue](https://gitee.com/Juster-zhu/GeneralUpdate/issues/I3POMG)





## 📍2022-03-23

Instructions

- Online Help Documentation: [http://justerzhu.cn/](http://justerzhu.cn/) (Will be divided into separate documents by component in the future)
- NuGet Versioning Standards Reference: [NuGet Package Versioning](https://docs.microsoft.com/en-us/nuget/concepts/package-versioning)
- Application Assembly Versioning Standards Reference: [Assembly Versioning](https://docs.microsoft.com/en-us/dotnet/standard/assembly/versioning) (The client program updated by the component is essentially your company's product; the component's operations will follow this standard.)
- This release includes many changes and is not a smooth upgrade (use with caution if stability is crucial or if modifications are undesirable). The project is gradually stabilizing, and future updates will aim for smooth upgrades to avoid requiring further developer modifications.
- In case of a "blunder" such as incorrect update package content or unforeseen events during the update, rollback is not currently supported. The current solutions are (1) request the update again, or (2) send an emergency fix update package via the push mechanism.
- Regarding the "non-rollback" issue, if you have better solutions, please share your thoughts in the issues section. Currently, there are two opinions in the community: (1) In case of unexpected events, the component should roll back the local program to the previous version to ensure usability even if the update fails. (2) The purpose of the update component is to push the component to a new version, making the release effective. If the update fails, it should continue until it succeeds; only forward, no retreat.

Release Content

Let's first look at the version numbers and the changes made in this major release (8 sections in total).

| Component Name             | Version (old) | Version (new) | Status  |
| -------------------------- | ------------- | ------------- | ------- |
| GeneralUpdate.AspNetCore   | 1.0.0         | 1.1.0         | Normal  |
| GeneralUpdate.ClientCore   | 1.1.2         | 2.3.6         | Normal  |
| GeneralUpdate.Core         | 3.6.10        | 4.10.12       | Normal  |
| GeneralUpdate.Differential | -             | 1.0.0         | Normal  |
| GeneralUpdate.Common       | 1.0.0         | -             | Removed |

**[1] Component GeneralUpdate.AspNetCore**

1. [NEW] Added latest version push functionality with the VersionHub object. This primarily addresses the need to push urgent bug fix versions temporarily.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUpdateService, GeneralUpdateService>();
builder.Services.AddSignalR();
var app = builder.Build();

app.MapHub<VersionHub>("/versionhub");

app.Use(async (context, next) =>
{
   var hubContext = context.RequestServices.GetRequiredService<IHubContext<VersionHub>>();
   await CommonHubContextMethod((IHubContext)hubContext);
   if (next != null)
  {
       await next.Invoke();
  }
});

async Task CommonHubContextMethod(IHubContext context)
{
   await context.Clients.All.SendAsync("clientMethod", "");
}
```

**[2] Component GeneralUpdate.ClientCore**

1. [NEW] Overloaded configuration method in GeneralClientBootstrap, simplifying it to require only the remote server address, such as http://127.0.0.1, and the update program name, reducing developer concern over parameters as they are automatically obtained within the component.

```csharp
public GeneralClientBootstrap Config(string url, string appName = "AutoUpdate.Core");
```

2. [NEW] Added latest version reception push functionality with the VersionHub object. This primarily addresses the need to receive urgent bug fix versions temporarily, allowing update operations in the GetMessage callback function.

```csharp
VersionHub<string>.Instance.Subscribe($"{ baseUrl }/{ hubName }", "TESTNAME", new Action<string>(GetMessage));
```

**[3] Component GeneralUpdate.Core**

1. [FIX] Fixed issue where a download interruption due to network disconnection did not throw an exception after the timeout period.
2. [REFACTOR] Refactored multi-task download design.
3. [FIX] Fixed issue where FileUtil.Update32Or64Libs would throw exceptions under certain conditions.
4. [REFACTOR] Refactored default update strategy design.
5. [NEW] Added latest version reception push functionality with the VersionHub object. This primarily addresses the need to receive urgent bug fix versions temporarily, allowing update operations in the GetMessage callback function.

```csharp
VersionHub<string>.Instance.Subscribe($"{ baseUrl }/{ hubName }", "TESTNAME", new Action<string>(GetMessage));
```

6. [NEW] Added support for extracting 7z compressed files.
7. [REMOVE] Removed duplicate code between GeneralUpdate.Core and GeneralUpdate.ClientCore, addressing redundancy through linked files.
8. [NEW] Added support for incremental updates. For example, if only one file is updated, only that file will be packaged.
9. [NEW] Added support for binary differential functionality. For example, if a file like temp.dll is modified to create a new version, a .patch file will be created from the differences between the new and old versions. With normal project iteration, this .patch file will typically be only a few kilobytes. Combined with 7z's high compression, the update package size is minimized to save bandwidth and data usage. It should be noted that if new files are added in the new version, they will be included in the update package as they are.

**[4] Component [NEW] GeneralUpdate.Differential**

Added the Differential component in GeneralUpdate, which mainly provides the following functionalities:

1. [NEW] Added binary differential updates, generating .patch files.
2. [NEW] Added incremental updates. For example, if version 1.1.1.1 originally had 10 files and this release modified 3 of them, only the modified files will be packaged.
3. [NEW] Updated configuration files (currently only supports .json configuration files with a depth of 1 level). For example, if the client program uses a .json file as a configuration file, the original configuration content will be preserved while updating the configuration file. Assuming the client's original configuration file (.json) contains:

```json
{
  "ip":"123456789",
  "port":9999
}
```

The server's new configuration file structure is:

```json
{
  "ip":"0",
  "port":0,
  "path":"c://"
}
```

The "path" field will be updated into the client's local configuration file, preserving the original content as:

```json
{
  "ip":"123456789",
  "port":9999,
  "path":"c://"
}
```

**[5] Component GeneralUpdate.Zip**

1. [REFACTOR] Refactored compression component design to support .zip and .7z archive formats.
2. [NEW] Added 7z extraction functionality.

**[6] Component [REMOVE] GeneralUpdate.Common**

1. Removed this component, decoupling GeneralUpdate.Core and GeneralUpdate.ClientCore as it was deemed unnecessary for the current version.

**[7] Component GeneralUpdate.Single**

No functional updates in this release.

**[8] Test Cases Example**

To simplify developers' work in writing test cases during validation and testing, the following test cases have been added to the GeneralUpdate solution:

- AutoUpdate.ClientCore
- AutoUpdate.Core
- AutoUpdate.MD5
- AutoUpdate.Differential
- AutoUpdate.ZIP
- AutoUpdate.MinimalService
- AutoUpdate.Test



## 📍2022-02-16

Current Framework Support

Theoretical Support: [Microsoft .NET Standard Documentation](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

| Framework Name                        | Support Status                      |
| ------------------------------------- | ----------------------------------- |
| .NET Core 2.0                         | Supported                           |
| .NET 5, 6, 7                          | Supported                           |
| .NET Framework 4.6.1                  | Supported                           |
| Mono 5.4                              | Theoretically supported, unverified |
| Xamarin.iOS                           | Theoretically supported, unverified |
| Xamarin.Mac                           | Theoretically supported, unverified |
| Xamarin.Android                       | Theoretically supported, unverified |
| Universal Windows Platform 10.0.16299 | Theoretically supported, unverified |
| Unity 2018.1                          | Theoretically supported, unverified |

| UI Framework Name | Support Status                               |
| ----------------- | -------------------------------------------- |
| WPF               | Supported                                    |
| UWP               | Unverified, awaiting feedback                |
| MAUI              | Not supported, in progress for compatibility |
| Avalonia          | Unverified, awaiting feedback                |
| WinUI             | Unverified, awaiting feedback                |
| Console           | Supported                                    |

| Operating System Name | Support Status |
| --------------------- | -------------- |
| Windows               | Supported      |
| Linux                 | Unverified     |
| Mac                   | Unverified     |
| iOS                   | Not supported  |
| Android               | Not supported  |



## 📍2021-08-14

Notice

- The GeneralUpdate website is expected to be developed using Blazor. It will introduce component structure, update processes, quick start guides, parameters, and more.

![Image](imgs/udpate_flow.jpg)

  1. When the main program starts, it checks if the updater needs to be updated.
  2. If an update is needed, it uploads the version number of the updater and updates it incrementally.
  3. Once the updater is updated or if no update is needed, it checks if the main program needs an update. If so, the updater is launched.
  4. Requests the main program update version.
  5. Receives multiple update packages for the main version and updates incrementally.
  6. After updating, the updater is closed, and the main program is started.

![Image](imgs/supporting_relationships.jpg)

- The above update and download processes support resuming from breakpoints and incremental updates.
- Incremental download functionality is sorted by the release date of versions, with earlier versions being updated first (see the SQL script field content in the source code for details).
- The maximum tested update content for incremental downloads is 1GB.

2. Nuget

- (New) https://www.nuget.org/packages/GeneralUpdate.Common/
- (New) https://www.nuget.org/packages/GeneralUpdate.ClientCore/
- (New) https://www.nuget.org/packages/GeneralUpdate.AspNetCore/
- https://www.nuget.org/packages/GeneralUpdate.Zip/
- https://www.nuget.org/packages/GeneralUpdate.Single/
- (Update) https://www.nuget.org/packages/GeneralUpdate.Core/

3. Issues & Git, Gitee

Feel free to raise issues at the following addresses. Please describe the cause of the anomaly or defect details as clearly as possible. The checking cycle is every Friday.

- https://gitee.com/Juster-zhu/GeneralUpdate
- https://github.com/WELL-E/AutoUpdater/tree/autoupdate2
- https://github.com/WELL-E/AutoUpdater/issues
- https://gitee.com/Juster-zhu/GeneralUpdate/issues

4. New

- GeneralUpdate.Core adds incremental update functionality.
- GeneralUpdate.Core introduces new events: ExceptionEvent, MutiDownloadStatisticsEvent, MutiDownloadErrorEvent, MutiDownloadCompletedEvent, MutiDownloadProgressEvent, MutiAllDownloadCompletedEvent.
- GeneralUpdate.Core adds the RemoteAddressBase64 method.
- A new ClientParameter class is added for inter-process communication to pass parameters between components.
- A new component, GeneralUpdate.AspNetCore, is added, which provides update version information based on update type and supports pipeline dependency injection. However, you need to write your own methods for database queries.
- A new component, GeneralUpdate.ClientCore, is added:
  - (1) It can update the version of the update component itself.
  - (2) It supports incremental updates for update components (multiple update packages can be downloaded simultaneously).
  - (3) It simplifies the launch of the updater, eliminating the previous cumbersome process of starting and passing parameters.
- A new component, GeneralUpdate.Common, is added. This library integrates all common classes and helper methods used within the components (this component is mandatory, and its update frequency is very low, so it's not recommended to package it in update packages).
- A new MySQL script is added to create the update_version table for the GeneralUpdate.AspNetCore server.

5. Remove

- All notification events in GeneralUpdate.Core are replaced with MutiXXXXEvent.
- The method RemoteAddress in GeneralUpdateBootstrap for passing parameters through processes is removed from GeneralUpdate.Core.
- The feature for rolling back failed updates is removed, as it caused fatal exceptions due to insufficient permissions when rolling back updates on the C drive. This feature may be reopened in the future.

6. Fix

- Fixed the issue with HTTPS SSL access failures.
- Fixed notification exceptions related to event registration with begininvoke in other .NET framework versions.
- Corrected multiple class name spelling errors.
- Removed unnecessary fields from several Model classes.

7. Quick Start

**(1) Example GeneralUpdate.ClientCore**

```csharp
// Client version.
var mainVersion = "1.1.1";
var mianType = 1;

// Updater version
clientParameter = new ClientParameter();
clientParameter.ClientVersion = "1.1.1";
clientParameter.ClientType = 2;
clientParameter.AppName = "AutoUpdate.ConsoleApp";
clientParameter.MainAppName = "AutoUpdate.Test";
clientParameter.InstallPath = @"D:\update_test";
clientParameter.UpdateLogUrl = "https://www.baidu.com/";
clientParameter.ValidateUrl = $"https://127.0.0.1:5001/api/update/getUpdateValidate/{ clientParameter.ClientType }/{ clientParameter.ClientVersion }";
clientParameter.UpdateUrl = $"https://127.0.0.1:5001/api/update/getUpdateVersions/{ clientParameter.ClientType }/{ clientParameter.ClientVersion }";
clientParameter.MainValidateUrl = $"https://127.0.0.1:5001/api/update/getUpdateValidate/{ mianType }/{ mainVersion }";
clientParameter.MainUpdateUrl = $"https://127.0.0.1:5001/api/update/getUpdateVersions/{ mianType }/{ mainVersion }";

generalClientBootstrap = new GeneralClientBootstrap();
generalClientBootstrap.MutiDownloadProgressChanged += OnMutiDownloadProgressChanged;
generalClientBootstrap.MutiDownloadStatistics += OnMutiDownloadStatistics;
generalClientBootstrap.MutiDownloadCompleted += OnMutiDownloadCompleted;
generalClientBootstrap.MutiAllDownloadCompleted += OnMutiAllDownloadCompleted;
generalClientBootstrap.MutiDownloadError += OnMutiDownloadError;
generalClientBootstrap.Exception += OnException;
generalClientBootstrap.Config(clientParameter).
    Strategy<ClientStrategy>();
await generalClientBootstrap.LaunchTaskAsync();
```

**(2) Example GeneralUpdate.Core**

```csharp
static void Main(string[] args)
{
    var resultBase64 = args[0];
    var bootstrap = new GeneralUpdateBootstrap();
    bootstrap.Exception += OnException;
    bootstrap.MutiDownloadError += OnMutiDownloadError;
    bootstrap.MutiDownloadCompleted += OnMutiDownloadCompleted;
    bootstrap.MutiDownloadStatistics += OnMutiDownloadStatistics;
    bootstrap.MutiDownloadProgressChanged += OnMutiDownloadProgressChanged;
    bootstrap.MutiAllDownloadCompleted += OnMutiAllDownloadCompleted;
    bootstrap.Strategy<DefaultStrategy>().
        Option(UpdateOption.DownloadTimeOut, 60).
        RemoteAddressBase64(resultBase64).
        LaunchAsync();
}
```

**(3) Example GeneralUpdate.AspNetCore**

```csharp
Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddSingleton<IUpdateService, GeneralUpdateService>();
}

UpdateController.cs

private readonly ILogger<UpdateController> _logger;
private readonly IUpdateService _updateService;

public UpdateController(ILogger<UpdateController> logger, IUpdateService updateService)
{
    _logger = logger;
    _updateService = updateService;
}

/// <summary>
/// https://localhost:5001/api/update/getUpdateVersions/1/1.1.1
/// </summary>
/// <param name="clientType"> 1:ClientApp 2:UpdateApp</param>
/// <param name="clientVersion"></param>
/// <returns></returns>
[HttpGet("getUpdateVersions/{clientType}/{clientVersion}")]
public async Task<IActionResult> GetUpdateVersions(int clientType, string clientVersion)
{
    _logger.LogInformation("Client request 'GetUpdateVersions'.");
    var resultJson = await _updateService.UpdateVersionsTaskAsync(clientType, clientVersion, UpdateVersions);
    return Ok(resultJson);
}

/// <summary>
/// https://localhost:5001/api/update/getUpdateValidate/1/1.1.1
/// </summary>
/// <param name="clientType"> 1:ClientApp 2:UpdateApp</param>
/// <param name="clientVersion"></param>
/// <returns></returns>
[HttpGet("getUpdateValidate/{clientType}/{clientVersion}")]
public async Task<IActionResult> GetUpdateValidate(int clientType, string clientVersion)
{
    _logger.LogInformation("Client request 'GetUpdateValidate'.");
    var lastVersion = GetLastVersion();
    var resultJson = await _updateService.UpdateValidateTaskAsync(clientType, clientVersion, lastVersion, true, GetValidateInfos);
    return Ok(resultJson);
}
```



## 📍2021-03-18

Statement

1. This component will support applications developed with the following frameworks: .NET Framework 4.6.1, .NET Core 2.0, and .NET 5.
2. The GeneralUpdate.Single component currently only supports WPF and the .NET Framework.

Additions

1. GeneralUpdate.Core version 3.2.1 introduces a new feature for rollback on update failure.
2. A new component, GeneralUpdate.Zip version 1.0.0, has been added, providing functionality for compressing and decompressing files, and it can be used independently.

Fixes and Modifications

1. Fixed the issue where events were not being notified during multithreaded operations.
2. The component framework version has been changed from .NET Framework 4.5.2 to .NET Standard 2.0. (Supported frameworks reference: https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

Removals

1. Removed the dependency on the third-party component 7zip.
2. Removed the RegistryUtil utility class.

GeneralUpdate Quick start

```c#
        args = new string[6] {
            "0.0.0.0",
            "1.1.1.1",
            "https://github.com/WELL-E",
             "http://192.168.50.225:7000/update.zip",
             @"E:\PlatformPath",
            "509f0ede227de4a662763a4abe3d8470",
             };

        GeneralUpdateBootstrap bootstrap = new GeneralUpdateBootstrap();
        bootstrap.DownloadStatistics += OnDownloadStatistics;
        bootstrap.ProgressChanged += OnProgressChanged;
        bootstrap.Strategy<DefultStrategy>().
            Option(UpdateOption.Format, "zip").
            Option(UpdateOption.MainApp, "your application name").
            Option(UpdateOption.DownloadTimeOut,60).
            RemoteAddress(args).
            Launch();
```



## 📍2020-08-30

New Additions

1. In the new release, GeneralUpdate.Core version 2.1.0 adds support for resuming downloads.
2. In the new release, a new component, GeneralUpdate.Single version 1.0.0, has been added. It provides singleton functionality to prevent multiple instances of the auto-update program from starting.

Update Process

1. The client program starts and retrieves update information from the server, then parses and compares it to determine if an update is needed.

2. Parses process parameters, such as local version number, latest version number, download URL, extraction path, installation path, etc.

3. The client program launches the updater (GeneralUpdate) and closes itself.

4. The auto-update program (GeneralUpdate) performs the following based on the passed update information: (1) download, (2) MD5 checksum, (3) extraction, (4) delete update files, (5) replace update files, (6) close the update program itself, (7) start the client program.

5. Update is completed.

Inter-process Communication

```
using System;
using System.Diagnostics;
using System.ComponentModel;

namespace MyProcessSample
{
    class MyProcess
    {
        //此段代码来自于msdn
        // Opens the Internet Explorer application.
        void OpenApplication(string myFavoritesPath)
        {
            // Start Internet Explorer. Defaults to the home page.
            Process.Start("IExplore.exe");

            // Display the contents of the favorites folder in the browser.
            Process.Start(myFavoritesPath);
        }

        // Opens urls and .html documents using Internet Explorer.
        void OpenWithArguments()
        {
            // url's are not considered documents. They can only be opened
            // by passing them as arguments.
            Process.Start("IExplore.exe", "www.northwindtraders.com");

            // Start a Web page using a browser associated with .html and .asp files.
            Process.Start("IExplore.exe", "C:\\myPath\\myFile.htm");
            Process.Start("IExplore.exe", "C:\\myPath\\myFile.asp");
        }

        // Uses the ProcessStartInfo class to start new processes,
        // both in a minimized mode.
        void OpenWithStartInfo()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("IExplore.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;

            Process.Start(startInfo);

            startInfo.Arguments = "www.northwindtraders.com";

            Process.Start(startInfo);
        }

        static void Main()
        {
            // Get the path that stores favorite links.
            string myFavoritesPath =
                Environment.GetFolderPath(Environment.SpecialFolder.Favorites);

            MyProcess myProcess = new MyProcess();

            myProcess.OpenApplication(myFavoritesPath);
            myProcess.OpenWithArguments();
            myProcess.OpenWithStartInfo();
        }
    }
}
```

Using GeneralUpdate.Core-2.1.0

```csharp
#region Launch1
args = new string[6] {
    "0.0.0.0",
    "1.1.1.1",
    "https://github.com/WELL-E",
    "http://192.168.50.225:7000/update.zip",
    @"E:\PlatformPath",
    "509f0ede227de4a662763a4abe3d8470",
};

GeneralUpdateBootstrap bootstrap = new GeneralUpdateBootstrap(); // Auto-update bootstrap class
bootstrap.DownloadStatistics += OnDownloadStatistics; // Download progress notification event
bootstrap.ProgressChanged += OnProgressChanged; // Update progress notification event
bootstrap.Strategy<DefultStrategy>() // Register strategy, can customize update process
    .Option(UpdateOption.Format, "zip") // Specify the format of the update package, currently only zip is supported
    .Option(UpdateOption.MainApp, "your application name") // Specify the main program name to start after the update, no need to add .exe, just the name
    .Option(UpdateOption.DownloadTimeOut, 60) // Download timeout (seconds), default is 30 seconds if not specified
    .RemoteAddress(args) // Use the previous parameter array
    .Launch(); // Start the update
#endregion

#region Launch2

/*
 * Launch2
 * Added a second launch method
 * Process:
 * 1. Specify the update address, e.g., https://api.com/GeneralUpdate?version=1.0.0.1, passing the current client version in the web API
 * 2. If an update is needed, the API returns all update information (details in /Models/UpdateInfo.cs)
 * 3. After obtaining the update information, start the HTTP request for the update package
 * 4. Download
 * 5. Extract
 * 6. Update local files
 * 7. Close the update program
 * 8. Start the configured main program
 * The update program must be placed in the same directory as the main program
 */

//GeneralUpdateBootstrap bootstrap2 = new GeneralUpdateBootstrap();
//bootstrap2.DownloadStatistics += OnDownloadStatistics;
//bootstrap2.ProgressChanged += OnProgressChanged;
//bootstrap2.Strategy<DefultStrategy>()
//    .Option(UpdateOption.Format, "zip")
//    .Option(UpdateOption.MainApp, "")
//    .Option(UpdateOption.DownloadTimeOut, 60) // Download timeout (seconds), default is 30 seconds if not specified
//    .RemoteAddress(@"https://api.com/GeneralUpdate?version=1.0.0.1") // Specify update address
//    .Launch();

#endregion

private static void OnProgressChanged(object sender, ProgressChangedEventArgs e)
{
    if (e.Type == ProgressType.Updatefile)
    {
        var str = $"Currently updating file: {e.ProgressValue}, total files to update: {e.TotalSize}";
        Console.WriteLine(str);
    }

    if (e.Type == ProgressType.Done)
    {
        Console.WriteLine("Update completed");
    }
}

private static void OnDownloadStatistics(object sender, DownloadStatisticsEventArgs e)
{
    Console.WriteLine($"Download speed: {e.Speed}, remaining time: {e.Remaining.Minute}:{e.Remaining.Second}");
}
```

Using GeneralUpdate.Single-1.0.0

```csharp
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, ISingleInstanceApp
{
    private const string AppId = "{7F280539-0814-4F9C-95BF-D2BB60023657}";

    [STAThread]
    protected override void OnStartup(StartupEventArgs e)
    {
        string[] resultArgs = null;

        if (e.Args == null || e.Args.Length == 0)
        {
            resultArgs = new string[6] {
                "0.0.0.0",
                "1.1.1.1",
                "https://github.com/WELL-E",
                "http://192.168.50.225:7000/update.zip",
                @"E:\PlatformPath",
                "509f0ede227de4a662763a4abe3d8470",
            };
        }
        else
        {
            resultArgs = e.Args;
        }

        if (resultArgs.Length != 6) return;
        if (SingleInstance<App>.InitializeAsFirstInstance(AppId))
        {
            var win = new MainWindow();
            var vm = new MainViewModel(resultArgs, win.Close);
            win.DataContext = vm;

            var application = new App();
            application.InitializeComponent();
            application.Run(win);
            SingleInstance<App>.Cleanup();
        }
    }

    public bool SignalExternalCommandLineArgs(IList<string> args)
    {
        if (this.MainWindow.WindowState == WindowState.Minimized)
        {
            this.MainWindow.WindowState = WindowState.Normal;
        }
        this.MainWindow.Activate();

        return true;
    }
}
```

Q&A

1. What should be done if there are multiple version iterations and cross-version updates?

   As long as it's not a framework-level update, updates can be applied. Regardless of how many iterations or versions you cross, you just need to place the final package on the server. There is no incremental update involved here, so if there are many updates, you can simply overwrite all the new files.

2. Is GeneralUpdate integrated with the client as a whole?

   No, GeneralUpdate is a program independent of the client.

3. Can it perform incremental updates, automatic rollback on failure, or update local data or configuration files?

   Currently, it cannot. (This feature is in the development plan.)

4. How does GeneralUpdate perform updates?

   The update method involves overwriting the existing client files locally.



## 📍2020-05-03

Here is the translation of your content into English:

1. Concise launch code as follows: Launch1 Launch2

| Name                   | Type             | Remarks                                                      |
| ---------------------- | ---------------- | ------------------------------------------------------------ |
| UpdateOption.Format    | Config Parameter | Compression format of the update package (currently only supports zip) |
| UpdateOption.MainApp   | Config Parameter | Name of the main application to launch after the update      |
| DownloadStatistics     | Event            | Event notification for update package download               |
| ProgressChanged        | Event            | Event notification for update progress                       |
| Strategy()             | Method           | Strategy injection                                           |
| RemoteAddress()        | Method           | Remote address configuration, pass in args[] if not available |
| Launch                 | Method           | Start update                                                 |
| GeneralUpdateBootstrap | Class            | Update bootstrap class                                       |

2. Added Strategy (update strategy), which is open for everyone to freely extend the update method without modifying the source code, not limited to the default update strategy.

3. When updating local files, there will be an update notification event, clearly indicating the total number of files to update and the current file being updated.

4. Added new update statuses: Check (check for updates), Download (download update package), UpdateFile (update files), Done (update completed), Fail (update failed). Developers can directly use a series of enumeration values to determine the current running status and make corresponding handling.

5. Added validation for several startup configuration parameters.

```csharp
if (args != null)
{
    if (args.Length == 0)
    {
        throw new NullReferenceException("Args does not contain any elements.");
    }

    if (args.Length > elementNum)
    {
        throw new Exception($"The number of args cannot be greater than { elementNum }");
    }
}

if (string.IsNullOrWhiteSpace(PacketName))
{
    throw new NullReferenceException("packet name not set");
}

if (string.IsNullOrWhiteSpace(DownloadPath))
{
    throw new NullReferenceException("download path not set");
}

if (string.IsNullOrWhiteSpace(InstallPath))
{
    throw new NullReferenceException("install path not set");
}

if (string.IsNullOrWhiteSpace(MD5))
{
    throw new NullReferenceException("MD5 not set");
}
```

6. Download update progress notification event will provide remaining download time (Remaining) and download speed (Speed).

```csharp
#region Launch1

args = new string[6] {
    "0.0.0.0",
    "1.1.1.1",
    "https://github.com/WELL-E",
    "http://192.168.50.225:7000/update.zip",
    @"E:\PlatformPath",
    "509f0ede227de4a662763a4abe3d8470",
};

GeneralUpdateBootstrap bootstrap = new GeneralUpdateBootstrap();
bootstrap.DownloadStatistics += OnDownloadStatistics;
bootstrap.ProgressChanged += OnProgressChanged;
bootstrap.Strategy<DefultStrategy>().
    Option(UpdateOption.Format, "zip"). // Specify the format of the update package, currently only supports zip
    Option(UpdateOption.MainApp, "your application name"). // Specify the name of the main application to launch after the update, no need to add .exe, just the name
    RemoteAddress(args). // Retains the previous parameter array collection
    Launch();

#endregion

#region Launch2

/*
 * Launch2
 * Added a second launch method
 * Process:
 * 1. Specify the update address, e.g., https://api.com/GeneralUpdate?version=1.0.0.1, passing the current client version to the web API
 * 2. If an update is needed, the API returns all update information (refer to /Models/UpdateInfo.cs for details)
 * 3. After obtaining the update information, start an HTTP request for the update package
 * 4. Download
 * 5. Unzip
 * 6. Update local files
 * 7. Close the update program
 * 8. Start the configured main program
 * The update program must be in the same directory as the main program
 */

// GeneralUpdateBootstrap bootstrap2 = new GeneralUpdateBootstrap();
// bootstrap2.DownloadStatistics += OnDownloadStatistics;
// bootstrap2.ProgressChanged += OnProgressChanged;
// bootstrap2.Strategy<DefultStrategy>().
//     Option(UpdateOption.Format, "zip").
//     Option(UpdateOption.MainApp, "KGS.CPP").
//     RemoteAddress(@"https://api.com/GeneralUpdate?version=1.0.0.1"). // Specify the update address
//     Launch();

#endregion

private static void OnProgressChanged(object sender, ProgressChangedEventArgs e)
{
    if (e.Type == ProgressType.Updatefile)
    {
        var str = $"Currently updating file: {e.ProgressValue}, Total number of files to update: {e.TotalSize}";
        Console.WriteLine(str);
    }

    if (e.Type == ProgressType.Done)
    {
        Console.WriteLine("Update completed");
    }
}

private static void OnDownloadStatistics(object sender, DownloadStatisticsEventArgs e)
{
    Console.WriteLine($"Download speed: {e.Speed}, Remaining time: {e.Remaining.Minutes}:{e.Remaining.Seconds}");
}
```

Let me know if you need further assistance!
