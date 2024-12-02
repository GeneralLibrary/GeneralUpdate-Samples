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

NuGet Installation

```shell
NuGet\Install-Package GeneralUpdate.ClientCore -Version 3.0.0
```



### Example

#### OSS Process

![](imgs/oss_flow.png)

1. Prepare the version.json configuration file and update files (update.zip) just like in previous packaging methods.

![](imgs/oss_version_json.png)

2. When the Client starts, it directly requests the OSS server or file server to download the version.json file.

![](imgs/oss_util.png)

3. After downloading to the local system, it parses the version information to determine if an update is needed. If so, it passes the information to Upgrade through a process launch (Client self-closes).

4. After Upgrade starts, it directly downloads update.zip and extracts it locally to overwrite the local files.

5. After the Upgrade completes, it starts the Client and self-closes. The update ends. Compared to the GeneralUpdateBootstrap function, the GeneralUpdateOSS function has a very low usage threshold. If the company does not have high requirements for automatic updates, this feature can be used. In summary, this function downloads the version.json and updates based on the content in the file, downloading each version's update package and extracting it to complete the update.



The following example defines methods, including the usage of GeneralClientOSS.

```c#
Task.Run(async () =>
{
    var url = "http://192.168.xxx.xxx";
    var apk = "com.companyname.generalupdate.ossclient.apk";
    var authority = "com.generalupdate.oss.fileprovider";
    var currentVersion = "1.0.0.0";
    var versionFileName = "version.json";
    GeneralClientOSS.AddListenerDownloadProcess(OnOSSDownload);
    GeneralClientOSS.AddListenerException(OnException);
    await GeneralClientOSS.Start(new ParamsOSS(url, apk, authority, currentVersion, versionFileName));
});
```



### Annotation

GeneralUpdateOSS provides the following capabilities.

#### Properties

| Properties | Description                          |
| ---------- | ------------------------------------ |
| ParamsOSS  | OSS update configuration parameters. |



#### Methods

| Method                 | Description                                                 |
| ---------------------- | ----------------------------------------------------------- |
| Start()                | Start the OSS update.                                       |
| AddListenerException() | Listen for internal exception information of the component. |



### 🌴ParamsOSS

**Properties**

**Url** string: HTTP API server request address.

**AppName** string: Application name.

**CurrentVersion** string: Current version.

**VersionFileName** string: Version file name.



### 🌼Start()

**Method**

Start the OSS update.

```c#
public static async Task Start(ParamsOSS configParams, string upgradeAppName = "GeneralUpdate.Upgrade");
```



**Parameter Types**

ParamsOSS: Update configuration parameters, detailed content can be found in this document under 🌴ParamsOSS.



**Parameters**

**parameter** ParamsOSS: Update configuration parameters for the Android platform.

**upgradeAppName**: Name of the application to be upgraded.



### Applicable to

| Product        | Version       |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |