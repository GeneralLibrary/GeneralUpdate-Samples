---
slug: quikstart
title: GeneralUpdate
authors: juster
tags: [quikstart]
---





## 示例 UI

仓库地址：

- https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Client/ClientSample.sln
- https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/UpgradeSample.sln

![](imgs\sampleclient.png)

![](imgs\sampleupgrade.png)



## Step1

在Github上下载Sample仓库，使用示例前请确保本地安装了.NET 8的运行环境。

- https://github.com/GeneralLibrary/GeneralUpdate-Samples

仓库目录内容如下：

![](imgs\content.png)

| 名称         | 说明                             |
| ------------ | -------------------------------- |
| Client       | 主客户端示例程序                 |
| Server       | 服务端示例程序                   |
| StartManager | 更新流程控制台                   |
| Upgrade      | 升级端示例程序                   |
| process.bat  | 无需关注                         |
| resource.bat | 无需关注                         |
| start.cmd    | 启动更新示例的脚本，一切从它开始 |



## Step2

找到文件目录，并双击（每次启动start.cmd脚本都会重置本地目录所以无需手动管理文件目录）：

```shell
...\GeneralUpdate-Samples\src\start.cmd
```

![](imgs\build.png)



如下图所示，自动开始编译并拷贝所有相关项目的bin目录到app目录下：

```
...\GeneralUpdate-Samples\src\run\app
```

![](imgs\build.png)



这时候就会出现StartManager.exe的界面，所有的选项均为字面意思，我们先别着急输入1这时候先看看app目录更新之前的装备后续方便我们验证更新结果。

![](imgs\manager.png)



我们进入到app目录之后，可以看到这是没有升级之前的目录。

![](imgs\rundir.png)



## Step3

检查完app目录之后：

- 输入选项1然后回车
- 服务端示例程序启动
- 主客户端示例程序启动，开始请求更新（主客户端程序更新完成之后会自动关闭）。

![](imgs\upgrade.png)



看到主客户端程序自动关闭，我们再去检查一下run\app目录。会发现多了一个备份目录“app-1.0.0.0”和一个“Congratulations on the update.txt”。

![](imgs\rundir2.png)

看到这里代表您已经成功的完成了一次升级，Congratulations！

![](imgs\result.png)
