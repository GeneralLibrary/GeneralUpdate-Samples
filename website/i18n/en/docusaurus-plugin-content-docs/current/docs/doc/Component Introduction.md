---
sidebar_position: 2
---



![](imgs\GeneralUpdate_h2.png)

### 1. What is GeneralUpdate?

GeneralUpdate is an open-source, cross-platform automatic update component based on .NET Standard 2.0. A small star is your greatest support, motivating us to keep updating and maintaining. ENENENENENENEN

##### Project Open Source Repository Address

- https://github.com/GeneralLibrary/GeneralUpdate
- https://gitee.com/GeneralLibrary/GeneralUpdate

##### Related Open Source Project Repository Addresses

Current Project GeneralUpdate

- https://github.com/GeneralLibrary/GeneralUpdate
- https://gitee.com/GeneralLibrary/GeneralUpdate

MAUI GeneralUpdate.Maui

- https://github.com/GeneralLibrary/GeneralUpdate.Maui

Packaging Tool Project Address GeneralUpdate.Tools

- https://github.com/GeneralLibrary/GeneralUpdate.Tools
- https://gitee.com/GeneralLibrary/GeneralUpdate.Tools

Sample Project Address GeneralUpdate-Samples

- https://github.com/GeneralLibrary/GeneralUpdate-Samples
- https://gitee.com/GeneralLibrary/GeneralUpdate-Samples

##### Help Documentation #####

- Video Tutorial: https://www.bilibili.com/video/BV1aX4y137dd
- Official Website: https://www.justerzhu.cn/
- Quick Start: https://mp.weixin.qq.com/s/pRKPFe3eC0NSqv9ixXEiTg
- Usage Tutorial Video: https://www.bilibili.com/video/BV1FT4y1Y7hV
- Help Documentation & Official Website Source Repository: https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/website/doc

##### Communication and Exchange

This open-source project currently follows the MIT open-source license and can be used for commercial purposes for free. Free communication channels will periodically address developers' questions. All community donations will be used for the development and construction of open-source projects. Code contributors will be rewarded with donation funds.

- Free Q&A: https://github.com/GeneralLibrary/GeneralUpdate/issues
- Free Technical Exchange: .NET Technology Exchange Group 341349660
- Open Source Project Usage Discussion: GeneralUpdate Exchange Group 748744489
- One-on-one Q&A for open-source projects requires paid consultation.



### Business Cooperation

Customized secondary development, technical consultation, open-source project rewards. Below are the contact details for enterprise inquiries, quotations, and business cooperation (please indicate your purpose when adding as a friend, casual chat is declined, adding on WeChat is recommended).

![](imgs\contact.png)





### 2. Unifying Language

Before using GeneralUpdate, we need to understand some basic concepts in the system.

 ![](imgs/client_server_upgrade.jpg)

- Client: Refers to your main application, which is the client to be updated. It can also be understood as the company's product.
- Upgrade: Refers to the upgrade program, which will be an independent process. It needs to be placed in the same directory as the Client and should not be associated with any business or design during use (or coding), maintaining independent reference.
- Server: Refers to the server application (ASP.NET) that will provide version update information interaction and version verification functions.
- GeneralUpdate.Tool: Is the packaging tool provided by this open-source project, used to generate update packages (7z and zip).
- UpdatePacket: As the name suggests, it is an update package. The contents of the update package are patch files (.patch) or new files with content differences between the new and old versions.
- SQL: Currently, SQL scripts are generated based on the MySQL database.



#### Version Number Execution Standards

- Nuget Version Management Reference Standard: https://docs.microsoft.com/zh-cn/nuget/concepts/package-versioning
- Application Assembly Version Management Reference Standard: https://docs.microsoft.com/zh-cn/dotnet/standard/assembly/versioning (The client program updated by the component, in simple terms, is your company's product; the component's operation will be executed according to this standard.)
- In each iteration, besides updating the version number in AssemblyInfo, the version number of the DLL file also needs to be modified.



### 3. What does GeneralUpdate provide?

##### Component Functions

| Function                   | Supported      | Remarks                                                      |
| -------------------------- | -------------- | ------------------------------------------------------------ |
| Resume Download            | Yes            | When a single update fails, continue with the previous update download content on the next startup. (Enabled by default when referencing the component) |
| Incremental Update         | Yes            | Update only the currently modified files compared to the previous version, and delete the files that do not exist in the current version. (Enabled by default when referencing the component) |
| Binary Differential Update | Yes            | Generate patch files by comparing new and old versions using differential algorithms. (Enabled by default when referencing the component) |
| Incremental Update         | Yes            | Update only the currently modified files compared to the previous version, and delete the files that do not exist in the current version. (Enabled by default when referencing the component) |
| Forced Update              | Yes            | Directly force an update after opening the client.           |
| Multi-branch Update        | Yes            | When a product has multiple branches, update the corresponding content based on different branches. |
| Latest Version Push        | Yes            | Implemented based on Signal R, push the current latest version. |
| Multi-language             | To be verified | The component can also be written as a console program, serving as an update "script" for applications in other languages. |
| Skip Update                | Yes            | Support injecting pop-ups to let users decide whether to update this release, with the server deciding that forced updates are not effective. |
| Mutual Upgrade             | Yes            | The main program can update the upgrade program, and the upgrade program can update the main program. |
| Blacklist                  | Yes            | During updates, files and file extensions in the blacklist will be skipped. |
| OSS                        | Yes            | Simplified update, an independent update mechanism. Just place the version configuration file version.json on the file server. The component will update and download based on the version information in the configuration file. |
| Rollback, Backup           | Yes            | Before updating, local files on the client will be backed up. If the client fails to start or crashes, it will roll back and overwrite. |
| Driver Update              | To be verified | Before updating, drivers will be backed up locally. If the client fails to start or crashes, it will roll back and overwrite. |
| Custom Method List         | Yes            | Inject a custom method collection that will be executed before the update starts. If any exceptions occur during the execution of the custom method list, they will be notified through exception subscription. (Recommended to check the current software environment before updating) |
| AOT                        | Yes            | Support AOT compilation and release.                         |



##### Packaging Tool

GeneralUpdate.PacketTool is a desktop packaging program written in .NET 8. https://github.com/GeneralLibrary/GeneralUpdate.Tools

| Function                                 | Supported      | Remarks                                                      |
| ---------------------------------------- | -------------- | ------------------------------------------------------------ |
| Differential Update Package Generation   | Yes            | Compare the previous version and the current version to find the files that need to be updated or newly added files. |
| Automatic Upload of Update Packages      | In development | Automatically upload the generated differential update packages to the server. |
| Editable Selection of Differential Files | In development | Visualize the packaging content of the differential package and select packaging files. |



### 5. What does GeneralUpdate support?

##### .NET Framework

| Framework Name                   | Supported |
| -------------------------------- | --------- |
| .NET Core 2.0                    | Yes       |
| .NET 5 ... to the latest version | Yes       |
| .NET Framework 4.6.1             | Yes       |

##### UI Framework

| UI Framework Name | Supported                                |
| ----------------- | ---------------------------------------- |
| WPF               | Yes                                      |
| UWP               | Cannot update in store mode              |
| MAUI              | Currently only supports Android platform |
| Avalonia          | Yes                                      |
| WinUI             | Yes                                      |
| Console           | Yes                                      |
| WinForms          | Yes                                      |

##### Operating System

| Operating System Name     | Supported      |
| ------------------------- | -------------- |
| Windows                   | Yes            |
| Linux                     | Yes            |
| Android                   | Yes            |
| Kirin V10 (Phytium S2500) | Yes            |
| Kirin V10 (x64)           | Yes            |
| Ubuntu                    | Yes            |
| Loongson (Loongnix)       | To be verified |



### Support Us

Dear Sponsors,

This project is an automatic update component designed and maintained primarily by the author. The goal of this project is to provide a platform for sharing, learning, and innovation for developers and technology enthusiasts worldwide. We are committed to promoting technological development and the power of shared knowledge.

However, we need your support. Your sponsorship will enable us to further improve the quality of the project, add more features, and provide a better user experience. Additionally, as a token of appreciation, we will display your company logo on the project website.

This is a great opportunity to not only drive technological advancement but also to enhance your brand's visibility and community influence. We believe your sponsorship will have a profound impact on this open-source project. Furthermore, all sponsorships will be used to purchase GeneralUpdate merchandise to reward community partners who contribute code to the project. If you have made an actual contribution, please contact me via the email below.

The size of the project has exceeded the scope of individual maintenance, and designing and maintaining it is not easy. We look forward to your active participation in creating a better future together!

Thank you!

​                                                                                                                                       Author: Juster     