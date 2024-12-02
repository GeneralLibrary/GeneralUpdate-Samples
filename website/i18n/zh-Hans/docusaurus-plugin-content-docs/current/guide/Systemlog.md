---
sidebar_position: 5
---

### System log 系统日志

升级失败时可能会出现运行启动失败、驱动安装失败等问题。那么可以使用Sysmon工具导出系统事件日志。*系统监视器* (*Sysmon*) 是一项 Windows 系统服务，也是一个设备驱动程序，一旦安装在系统上，就会在系统重新启动后一直驻留，以监视系统活动并将其记录到 Windows 事件日志中。 它提供有关进程创建、网络连接和文件创建时间更改的详细信息。 通过使用 [Windows 事件收集](https://msdn.microsoft.com/library/windows/desktop/bb427443(v=vs.85).aspx)或 [SIEM](https://en.wikipedia.org/wiki/security_information_and_event_management) 代理收集生成的事件，然后对事件进行分析，你可识别恶意或异常活动，并了解入侵者和恶意软件如何在网络上运行。 该服务作为[受保护的进程](https://learn.microsoft.com/windows/win32/services/protecting-anti-malware-services-#system-protected-process)运行，从而禁止广泛的用户模式交互。



### Sysmon 功能概述

*Sysmon* 包括以下功能：

- 记录当前进程和父进程中使用完整命令行创建的进程。
- 记录使用 SHA1（默认）、MD5、SHA256 或 IMPHASH 的进程映像文件的哈希。
- 可以同时使用多个哈希。
- 在进程内创建事件之中包含一个进程 GUID，当 Windows 重新使用进程 ID 时，允许事件的相关性。
- 在每个事件中包含会话 GUID，允许同一登录会话上事件的相关性。
- 记录驱动程序或 DLL 的加载及其签名与哈希。
- 记录磁盘和卷的原始读取访问打开次数。
- （可选）记录网络连接，包括每个连接的源进程、IP 地址、端口数量、主机名和端口名称。
- 检测文件创建时间的更改，以了解文件真正创建的时间。 修改文件创建时间戳是恶意软件惯用的伎俩来掩盖其轨道。
- 如果注册表中发生更改，则自动化重新加载配置。
- 进行规则筛选以动态包含或不包含某些事件。
- 在启动进程之初生成事件，以捕获相当复杂的内核模式恶意软件进行的活动。



#### （1）Windows平台

下载地址： https://download.sysinternals.com/files/Sysmon.zip



#### （2）Linux平台

下载地址：https://github.com/Sysinternals/SysmonForLinux



官方文档：https://learn.microsoft.com/zh-cn/sysinternals/downloads/sysmon
