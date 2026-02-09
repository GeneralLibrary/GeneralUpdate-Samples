---
sidebar_position: 9
---

### Definition

Namespace: GeneralUpdate.ClientCore

Assembly: GeneralUpdate.ClientCore.dll



**OSS**

OSS stands for Object Storage Service. For those developing server-side technology stacks, this term is likely familiar, as major cloud service providers offer similar services. Simply put, it's a file server. Examples include Alibaba Cloud OSS, Tencent Cloud COS, and Huawei Cloud OBS. While they have different names, their functionalities are quite similar. The new feature of this update component is implemented in a very similar manner, so we chose OSS to name this feature, making it easy for developers to quickly understand its purpose (requires the use of components GeneralUpdate.ClientCore, GeneralUpdate.Core).



**GeneralClientOSS**

The GeneralClientOSS class is another update mechanism. Unlike GeneralClientBootstrap, GeneralClientOSS does not require writing server-side code and directly updates based on the version information from the version.json file on the OSS file server, simplifying the threshold for beginners using this component.

```c#
public sealed class GeneralClientOSS
```



### Example

| Description                 | Address                                                      |
| --------------------------- | ------------------------------------------------------------ |
| Address of code example     | [Enter](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src/OSS) |
| Hashing algorithm (sha256)  | [Enter](https://github.com/GeneralLibrary/GeneralUpdate/blob/master/src/c%23/GeneralUpdate.Core/Pipeline/HashMiddleware.cs) |
| Test script (oss_start.cmd) | [Enter](https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/src) |
| versions.json Making tool   | [Enter](https://github.com/GeneralLibrary/GeneralUpdate.Tools) |

#### OSS Process

![](imgs/oss_flow.png)

1. Prepare the version.json configuration file and update files (update.zip) just like in previous packaging methods.

```json
[
  {
    "PacketName": "packet_20250102230201638_1.0.0.1",
    "Hash": "ad1a85a9169ca0083ab54ba390e085c56b9059efc3ca8aa1ec9ed857683cc4b1",
    "Version": "1.0.0.1",
    "Url": "http://localhost:5000/packages/packet_20250102230201638_1.0.0.1.zip",
    "PubTime": "2025-01-02T23:48:21"
  }
]
```



2. When the Client starts, it directly requests the OSS server or file server to download the version.json file.

![](imgs/oss_util.png)

3. After downloading to the local system, it parses the version information to determine if an update is needed. If so, it passes the information to Upgrade through a process launch (Client self-closes).

4. After Upgrade starts, it directly downloads update.zip and extracts it locally to overwrite the local files.

5. After the Upgrade completes, it starts the Client and self-closes. The update ends. Compared to the GeneralUpdateBootstrap function, the GeneralUpdateOSS function has a very low usage threshold. If the company does not have high requirements for automatic updates, this feature can be used. In summary, this function downloads the version.json and updates based on the content in the file, downloading each version's update package and extracting it to complete the update.

6. Since the OSS functionality does not include a blacklist mechanism, when creating the update.zip, please ensure that the files used internally by the following components are not added to the package, or directly release them as a single file to avoid occupying space and reducing update complexity.

```json
"Microsoft.Bcl.AsyncInterfaces.dll",
"System.Collections.Immutable.dll", 
"System.IO.Pipelines.dll", 
"System.Text.Encodings.Web.dll",
"System.Text.Json.dll"
```



The following example defines methods, including the usage of GeneralClientOSS , GeneralUpdateOSS.

GeneralClientOSS :

```c#
using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Shared.Object;

var paramsOSS = new GlobalConfigInfoOSS
{
    Url = "http://localhost:5000/packages/versions.json",
    CurrentVersion = "1.0.0.0",
    VersionFileName = "versions.json",
    AppName = "OSSClientSample.exe",
    Encoding = Encoding.UTF8.WebName
};

await GeneralClientOSS.Start(paramsOSS, "OSSUpgradeSample.exe");
```

GeneralUpdateOSS :

```
using GeneralUpdate.Core;

/*
 * GeneralUpdateOSS will by default read the JSON content of GlobalConfigInfoOSS stored in the system environment variables by GeneralClientOSS
 * , and developers do not need to be concerned with the entire process.
 * 
 * Environment.GetEnvironmentVariable("GlobalConfigInfoOSS", EnvironmentVariableTarget.User);
 * 
 * Typically, GeneralClientOSS and GeneralUpdateOSS appear as a pair.
 */
try
{
    await GeneralUpdateOSS.Start();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```



### Applicable to

| Product        | Version       |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |