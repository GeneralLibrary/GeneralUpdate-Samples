---
sidebar_position: 2
---

### File Occupancy

#### (1) Windows Platform

Even though applications are closed during automatic upgrades, file occupancy can occur if processes are still running due to special circumstances. In such cases, you can use Microsoft's handle.exe tool to check if there are any processes running in a specified directory. "handle.exe" is a command-line tool provided by Microsoft that displays which processes have opened specific files. In C#, you can invoke handle.exe using the `System.Diagnostics.Process` class. If a process is detected, it will return a list of processes running in that directory.

```c#
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Process process = new Process();
        process.StartInfo.FileName = "handle.exe";
        process.StartInfo.Arguments = "filename"; // Replace 'filename' with the actual file or directory
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        Console.WriteLine(output);

        process.WaitForExit();
    }
}
```

References:
- https://learn.microsoft.com/zh-cn/sysinternals/downloads/handle
