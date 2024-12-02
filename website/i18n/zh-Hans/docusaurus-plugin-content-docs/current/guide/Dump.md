---
sidebar_position: 4
---

### Dump 转储文件

在自动升级的过程中如果更新失败，或程序更新完成之后运行崩溃都可以使用ProcDump工具辅助导出dump文件。ProcDump 是一个命令行实用工具，其主要用途是监视应用程序的 CPU 峰值，并在出现峰值期间生成故障转储，管理员或开发人员可以使用这些转储来确定出现峰值的原因。 ProcDump 还支持挂起窗口监视（使用与 Windows 和任务管理器使用的窗口挂起相同的定义）、未处理的异常监视，并且可以根据系统性能计数器的值生成转储。 它还可用作可嵌入到其他脚本中的常规进程转储实用工具。

##### （1）Windows平台

C#实现调用：

```c#
using System;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        var procDumpPath = @"C:\Path\To\procdump.exe";
        var processId = 1234; // 您要转储的进程的ID
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



参考资料：

- https://learn.microsoft.com/zh-cn/sysinternals/downloads/procdump