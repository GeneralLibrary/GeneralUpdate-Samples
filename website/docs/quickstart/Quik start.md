---
slug: quikstart
title: GeneralUpdate
authors: juster
tags: [quikstart]
---



## Sample UI

Address：

- https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/ClientSample.sln
- https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/UpgradeSample.sln

![](imgs\sampleclient.png)

![](imgs\sampleupgrade.png)



## Step1

Download the Sample repository from GitHub. Before using the sample, make sure you have .NET 8 runtime environment installed locally.

- https://github.com/GeneralLibrary/GeneralUpdate-Samples

The repository directory contents are as follows:

![](imgs\content.png)

| Name          | Description                           |
| ------------- | ------------------------------------- |
| Client        | Main client sample program            |
| Server        | Server sample program                 |
| StartManager  | Update process console                |
| Upgrade       | Upgrade client sample program         |
| process.bat   | Not required for attention            |
| resource.bat  | Not required for attention            |
| start.cmd     | Script to start the update sample     |
| oss_start.cmd | Script to start the update OSS sample |

## Step2

Locate the file directory and double-click (the start.cmd script resets the local directory each time it is launched, so manual directory management is unnecessary):

```shell
...\GeneralUpdate-Samples\src\start.cmd
```

![](imgs\build.png)



The automatic process will begin compiling and copying all related project bin directories to the app directory:

```
...\GeneralUpdate-Samples\src\run\app
```

![](imgs\build.png)



Upon entering the app directory, you will see the setup prior to the upgrade.

![](imgs\rundir.png)



## Step3

After checking the app directory:

- Enter option 1 and press Enter
- The server sample program will start
- The main client sample program will start, initiating the update request (once the main client program update is complete, it will close automatically).

![](imgs\upgrade.png)



Once the main client program closes automatically, check the run\app directory again. You will notice a new backup directory named "app-1.0.0.0" and a file named "Congratulations on the update.txt".

![](imgs\rundir2.png)

Seeing this indicates that you have successfully completed an upgrade. Congratulations!

![](imgs\result.png)
