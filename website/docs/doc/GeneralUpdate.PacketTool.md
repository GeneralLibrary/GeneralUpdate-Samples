---
sidebar_position: 11
---

### Introduction

![](imgs\tool.png)

The tool is developed using Avalonia and supports Windows / Linux / Mac desktop use only.

| Repository Address                                    |
| ----------------------------------------------------- |
| https://github.com/GeneralLibrary/GeneralUpdate.Tools |

| Feature             | Supported | Remarks                                                      |
| ------------------- | --------- | ------------------------------------------------------------ |
| Differential Update | Yes       | Compares the previous version with the current version to find files that need updating or files that are newly added or deleted. |

#### Description

| Name        | Remarks                                                      |
| ----------- | ------------------------------------------------------------ |
| Source path | Indicates the folder path of the previous version.           |
| Target path | Indicates the folder path of the current version.            |
| Patch path  | Indicates the path where the final update patch package will be generated. |
| Build       | Recursively compares all project files (DLL, exe, etc.) under the Source path and Target path folders, and packages the update package based on a binary differential check and incremental check analysis of the file list that needs updating according to the folder structure. |
| Clear       | Clears the current input content.                            |