---
sidebar_position: 3
---

### 定义

命名空间：GeneralUpdate.Bowl

程序集：GeneralUpdate.Bowl.dll



该组件在升级流程结束之前启动的一个独立进程，它在流程结束之前去启动主客户端应用程序并监控是否正常运行。

```c#
public sealed class Bowl
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.Bowl -Version 9.1.0
```



### 示例

以下示例定义方法，包含Bowl使用。

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



### 捕获

如果监控到崩溃闪退将会在运行目录下生成：

- 📒Dump文件（1.0.0.*_fail.dmp）
- 📒升级包版本信息（1.0.0.*_fail.json）
- 📒驱动信息（driverInfo.txt）
- 📒操作系统信息/硬件信息（systeminfo.txt）
- 📒系统事件日志（systemlog.evtx）

导出到“fail”目录下并根据版本号区分文件夹。

![](imgs\crash.jpg)



#### (1)x.0.0.*_fail.dmp

![](imgs\dump.png)



#### (2)x.0.0.*_fail.json

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



#### (3)driverInfo.txt

```json

模块名       显示名称               描述                   驱动程序类型  启动模式   状态       状态       接受停止    接受暂停     分页缓冲池 代码(字节) BSS(字  链接日期               路径                                             Init(字节)
============ ====================== ====================== ============= ========== ========== ========== =========== ============ ========== ========== ======= ====================== ================================================ ==========   
360AntiAttac 360Safe Anti Attack Se 360Safe Anti Attack Se Kernel        System     Running    OK         TRUE        FALSE        4,096      36,864     0       9/29/2022 3:45:03 PM   C:\Windows\system32\Drivers\360AntiAttack64.sys  4,096     
360AntiHacke 360Safe Anti Hacker Se 360Safe Anti Hacker Se Kernel        System     Running    OK         TRUE        FALSE        4,096      139,264    0       11/27/2023 3:43:37 PM  C:\Windows\system32\Drivers\360AntiHacker64.sys  8,192     
360AntiHijac 360Safe Anti Hijack Se 360Safe Anti Hijack Se Kernel        System     Running    OK         TRUE        FALSE        4,096      73,728     0       5/8/2024 12:19:52 PM   C:\Windows\system32\Drivers\360AntiHijack64.sys  4,096     
360AntiSteal 360Safe Anti Steal Fil 360Safe Anti Steal Fil Kernel        System     Running    OK         TRUE        FALSE        4,096      20,480     0       4/18/2024 3:58:04 PM   C:\Windows\system32\Drivers\360AntiSteal64.sys   8,192     
360Box64     360Box mini-filter dri 360Box mini-filter dri File System   System     Running    OK         TRUE        FALSE        0          225,280    0       8/7/2024 11:50:19 AM   C:\Windows\system32\DRIVERS\360Box64.sys         12,288    

//...
```



#### (4)systeminfo.txt

```json

主机名:           ****
OS 名称:          Microsoft Windows 11 专业版
OS 版本:          10.0.2*** Build 22***
OS 制造商:        Microsoft Corporation
OS 配置:          独立工作站
OS 构建类型:      Multiprocessor Free
注册的所有人:      ****@outlook.com
注册的组织:       
产品 ID:          ****-80000-***00-A****
初始安装日期:     11/16/2023, 9:56:28 PM
系统启动时间:     11/26/2024, 9:37:51 PM
系统制造商:       ASUS
系统型号:         System Product Name
系统类型:         x64-based PC
处理器:           安装了 1 个处理器。
                  [01]: Intel** Family * Model *** Stepping * GenuineIntel ~**** Mhz
BIOS 版本:        American Megatrends Inc. 1402, 4/1/2022
Windows 目录:     C:\Windows
系统目录:         C:\Windows\system32
启动设备:         \Device\Ha*****olume1
系统区域设置:     zh-cn;中文(中国)
输入法区域设置:   zh-cn;中文(中国)
时区:             (UTC+08:00) **，**，*******，****
物理内存总量:     16,194 MB
可用的物理内存:   1,795 MB
虚拟内存: 最大值: 25,410 MB
虚拟内存: 可用:   9,438 MB
虚拟内存: 使用中: 15,972 MB
页面文件位置:     D:\****file.sys
域:               WORKGROUP
登录服务器:       \\****
修补程序:         安装了 6 个修补程序。
                  [01]: KB504****
                  [02]: KB502****
                  [03]: KB503****
                  [04]: KB503****
                  [05]: KB504****
                  [06]: KB504****
网卡:             安装了 3 个 NIC。
                  [01]: Intel(R) Ethernet Connection (**) I***-V
                      连接名:      以太网
                      启用 DHCP:   是
                      DHCP 服务器: 192.168.**.**
                      IP 地址
                        [01]: 192.168.**.**
                        [02]: ***::2640:***:****:****
                  [02]: VMware Virtual Ethernet Adapter for VMnet1
                      连接名:      VMware Network Adapter VMnet1
                      启用 DHCP:   是
                      DHCP 服务器: 192.168.**.**
                      IP 地址
                        [01]: 192.168.**.**
                        [02]: ***::9b3:***,***:****
                  [03]: VMware Virtual Ethernet Adapter for VMnet8
                      连接名:      VMware Network Adapter VMnet8
                      启用 DHCP:   是
                      DHCP 服务器: 192.168.**.**
                      IP 地址
                        [01]: 192.***,***:****
                        [02]: fe80::***:***:***:****
Hyper-V 要求:     已检测到虚拟机监控程序。将不显示 Hyper-V 所需的功能。

//...
```



#### (5)systemlog.evtx

![](imgs\evtx.png)



### 注解

Bowl提供运行监控功能，并导出相关错误信息 。



#### 方法

| Method   |          |
| -------- | -------- |
| Launch() | 启动监控 |



### 🌼Launch()

**启动函数**

```c#
Launch(MonitorParameter? monitorParameter = null);
```

**参数**

```c#
public class MonitorParameter
{   
    //被监控的目录
    public string TargetPath { get; set; }
    
    //捕获的异常信息导出到的目录
    public string FailDirectory { get; set; }
    
    //备份目录
    public string BackupDirectory { get; set; }
    
    //监控进程名称或者进程id
    public string ProcessNameOrId { get; set; }
 
    //dump文件名
    public string DumpFileName { get; set; }
    
    //升级包版本信息（.json）文件名
    public string FailFileName { get; set; }

    /// <summary>
    /// Upgrade: upgrade mode. This mode is primarily used in conjunction with GeneralUpdate for internal use. Please do not modify it arbitrarily when the default mode is activated.
    /// Normal: Normal mode,This mode can be used independently to monitor a single program. If the program crashes, it will export the crash information.
    /// </summary>
    public string WorkModel { get; set; } = "Upgrade";
}
```



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
| ASP.NET        | Any           |