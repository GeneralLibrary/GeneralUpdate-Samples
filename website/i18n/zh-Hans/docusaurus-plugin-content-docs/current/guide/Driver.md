---
sidebar_position: 3
---

### Driver 驱动

#### （1）Windows平台

##### 驱动安装

本组件内置使用的驱动安装工具为微软自带的工具PnPutil.exe或使用setupapi.dll来实现。

驱动安装时需要注意的问题有两点：

| 名称 | 说明                               |
| ---- | ---------------------------------- |
| 安装 | 驱动证书安装，需要在驱动之前安装。 |
| 版本 | 区分x86 , x64版本。                |

**PnPUtil实现：**

PnPUtil是一个命令行实用程序，它可以用来管理Windows的驱动程序商店。你可以使用它来添加、删除和列出驱动程序。

以下是如何使用PnPUtil来安装驱动程序的步骤：

1. 打开命令提示符（以管理员身份）。

2. 导航到包含驱动程序的INF文件的目录。

3. 运行以下命令：

   `pnputil /add-driver <INF文件名>`

   例如，如果你的INF文件名为`mydriver.inf`，那么你应该运行`pnputil /add-driver mydriver.inf`。

4. PnPUtil将会添加驱动程序到驱动程序商店，并尝试为任何匹配的设备安装驱动程序。

注意，PnPUtil需要管理员权限才能运行。

在C#中，你可以使用System.Diagnostics.Process类来运行PnPUtil。以下是一个例子：

```c#
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        string infPath = "Path to your INF file";

        Process process = new Process();
        process.StartInfo.FileName = "pnputil.exe";
        process.StartInfo.Arguments = "/add-driver " + infPath;
        process.StartInfo.Verb = "runas";  // 运行为管理员
        process.Start();

        process.WaitForExit();
    }
}
```



**setupapi.dll实现：**

```c#
using System;
using System.Runtime.InteropServices;

public class Program
{
    // 定义 SetupCopyOEMInf 函数的 P/Invoke 签名
    [DllImport("setupapi.dll", EntryPoint = "SetupCopyOEMInf", SetLastError = true)]
    public static extern bool SetupCopyOEMInf(
        string SourceInfFileName,
        string OEMSourceMediaLocation,
        int OEMSourceMediaType,
        int CopyStyle,
        string DestinationInfFileName,
        int DestinationInfFileNameSize,
        ref int RequiredSize,
        string DestinationInfFileNameComponent
    );

    public static void Main()
    {
        string infPath = "Path to your INF file";
        bool result = SetupCopyOEMInf(infPath, null, 0, 0, null, 0, ref int size, null);

        if (!result)
        {
            Console.WriteLine("Failed to install driver. Error code: " + Marshal.GetLastWin32Error());
        }
    }
}
```



##### 驱动证书

本组件内置使用Windows的证书管理工具（CertMgr.exe）或者使用.NET框架中的X509Store类来实现。

**CertMgr.exe实现：**

`CertMgr.exe` 是一个命令行工具，它是微软的.NET Framework的一部分。你可以在.NET Framework的安装目录中找到它。

对于大多数系统，它的位置通常是在以下目录之一：

- `C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin`
- `C:\Program Files\Microsoft SDKs\Windows\v7.0A\Bin`

如果你找不到它，你可以使用Windows的搜索功能来搜索`CertMgr.exe`。

注意，`CertMgr.exe`是一个命令行工具，你需要在命令提示符或PowerShell中运行它。你也可以在你的C#代码中使用`System.Diagnostics.Process.Start()`方法来调用它。



**X509Store实现：**

```c#
using System;
using System.Security.Cryptography.X509Certificates;

public class Example
{
    public static void Main()
    {
        string CertificatePath = "Path to your certificate file";

        // 创建一个新的X509证书实例
        X509Certificate2 certificate = new X509Certificate2(CertificatePath);

        // 打开当前用户的个人证书存储区
        X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        // 将新证书添加到存储区
        store.Open(OpenFlags.ReadWrite);
        store.Add(certificate);

        store.Close();
    }
}

```

