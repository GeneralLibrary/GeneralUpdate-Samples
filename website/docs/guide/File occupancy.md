---
sidebar_position: 2
---

### File occupancy 文件占用

#### （1）Windows平台

虽然在自动升级时会关闭应用程序，如果出现特殊情况出现文件占用通常是进程还在运行导致的。这时候可以使用微软官方提供的handle.exe检测工具来查看指定目录下是否有进程在运行"handle.exe"是一款由微软提供的命令行工具，可以用来显示哪些进程打开了哪些文件。在C#中调用handle.exe，我们可以使用`System.Diagnostics.Process`类，如果检测到则会返回该目录下正在运行的进程列表。

```c#
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Process process = new Process();
        process.StartInfo.FileName = "handle.exe";
        process.StartInfo.Arguments = "filename";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        Console.WriteLine(output);

        process.WaitForExit();
    }
}
```



参考资1料：

- https://learn.microsoft.com/zh-cn/sysinternals/downloads/handle
