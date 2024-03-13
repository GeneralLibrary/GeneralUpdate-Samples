### 1.GeneralUpdate是什么？

- GeneralUpdate是一款基于.NET Standard2.0开源多平台自动升级组件。
- 运行环境：.NET7、.NET MAUI、Visual studio 2022(Preview)

<img src="../imgs/zh/功能图.jpg" alt="功能图" style="zoom: 50%;" />

#####  本项目开源仓库地址

- https://github.com/JusterZhu/GeneralUpdate
- https://gitee.com/Juster-zhu/GeneralUpdate

##### 相关开源项目仓库地址

- GeneralUpdate使用示例： 

  https://gitee.com/GeneralLibrary/GeneralUpdate-Samples

  https://gitee.com/GeneralLibrary/GeneralUpdate-Samples

- 更新包打包工具： 

  https://gitee.com/GeneralLibrary/GeneralUpdate.Tools

  https://github.com/GeneralLibrary/GeneralUpdate.Tools

##### 帮助文档 #####

- 讲解视频： https://www.bilibili.com/video/BV1aX4y137dd
- 官方网站： http://justerzhu.cn/
- 快速启动： https://mp.weixin.qq.com/s/pRKPFe3eC0NSqv9ixXEiTg
- 使用教程视频：https://www.bilibili.com/video/BV1FT4y1Y7hV
- 帮助文档&官方网站源码仓库：https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/website/doc

##### 沟通交流

- 免费问题解答： https://github.com/JusterZhu/GeneralUpdate/issues
- 免费技术交流： .NET技术交流群 341349660
- 开源项目使用讨论：GeneralUpdate交流群 748744489



##### 商务合作

定制化二次开发，技术咨询，开源项目打赏。 

- 关于开源项目一对一问答需付费咨询。 

- 本开源项目目前遵循MIT开源协议开源，可免费用于商用。在免费沟通渠道会不定期回答各位开发者的 问题。所有的社区捐赠将用于开源项目的发展建设。代码贡献者将会用捐赠资金进行奖励。

-  邮箱：zhuzhen723723@outlook.com 

  一个小小star是您对我最大的支持，让我保持更新维护的动力。



### 2.统一语言（Unifying Language）

在开始使用GeneralUpdate之前我们需要先知道体系中的一些基础概念。

​                                                    <img src="../imgs/client_server_upgrade.jpg" alt="client_server_upgrade" style="zoom:50%;" />

- Client：是指你的主应用程序，是被更新的客户端。也可理解为是公司的产品。 
- Upgrade:是指升级程序，它将是一个独立的进程。需要和Client放在同一级目录下，在使用（或编码） 的过程中不可以和任何业务或设计关联、必须保持独立引用。 
- Server:是指服务端应用（ASP.NET）将提供版本更新信息交互、版本验证功能。 
- GeneralUpdate.Tool：是本开源项目提供的打包工具，用于生成更新包(7z和zip)。
- UpdatePacket：顾名思义就是更新包，更新包里的内容都是新旧版本中具有文件内容差异补丁 （.pacth）或新增文件。
-  SQL : 目前sql脚本是基于Mysql数据库生成



### 版本号执行标准

- Nuget版本管理参考标准：https://docs.microsoft.com/zh-cn/nuget/concepts/package-versioning 
- 应用程序集版本管理参考标准：https://docs.microsoft.com/zh-cn/dotnet/standard/assembly/versioning （被组件更新的客户端程序，说通俗点就是你公司的产品；组件的操作将按照这个标准执行。） 
- 每次迭代除了更新AssemblyInfo中的版本号，还需要修改DLL文件的版本号。



### 3.GeneralUpdate提供什么？

##### 组件功能

| 功能                           | 是否支持 | 备注                                                         |
| ------------------------------ | -------- | ------------------------------------------------------------ |
| 断点续传                       | 支持     | 单次更新失败时，下次一次启动时继续上一次更新下载更新包内容。（引用组件默认生效） |
| 逐版本更新                     | 支持     | 客户端当前版本如果与服务器相差多个版本，则根据多个版本的发布日期逐个更新。（引用组件默认生效） |
| 二进制差分更新                 | 支持     | 对比新老版本通过差分算法生成补丁文件。（引用组件默认生效）   |
| 增量更新功能                   | 支持     | 相比上一个版本只更新当前修改过的文件，并且删除当前版本不存在的文件。（引用组件默认生效） |
| 强制更新                       | 支持     | 打开客户端之后直接强制更新。                                 |
| 多分支更新                     | 支持     | 当一个产品有多个分支时，需要根据不同的分支更新对应的内容。   |
| 最新版本推送                   | 支持     | 基于Signal R实现，推送当前最新版本。                         |
| 客户端程序、服务端程序应用更新 | 支持     | C/S和B/S程序均可使用。                                       |
| 多平台、操作系统               | 部分支持 | Windows、MAUI Android平台                                    |
| 多语言                         | 待验证   | 也可将本组件编写为控制台程序，作为更新“脚本”。更新其他语言的应用程序。 |
| 跳过更新                       | 支持     | 支持注入弹窗让用户决定是否更新本次发布，服务端决定强制时更新不生效。 |
| 相互升级                       | 支持     | 主程序可更新升级程序，升级程序可更新主程序。                 |
| 黑名单                         | 支持     | 在更新过程中会跳过黑名单中的文件列表和文件扩展名列表。       |
| OSS                            | 支持     | 极简化更新，是一套独立的更新机制。只需要在文件服务器中放置version.json的版本配置文件。组件会根据配置文件中的版本信息进行更新下载。(支持Windows，MAUI Android) |
| 回滚                           | 待测试   | 逐版本更新时会备份每个版本，如果更新失败则逐版本回滚。       |
| 驱动更新                       | 待测试   | 逐版本更新时会备份每个版本的驱动文件（.inf），如果更新失败则逐版本回滚。 |
| 遗言                           | 待测试   | 开机时和升级时会检查升级是否成功，如果失败则根据遗言还原之前的备份。遗言是更新之前就已经自动创建在C:\generalupdate_willmessages目录下的will_message.json文件。will_message.json的内容是持久化回滚备份的文件目录相关信息。（需要部署GeneralUpdate.SystemService系统服务） |
| 自定义方法列表                 | 支持     | 注入一个自定义方法集合，该集合会在更新启动前执行。执行自定义方法列表如果出现任何异常，将通过异常订阅通知。（推荐在更新之前检查当前软件环境） |



##### 打包工具

GeneralUpdate.PacketTool使用.NET 8编写的桌面端打包程序。https://github.com/GeneralLibrary/GeneralUpdate.Tools

| 功能               | 支持   | 备注                                                         |
| ------------------ | ------ | ------------------------------------------------------------ |
| 差分更新包生成     | 是     | 根据上一个版本和当前版本进行对比找出需要更新的文件或新添加文件。 |
| 自动上传更新包     | 开发中 | 将生成好的差分更新包自动上传至服务器。                       |
| 可编辑选择差分文件 | 开发中 | 可视化查看差分包的打包内容，并可选择打包文件。               |



##### GeneralUpdate.SystemService发布/部署

GeneralUpdate.SystemService是一个windows系统服务，并不是部署在服务端的web api。它的主要作用是监听更新过程，以及更新崩溃之后还原。

**发布：**

推荐发布Single file，如果想发布AOT版本需要移除源码中映射代码。

```shell
dotnet publish -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true
```

**创建/部署windows服务：**

```shell
sc create MyWorkerService binPath="C:\your_path\GeneralUpdate.SystemService.exe"
```

**启动已部署的windows服务：**

```shell
sc start GeneralUpdate.SystemService
```

**删除已部署的windows服务：**

```shell
sc delete GeneralUpdate.SystemService
```



### 4.GeneralUpdate支持什么？

##### .NET框架

| 框架名称                   | 是否支持 |
| -------------------------- | -------- |
| .NET Core 2.0              | 支持     |
| .NET 5 ... to last version | 支持     |
| .NET Framework 4.6.1       | 支持     |



##### UI框架

| UI框架名称        | 是否支持              |
| ----------------- | --------------------- |
| WPF               | 支持                  |
| UWP               | 商店模式下不可更新    |
| MAUI              | 目前仅支持Android平台 |
| Avalonia          | 支持                  |
| WinUI             | 待验证，等待反馈      |
| Console（控制台） | 支持                  |
| Winform           | 支持                  |



##### 服务端框架

| 服务端框架 | 是否支持 |
| ---------- | -------- |
| ASP.NET    | 待验证   |



##### 操作系统

| 操作系统名称       | 是否支持 |
| ------------------ | -------- |
| Windows            | 支持     |
| Linux              | 待验证   |
| Mac                | 待验证   |
| Android            | 支持     |
| 树莓派(IoT)        | 待验证   |
| 麒麟V10(飞腾S2500) | 支持     |
| 麒麟V10(x64)       | 支持     |



### 赞助一下

尊敬的赞助者：

该项目以作者为主力设计、维护的自动升级组件。这个项目的目标是为全球的开发者和技术爱好者提供一个共享、学习和创新的平台，我们致力于推动技术的发展和共享知识的力量。

但是，我们需要您的支持。您的赞助将使我们能够进一步提高项目的质量，增加更多的功能，提供更好的用户体验。而且，作为回报，我们会在项目网站上展示您的公司标志，以表达我们对您的感激之情。

这是一个绝佳的机会，不仅可以推动技术的进步，也可以提高您的品牌知名度和社区影响力。我们相信，您的赞助将会对这个开源项目产生深远的影响。此外所有的赞助将用于购买GeneralUpdate实物周边产品奖励给项目贡献代码的社区伙伴，如果有实际贡献的小伙伴可以联系下方邮箱与我取得联系。

个人能力有限，目前项目的大小已经超出个人维护的精力范围之外设计维护实属不易，期待您的积极参与，让我们携手共创未来！

谢谢！

​          作者：Juster Zhu                                                                                                                                                                      



<img src="../imgs/payment.jpg" alt="payment" style="zoom:50%;" />