---
sidebar_position: 3
---

### Driver

#### (1) Windows Platform

##### Driver Installation

The tool used for driver installation in this component is the Microsoft built-in tool PnPutil.exe or setupapi.dll.

There are two key points to note when installing drivers:

| Name    | Description                                                  |
| ------- | ------------------------------------------------------------ |
| Install | Driver certificate installation should occur before the driver installation. |
| Version | Differentiate between x86 and x64 versions.                  |

**PnPUtil Implementation:**

PnPUtil is a command line utility that can be used to manage the Windows driver store. You can use it to add, delete, and list drivers.

Here are the steps to install drivers using PnPUtil:

1. Open Command Prompt as an administrator.

2. Navigate to the directory containing the driver's INF file.

3. Run the following command:

   `pnputil /add-driver <INF file name>`

   For example, if your INF file name is `mydriver.inf`, you should run `pnputil /add-driver mydriver.inf`.

4. PnPUtil will add the driver to the driver store and attempt to install the driver for any matching devices.

Note that PnPUtil requires administrator privileges to run.

In C#, you can use the System.Diagnostics.Process class to run PnPUtil. Here is an example:

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
        process.StartInfo.Verb = "runas";  // Run as administrator
        process.Start();

        process.WaitForExit();
    }
}
```

**setupapi.dll Implementation:**

```c#
using System;
using System.Runtime.InteropServices;

public class Program
{
    // Define the P/Invoke signature for the SetupCopyOEMInf function
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

##### Driver Certificate

This component uses Windows certificate management tools (CertMgr.exe) or the X509Store class in the .NET framework.

**CertMgr.exe Implementation:**

`CertMgr.exe` is a command line tool that is part of the Microsoft .NET Framework. You can find it in the .NET Framework installation directory.

For most systems, it is typically located in one of the following directories:

- `C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin`
- `C:\Program Files\Microsoft SDKs\Windows\v7.0A\Bin`

If you cannot find it, you can use Windows search to locate `CertMgr.exe`.

Note that `CertMgr.exe` is a command line tool and must be run in Command Prompt or PowerShell. You can also call it in your C# code using the `System.Diagnostics.Process.Start()` method.

**X509Store Implementation:**

```c#
using System;
using System.Security.Cryptography.X509Certificates;

public class Example
{
    public static void Main()
    {
        string CertificatePath = "Path to your certificate file";

        // Create a new X509 certificate instance
        X509Certificate2 certificate = new X509Certificate2(CertificatePath);

        // Open the current user's personal certificate store
        X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        // Add the new certificate to the store
        store.Open(OpenFlags.ReadWrite);
        store.Add(certificate);

        store.Close();
    }
}
```

