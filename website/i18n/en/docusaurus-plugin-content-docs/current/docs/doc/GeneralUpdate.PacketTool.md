---
sidebar_position: 11
---

### Introduction

The tool is developed using .NET WPF (.NET 8) and currently supports desktop usage on Windows and Linux (Ubuntu) platforms.

- Repository Links:

  - https://gitee.com/GeneralLibrary/GeneralUpdate.Tools
  - https://github.com/GeneralLibrary/GeneralUpdate.Tools

![](imgs/tool.png)

#### Features Provided

| Feature                             | Supported | Remarks                                                      |
| ----------------------------------- | --------- | ------------------------------------------------------------ |
| Delta Update Package Generation     | Yes       | Compares the previous version with the current version to identify files that need updating or new files added. |
| Automatic Upload of Update Packages | Yes       | Automatically uploads the generated delta update package to the server. |
| Editable Selection of Delta Files   | Yes       | Allows visual inspection of the contents of the delta package and selection of files for packaging. |

#### Usage Example

Source path: Represents the folder path of the previous version.

Target path: Represents the folder path of the current version.

Patch path: Represents the path where the final delta package is generated.

Build: Recursively compares all project files (DLL, exe, etc.) in the Source path and Target path folders. It analyzes the list of files that need updating using binary delta checks and incremental checks, and then packages the update package according to the folder structure.