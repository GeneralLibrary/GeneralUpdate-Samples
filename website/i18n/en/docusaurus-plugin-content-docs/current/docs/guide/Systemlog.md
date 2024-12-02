---
sidebar_position: 5
---

### System Log

When an upgrade fails, issues such as startup failures or driver installation failures may occur. The Sysmon tool can be used to export system event logs. *System Monitor* (*Sysmon*) is a Windows system service and device driver that, once installed on a system, remains resident across system reboots to monitor and log system activity to the Windows event log. It provides detailed information about process creations, network connections, and file creation time changes. By using [Windows Event Collection](https://msdn.microsoft.com/library/windows/desktop/bb427443(v=vs.85).aspx) or [SIEM](https://en.wikipedia.org/wiki/security_information_and_event_management) agents to collect the generated events, and then analyzing these events, you can identify malicious or anomalous activity and understand how intruders and malware operate on your network. The service runs as a [protected process](https://learn.microsoft.com/windows/win32/services/protecting-anti-malware-services-#system-protected-process), preventing broad user-mode interaction.

### Sysmon Features Overview

*Sysmon* includes the following features:

- Logs processes created with their full command line, along with their parent process.
- Logs hashes of process image files using SHA1 (default), MD5, SHA256, or IMPHASH.
- Supports multiple hashes used simultaneously.
- Includes a process GUID in process creation events to allow correlation of events even when Windows reuses process IDs.
- Includes a session GUID in each event to allow correlation of events on the same logon session.
- Logs the loading of drivers or DLLs with their signatures and hashes.
- Logs raw read access attempts to disks and volumes.
- (Optional) Logs network connections, including source process, IP addresses, port numbers, hostnames, and port names for each connection.
- Detects file creation time changes to determine the true creation time of a file. Modifying file creation timestamps is a common malware tactic to hide its tracks.
- Automatically reloads configuration if changes are made to the registry.
- Performs rule filtering to dynamically include or exclude certain events.
- Generates events at the start of the process to capture activities performed by sophisticated kernel-mode malware. 

Sysmon can be a valuable tool for monitoring system activities and diagnosing issues that arise during system updates or when troubleshooting security incidents.



#### （1）Windows

address： https://download.sysinternals.com/files/Sysmon.zip



#### （2）Linux

address：https://github.com/Sysinternals/SysmonForLinux

doc：https://learn.microsoft.com/zh-cn/sysinternals/downloads/sysmon
