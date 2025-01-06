---
sidebar_position: 2
---

![](imgs\GeneralUpdate_h2.png)

## 1. What is GeneralUpdate?

**Updates limitless, upgrades boundless.**

GeneralUpdate is an open-source cross-platform application automatic update component based on .NET Standard 2.0 and licensed under the MIT license.

## 2. What does GeneralUpdate offer?

##### Component Features

| Feature                    | Supported      | Notes                                                        |
| -------------------------- | -------------- | ------------------------------------------------------------ |
| Resume Download            | Yes            | If an update fails, it will resume downloading the update package the next time it starts. (Effective by default when the component is referenced) |
| Version-by-Version Update  | Yes            | If the current client version is multiple versions behind the server, it will update one by one based on the release dates of the versions. (Effective by default when the component is referenced) |
| Binary Differential Update | Yes            | Generates patch files by comparing new and old versions using differential algorithms. (Effective by default when the component is referenced) |
| Incremental Update         | Yes            | Updates only the files that have changed compared to the previous version and deletes files that do not exist in the current version. (Effective by default when the component is referenced) |
| Forced Update              | Yes            | Forces an update directly after opening the client.          |
| Multi-Branch Update        | Yes            | Supports updating different content based on different branches of a product. |
| Latest Version Push        | Yes            | Implemented based on Signal R to push the latest version.    |
| Multi-Language             | To be verified | Can also be written as a console program to act as an update "script" for applications in other languages. |
| Skip Update                | Yes            | Supports injecting a pop-up to let users decide whether to update this release; forced updates from the server side will override this. |
| Mutual Upgrade             | Yes            | The main program can update the upgrade program, and vice versa. |
| Blacklist                  | Yes            | Skips files and file extensions listed in the blacklist during the update process. |
| OSS                        | Yes            | Simplifies updates with an independent update mechanism. Only requires placing a `version.json` configuration file on the file server. The component will update and download based on the version information in the configuration file. |
| Rollback and Backup        | Yes            | Backs up local client files before updating, and rolls back if the client fails to start or crashes. |
| Driver Update              | To be verified | Backs up drivers locally before updating, and rolls back if the client fails to start or crashes. |
| Custom Method List         | Yes            | Injects a collection of custom methods that execute before the update starts. If any exception occurs while executing the custom method list, it will be notified through exception subscription. (Recommended to check the current software environment before updating) |
| AOT                        | Yes            | Supports AOT compilation and publishing.                     |

## 3. What does GeneralUpdate support?

##### .NET Framework

| Framework Name                   | Supported |
| -------------------------------- | --------- |
| .NET Core 2.0                    | Yes       |
| .NET 5 ... to the latest version | Yes       |
| .NET Framework 4.6.1             | Yes       |

##### UI Framework

| UI Framework Name | Supported                                | Contributor |
| ----------------- | ---------------------------------------- | ----------- |
| WPF               | Yes                                      | JusterZhu   |
| UWP               | Not updatable in store mode              | lindexi     |
| MAUI              | Currently supports only Android platform | JusterZhu   |
| Avalonia          | Yes                                      | JusterZhu   |
| WinUI             | Yes                                      | JusterZhu   |
| Console           | Yes                                      | JusterZhu   |
| WinForms          | Yes                                      | JusterZhu   |

##### Operating Systems

| Operating System Name         | Supported | Contributor                                     |
| ----------------------------- | --------- | ----------------------------------------------- |
| Windows                       | Yes       | JusterZhu                                       |
| Linux                         | Yes       | JusterZhu                                       |
| Android                       | Yes       | JusterZhu                                       |
| Kylin V10 (Feiteng S2500)     | Yes       | Wei                                             |
| Kylin V10 (Feiteng FT-2000)   | Yes       | Yao Shengwei                                    |
| Kylin V10 (x64)               | Yes       | Wei                                             |
| Ubuntu                        | Yes       | JusterZhu                                       |
| Huawei EulerOS (Kunpeng)      | Yes       | Yao Shengwei                                    |
| Loongson (Loongnix LoongArch) | Yes       | Avalonia Chinese Community (Dong Bin Rabbitism) |
| Apple Mac (M1)                | Yes       | JusterZhu                                       |

## 4. Repositories

| Name                  | Description                      | Repository                                                   |
| --------------------- | -------------------------------- | ------------------------------------------------------------ |
| GeneralUpdate         | Automatic Updates                | https://github.com/GeneralLibrary/GeneralUpdate<br />https://gitee.com/GeneralLibrary/GeneralUpdate |
| GeneralUpdate.Maui    | Maui Automatic Updates (Android) | https://github.com/GeneralLibrary/GeneralUpdate.Maui         |
| GeneralUpdate.Tools   | Update Patch Creation Tool       | https://github.com/GeneralLibrary/GeneralUpdate.Tools<br />https://gitee.com/GeneralLibrary/GeneralUpdate.Tools |
| GeneralUpdate-Samples | Usage Examples                   | https://github.com/GeneralLibrary/GeneralUpdate-Samples<br />https://gitee.com/GeneralLibrary/GeneralUpdate-Samples |

## 5. Unified Language

![](D:\github_project\GeneralUpdate-Samples\website\i18n\en\docusaurus-plugin-content-docs\current\docs\doc\imgs\client_server_upgrade.jpg)

Before starting to use GeneralUpdate, we need to understand some basic concepts in the system. Please download the GeneralUpdate-Samples repository and navigate to the `..\GeneralUpdate-Samples\src` directory for better understanding.

| Name                | Description                                                  | Explanation                                                  |
| ------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| Client              | The client being updated.                                    | If you want to update QQ, then QQ is the Client.             |
| Upgrade             | The upgrade program is an independent process. It should be placed in the same directory as the Client and should not be associated with any business or design during use (or coding); it must remain independently referenced. | QQ cannot update its own files during runtime, so the Upgrade handles this task. |
| Packet              | Update Patch Package                                         | Gamers often hear the concept of "patches," which are used to update vulnerabilities or content in a game. Similarly, here the update patch package contains files with content differences or new files from the old and new versions, and files that need to be deleted. |
| Server              | Server Application                                           | Manages version update information, patch packages, and version verification. (In the Samples, only a simple example is provided and does not fulfill these functions; developers need to implement this themselves or purchase the GeneralSpacestation service). |
| GeneralUpdate.Tools | Update Patch Creation Tool                                   | A packaging tool provided by this open-source project for generating update patch packages (.zip file format). |

##### Quick Start

- Quick Start: https://www.justerzhu.cn/docs/quickstart/quikstart
- Tutorial Video: https://www.bilibili.com/video/BV1c8iyYZE7P
- Official Website: https://www.justerzhu.cn/
- Help Documentation & Official Website Source Repository: https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/website/doc

##### Version Number Standards

- NuGet Versioning Guidelines: https://docs.microsoft.com/zh-cn/nuget/concepts/package-versioning 
- Assembly Versioning Guidelines: https://docs.microsoft.com/zh-cn/dotnet/standard/assembly/versioning



## 6. Communication and Collaboration

In the free communication channels, questions from developers are answered periodically. All community donations will be used for the development and construction of open-source projects. Code contributors will be rewarded with donation funds.

- Free Q&A: https://github.com/GeneralLibrary/GeneralUpdate/issues
- Free Technical Exchange: .NET Technical Exchange Group 341349660
- Free Discussion on Open Source Project Usage: GeneralUpdate Discussion Group 748744489
- E-Mail: zhuzhen723723@outlook.com
- Due to the large number of individual communications, the author's time and energy are limited; one-on-one answers require paid consultation. Questions in discussion groups or issues are visible to everyone to avoid repetitive answers.
- The open-source project needs to establish a corporate wall on the official website. If any companies are using this project in their projects and wish to be featured for free promotion, please contact the author.

##### Business Cooperation

For customized secondary development, technical consultation, and open-source project sponsorship, see the contact information for corporate inquiries, quotations, business cooperation, and corporate wall (please specify your purpose when adding, idle chats are refused, WeChat is recommended).

![](D:\github_project\GeneralUpdate-Samples\website\i18n\en\docusaurus-plugin-content-docs\current\docs\doc\imgs\contact.png)

##### Sponsorship

Dear Sponsors,

This project is an automatic update component primarily designed and maintained by the author. The goal of this project is to provide a platform for developers and tech enthusiasts worldwide to share, learn, and innovate. We are committed to advancing technology and the power of shared knowledge.

However, we need your support. Your sponsorship will enable us to further improve the quality of the project, add more features, and provide a better user experience. In return, we will display your company's logo on the project's website to express our gratitude.

This is a great opportunity not only to promote technological progress but also to enhance your brand awareness and community influence. We believe your sponsorship will have a profound impact on this open-source project. Additionally, all sponsorship funds will be used to purchase GeneralUpdate merchandise to reward community partners who contribute code to the project. If you have made significant contributions, please contact me via the email below to get in touch.

The scale of the project has exceeded the capacity of individual maintenance, and design and maintenance are not easy. We look forward to your active participation, so let's join hands to create the future!

Thank you!

Author: Juster Zhu

![Sponsorship](imgs/payment.jpg)