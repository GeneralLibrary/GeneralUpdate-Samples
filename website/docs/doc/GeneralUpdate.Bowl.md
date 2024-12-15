---

sidebar_position: 3

---

#### Background Story

![](imgs\bowl.jpg)

Bowl is an orange cat owned by the author Juster. The reason for naming an orange cat or component "Bowl" is because orange cats tend to get fat easily. Juster hopes it will eat only a small bowl at a time, hence the name "Little Bowl." In the GeneralUpdate system, if the upgrade process encounters significant issues, Bowl will step in to restore the situation and analyze the reasons for failure.

![](imgs\mybowl.jpg)

### Definition

Namespace: GeneralUpdate.Bowl

Assembly: GeneralUpdate.Bowl.dll

This component is an independent process launched before the end of the upgrade process. It starts the main client application and monitors its normal operation before the process ends.

```c#
public sealed class Bowl
```

NuGet Installation

```shell
NuGet\Install-Package GeneralUpdate.Bowl -Version 1.0.0
```

### Example

The following example defines a method that includes the use of Bowl.

```c#
var installPath = AppDomain.CurrentDomain.BaseDirectory;
var lastVersion = "1.0.0.3";
var processInfo = new MonitorParameter
{
    ProcessNameOrId = "JsonTest.exe",
    DumpFileName = $"{lastVersion}_fail.dmp",
    FailFileName = $"{lastVersion}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", lastVersion),
    BackupDirectory = Path.Combine(installPath, lastVersion),
    WorkModel = "Normal"
};
Bowl.Launch(processInfo);
```

### Capture

If a crash is detected, the following files will be generated in the running directory:

- 📒 Dump file (1.0.0.*_fail.dmp)
- 📒 Upgrade package version information (1.0.0.*_fail.json)
- 📒 Driver information (driverInfo.txt)
- 📒 Operating system/hardware information (systeminfo.txt)
- 📒 System event log (systemlog.evtx)

These will be exported to the "fail" directory, categorized by version number.

![](imgs\crash.jpg)

#### (1) x.0.0.*_fail.dmp

![](imgs\dump.png)

#### (2) x.0.0.*_fail.json

```json
{
	"Parameter": {
		"TargetPath": "D:\\github_project\\GeneralUpdate\\src\\c#\\Generalupdate.CatBowl\\bin\\Debug\\net9.0\\",
		"FailDirectory": "D:\\github_project\\GeneralUpdate\\src\\c#\\Generalupdate.CatBowl\\bin\\Debug\\net9.0\\fail\\1.0.0.3",
		"BackupDirectory": "D:\\github_project\\GeneralUpdate\\src\\c#\\Generalupdate.CatBowl\\bin\\Debug\\net9.0\\1.0.0.3",
		"ProcessNameOrId": "JsonTest.exe",
		"DumpFileName": "1.0.0.3_fail.dmp",
		"FailFileName": "1.0.0.3_fail.json",
		"WorkModel": "Normal",
		"ExtendedField": null
	},
	"ProcdumpOutPutLines": [
        "ProcDump v11.0 - Sysinternals process dump utility",
        "Copyright (C) 2009-2022 Mark Russinovich and Andrew Richards",
        "Sysinternals - www.sysinternals.com", 
        "Process:               JsonTest.exe (19712)", 
        "Process image:  D:\\github_project\\GeneralUpdate\\src\\c#\\Generalupdate.CatBowl\\bin\\Debug\\net9.0\\JsonTest.exe", "CPU threshold:         n/a", 
        "Performance counter:   n/a", "Commit threshold:      n/a",
        "Threshold seconds:     n/a", "Hung window check:     Disabled", "Log debug strings:     Disabled", 
        "Exception monitor:     Unhandled", "Exception filter:      [Includes]", 
        "                       *", 
        "                       [Excludes]", 
        "Terminate monitor:     Disabled", 
        "Cloning type:          Disabled", 
        "Concurrent limit:      n/a", 
        "Avoid outage:          n/a", 
        "Number of dumps:       1", 
        "Dump folder:           D:\\github_project\\GeneralUpdate\\src\\c#\\Generalupdate.CatBowl\\bin\\Debug\\net9.0\\fail\\1.0.0.3\\", 
        "Dump filename/mask:    1.0.0.3_fail", 
        "Queue to WER:          Disabled", "Kill after dump:       Disabled", 
        "Press Ctrl-C to end monitoring without terminating the process.", 
        "[19:05:23] Exception: E0434352.CLR", "[19:05:23] Unhandled: E0434352.CLR", 
        "[19:05:23] Dump 1 initiated: D:\\github_project\\GeneralUpdate\\src\\c#\\Generalupdate.CatBowl\\bin\\Debug\\net9.0\\fail\\1.0.0.3\\1.0.0.3_fail.dmp", 
        "[19:05:23] Dump 1 writing: Estimated dump file size is 62 MB.", 
        "[19:05:23] Dump 1 complete: 62 MB written in 0.1 seconds", 
        "[19:05:23] Dump count reached."]
}
```

#### (3) driverInfo.txt

```json
Module Name   Display Name            Description               Driver Type  Start Mode   State       Status       Accept Stop    Accept Pause     Paged Pool Code(Bytes) BSS(Bytes) Link Date               Path                                             Init(Bytes)
============ ====================== ====================== ============= ========== ========== ========== =========== ============ ========== ========== ======= ====================== ================================================ ==========   
360AntiAttac 360Safe Anti Attack Se 360Safe Anti Attack Se Kernel        System     Running    OK         TRUE        FALSE        4,096      36,864     0       9/29/2022 3:45:03 PM   C:\Windows\system32\Drivers\360AntiAttack64.sys  4,096     
360AntiHacke 360Safe Anti Hacker Se 360Safe Anti Hacker Se Kernel        System     Running    OK         TRUE        FALSE        4,096      139,264    0       11/27/2023 3:43:37 PM  C:\Windows\system32\Drivers\360AntiHacker64.sys  8,192     
360AntiHijac 360Safe Anti Hijack Se 360Safe Anti Hijack Se Kernel        System     Running    OK         TRUE        FALSE        4,096      73,728     0       5/8/2024 12:19:52 PM   C:\Windows\system32\Drivers\360AntiHijack64.sys  4,096     
360AntiSteal 360Safe Anti Steal Fil 360Safe Anti Steal Fil Kernel        System     Running    OK         TRUE        FALSE        4,096      20,480     0       4/18/2024 3:58:04 PM   C:\Windows\system32\Drivers\360AntiSteal64.sys   8,192     
360Box64     360Box mini-filter dri 360Box mini-filter dri File System   System     Running    OK         TRUE        FALSE        0          225,280    0       8/7/2024 11:50:19 AM   C:\Windows\system32\DRIVERS\360Box64.sys         12,288    

//...
```

#### (4) systeminfo.txt

```json
Host Name:           ****
OS Name:          Microsoft Windows 11 Pro
OS Version:          10.0.2*** Build 22***
OS Manufacturer:        Microsoft Corporation
OS Configuration:          Standalone Workstation
OS Build Type:      Multiprocessor Free
Registered Owner:      ****@outlook.com
Registered Organization:       
Product ID:          ****-80000-***00-A****
Original Install Date:     11/16/2023, 9:56:28 PM
System Boot Time:     11/26/2024, 9:37:51 PM
System Manufacturer:       ASUS
System Model:         System Product Name
System Type:         x64-based PC
Processor(s):           Installed 1 Processor(s).
                  [01]: Intel** Family * Model *** Stepping * GenuineIntel ~**** Mhz
BIOS Version:        American Megatrends Inc. 1402, 4/1/2022
Windows Directory:     C:\Windows
System Directory:         C:\Windows\system32
Boot Device:         \Device\Ha*****olume1
System Locale:     zh-cn;Chinese (China)
Input Locale:   zh-cn;Chinese (China)
Time Zone:             (UTC+08:00) **，**，*******，****
Total Physical Memory:     16,194 MB
Available Physical Memory:   1,795 MB
Virtual Memory: Max Size: 25,410 MB
Virtual Memory: Available:   9,438 MB
Virtual Memory: In Use: 15,972 MB
Page File Location(s):     D:\****file.sys
Domain:               WORKGROUP
Logon Server:       \\****
Hotfix(s)::       6 Hotfix(s) Installed.
                  [01]: KB504****
                  [02]: KB502****
                  [03]: KB503****
                  [04]: KB503****
                  [05]: KB504****
                  [06]: KB504****
Network Card(s):             3 NIC(s) Installed.
                  [01]: Intel(R) Ethernet Connection (**) I***-V
                      Connection Name:      Ethernet
                      DHCP Enabled:   Yes
                      DHCP Server: 192.168.**.**
                      IP Address
                        [01]: 192.168.**.**
                        [02]: ***::2640:***:****:****
                  [02]: VMware Virtual Ethernet Adapter for VMnet1
                      Connection Name:      VMware Network Adapter VMnet1
                      DHCP Enabled:   Yes
                      DHCP Server: 192.168.**.**
                      IP Address
                        [01]: 192.168.**.**
                        [02]: ***::9b3:***,***:****
                  [03]: VMware Virtual Ethernet Adapter for VMnet8
                      Connection Name:      VMware Network Adapter VMnet8
                      DHCP Enabled:   Yes
                      DHCP Server: 192.168.**.**
                      IP Address
                        [01]: 192.***,***:****
                        [02]: fe80::***:***:***:****
Hyper-V Requirements:     A hypervisor has been detected. Features required for Hyper-V will not be displayed.

//...
```

#### (5) systemlog.evtx

![](imgs\evtx.png)

### Notes

Bowl provides runtime monitoring capabilities and exports relevant error information.

#### Methods

| Method   | Description      |
| -------- | ---------------- |
| Launch() | Start monitoring |

### 🌼Launch()

**Launch Function**

```c#
Launch(MonitorParameter? monitorParameter = null);
```

**Parameters**

```c#
public class MonitorParameter
{   
    // Directory being monitored
    public string TargetPath { get; set; }
    
    // Directory where captured exception information is exported
    public string FailDirectory { get; set; }
    
    // Backup directory
    public string BackupDirectory { get; set; }
    
    // Name or ID of the process being monitored
    public string ProcessNameOrId { get; set; }
 
    // Dump file name
    public string DumpFileName { get; set; }
    
    // Upgrade package version information (.json) file name
    public string FailFileName { get; set; }

    /// <summary>
    /// Upgrade: upgrade mode. This mode is primarily used in conjunction with GeneralUpdate for internal use. Please do not modify it arbitrarily when the default mode is activated.
    /// Normal: Normal mode, This mode can be used independently to monitor a single program. If the program crashes, it will export the crash information.
    /// </summary>
    public string WorkModel { get; set; } = "Upgrade";
}
```

### Applicable To

| Product        | Version       |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
| ASP.NET        | Any           |