---
sidebar_position: 6
---

### System Information

#### (1) Windows Platform

When an update fails, it might not be clear whether the failure is due to the operating system or other reasons. In such cases, you can use PsInfo to export information about the current operating system, which can help developers troubleshoot the issue. *PsInfo* is a command-line tool that can be used to collect critical information about local or remote Windows NT/2000 systems, including installation type, kernel version, registered organization and owner, number and type of processors, amount of physical memory, system installation date, and expiration date (if it is a trial version).

#### Using PsInfo

By default, *PsInfo* displays information about the local system. Specify a remote computer name to retrieve information from a remote system. Since *PsInfo* relies on remote registry access to gather its data, the remote system must have the remote registry service running, and the account running *PsInfo* must have access to the HKLM\System section of the remote registry.

To assist in automating Service Pack updates, *PsInfo* returns the system's Service Pack number (e.g., 0 for no Service Pack, 1 for SP 1, etc.).

**Usage: psinfo [[\\computer[,computer[,..] | @file [-u user [-p psswd]]] [-h] [-s] [-d] [-c [-t delimiter]] [filter]**

| Parameter      | Description                                                  |
| -------------- | ------------------------------------------------------------ |
| **\\computer** | Execute the command on the specified remote computer(s). If the computer name is omitted, the command runs on the local system. If a wildcard (\\*) is specified, the command runs on all computers in the current domain. |
| **@file**      | Run the command on each computer listed in the specified text file. |
| **-u**         | Specify an optional username for logging onto the remote computer. |
| **-p**         | Specify an optional password for the username. If omitted, you will be prompted to enter a hidden password. |
| **-h**         | Show a list of installed hotfixes.                           |
| **-s**         | Show a list of installed applications.                       |
| **-d**         | Show disk volume information.                                |
| **-c**         | Print in CSV format.                                         |
| **-t**         | The default delimiter for the -c option is a comma, but you can specify a different character. |
| **filter**     | PsInfo will only display data for fields that match the filter. For example, "psinfo service" will only list the service pack field. |

#### Example Output

When you run PsInfo, it will output information about the system in a structured format. This information can be used to diagnose and address any issues that may arise during the update process.

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



References：https://learn.microsoft.com/zh-cn/sysinternals/downloads/psinfo