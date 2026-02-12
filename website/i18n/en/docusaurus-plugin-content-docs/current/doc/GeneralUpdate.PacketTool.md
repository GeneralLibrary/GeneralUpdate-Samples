---
sidebar_position: 11
---

### Introduction

This tool is developed using Avalonia and supports Windows / Linux / Mac desktop platforms.

| Repository URL                                        |
| ----------------------------------------------------- |
| https://github.com/GeneralLibrary/GeneralUpdate.Tools |

| Feature                         | Supported | Remarks                                                      |
| ------------------------------- | --------- | ------------------------------------------------------------ |
| Build patch package             | Yes       | Compares the previous version with the current version to identify updated, newly added, or deleted files. |
| Build OSS version configuration | Yes       | Easily generates OSS version configuration files.            |
| Build Extension package         | Yes       |                                                              |

![](imgs\tool.png)

![](imgs\tool2.png)

![](imgs\tool3.png)

#### Explanation

**Build Patch Package**

| Name        | Remarks                                                      |
| ----------- | ------------------------------------------------------------ |
| Source path | Path to the folder of the previous version.                  |
| Target path | Path to the folder of the current version.                   |
| Patch path  | Path where the final update patch package will be generated. |
| Build       | Recursively compares all project files (DLL, exe, etc.) in the Source path and Target path folders. Analyzes the list of files to be updated through binary difference check and incremental check, then packages the update files according to the folder structure. |
| Clear       | Clears the currently entered content.                        |

**Build OSS Version Configuration**

| Name              | Remarks                                                      |
| ----------------- | ------------------------------------------------------------ |
| Release date time | Release time of the update package.                          |
| PacketName        | Name of the update package.                                  |
| Hash              | Hash value of the update package.                            |
| Version           | Version number of the update package.                        |
| Download address  | Download URL of the update package.                          |
| Get hash          | Function to retrieve the hash value of the update package.   |
| Append            | Appends new update information to the existing version details. |
| Clear             | Clears all filled-in content.                                |
| Copy              | Copies the generated content to the clipboard.               |
| Build             | Generates the OSS version configuration file (.json) to the local disk. |