---
sidebar_position: 4
---

### Dump Files

During the process of automatic updates, if an update fails or if the program crashes after the update, you can use the ProcDump tool to help export dump files. ProcDump is a command-line utility primarily used to monitor an application's CPU spikes and generate crash dumps during these spikes. Administrators or developers can use these dumps to determine the cause of the spikes. ProcDump also supports hung window monitoring (using the same definition as Windows and Task Manager), unhandled exception monitoring, and can generate dumps based on system performance counter values. It can also be used as a general-purpose process dump utility that can be embedded into other scripts.

##### (1) Windows Platform

C# Implementation for Calling ProcDump:

```c#
using System;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        var procDumpPath = @"C:\Path\To\procdump.exe";
        var processId = 1234; // The ID of the process you want to dump
        var dumpFilePath = @"C:\Path\To\dumpfile.dmp";

        var startInfo = new ProcessStartInfo
        {
            FileName = procDumpPath,
            Arguments = $"-ma {processId} {dumpFilePath}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();
    }
}
```

References：

- https://learn.microsoft.com/zh-cn/sysinternals/downloads/procdump