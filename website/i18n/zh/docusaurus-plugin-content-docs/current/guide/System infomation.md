---
sidebar_position: 6
---

### System infomation 系统信息

#### （1）Windows平台

当更新失败时，并不清楚是因为操作系统的原因还是其他原因导致的启动失败。这个时候可以使用PsInfo导出当前操作系统的信息，供开发人员进行问题排查。*PsInfo* 是一个命令行工具，它可用于收集有关本地或远程 Windows NT/2000 系统的关键信息，包括安装类型、内核版本、已注册的组织和所有者、处理器数量及其类型、物理内存量、系统的安装日期以及到期日期（如果为试用版）。

#### 使用 PsInfo

默认情况下，*PsInfo* 会显示本地系统的信息。 指定远程计算机名称以从远程系统获取信息。 由于 *PsInfo* 依赖于远程注册表访问来获取其数据，因此远程系统必须运行远程注册表服务，并且运行 *PsInfo* 的帐户必须有权访问远程注册表的 HKLM\System 部分。

为了帮助自动更新 Service Pack，*PsInfo* 会返回系统的 Service Pack 数的值（例如 0 表示无 Service Pack，1 表示 SP 1 等）。

**用法: psinfo [[\\computer[,computer[,..] | @file [-u user
[-p psswd]]] [-h] [-s] [-d] [-c [-t delimiter]] [filter]**

| 参数           | 说明                                                         |
| :------------- | :----------------------------------------------------------- |
| **\\computer** | 在指定的远程计算机上执行命令。 如果省略计算机名称，则命令在本地系统上运行，如果指定通配符 (\\*)，则命令将在当前域中的所有计算机上运行。 |
| **@file**      | 在指定的文本文件中列出的每台计算机上运行命令。               |
| **-u**         | 指定登录远程计算机的可选用户名。                             |
| **-p**         | 指定用户名的可选密码。 如果省略此内容，系统将提示你输入隐藏密码。 |
| **-h**         | 显示已安装的修补程序的列表。                                 |
| **-s**         | 显示已安装的应用程序的列表。                                 |
| **-d**         | 显示磁盘卷信息。                                             |
| **-c**         | 以 CSV 格式打印。                                            |
| **-t**         | -c 选项的默认分隔符为逗号，但可以使用指定的字符替代。        |
| **filter**     | Psinfo 将仅显示与筛选器匹配的字段的数据。 例如，“psinfo service”仅列出 service pack 字段。 |

#### 示例输出

```c#
C:\> psinfo \\development -h -d

PsInfo v1.6 - local and remote system information viewer
Copyright (C) 2001-2004 Mark Russinovich
Sysinternals - www.sysinternals.com

    System information for \\DEVELOPMENT:
    Uptime: 28 days, 0 hours, 15 minutes, 12 seconds
    Kernel version: Microsoft Windows XP, Multiprocessor Free
    Product type Professional
    Product version: 5.1
    Service pack: 0
    Kernel build number: 2600
    Registered organization: Sysinternals
    Registered owner: Mark Russinovich
    Install date: 1/2/2002, 5:29:21 PM
    Activation status: Activated
    IE version: 6.0000
    System root: C:\WINDOWS
    Processors: 2
    Processor speed: 1.0 GHz
    Processor type: Intel Pentium III
    Physical memory: 1024 MB
    Volume Type Format Label Size Free Free
    A: Removable 0%
    C: Fixed NTFS WINXP 7.8 GB 1.3 GB 16%
    D: Fixed NTFS DEV 10.7 GB 809.7 MB 7%
    E: Fixed NTFS SRC 4.5 GB 1.8 GB 41%
    F: Fixed NTFS MSDN 2.4 GB 587.5 MB 24%
    G: Fixed NTFS GAMES 8.0 GB 1.0 GB 13%
    H: CD-ROM CDFS JEDIOUTCAST 633.6 MB 0%
    I: CD-ROM 0%
    Q: Remote 0%
    T: Fixed NTFS Test 502.0 MB 496.7 MB 99%
    OS Hot Fix Installed
    Q147222 1/2/2002
    Q309521 1/4/2002
    Q311889 1/4/2002
    Q313484 1/4/2002
    Q314147 3/6/2002
    Q314862 3/13/2002
    Q315000 1/8/2002
    Q315403 3/13/2002
    Q317277 3/20/2002
```



官方文档：https://learn.microsoft.com/zh-cn/sysinternals/downloads/psinfo