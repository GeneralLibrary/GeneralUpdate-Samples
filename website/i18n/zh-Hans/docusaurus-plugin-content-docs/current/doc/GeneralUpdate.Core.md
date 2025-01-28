---
sidebar_position: 5
---

### 定义

命名空间：GeneralUpdate.Core

程序集：GeneralUpdate.Core.dll



GeneralUpdate.Core是最核心的组件之一，提供了大量主要功能。当主程序升级操作完成之后会通过进程 启动并传参的方式调用本组件来完成主程序升级操作。（主要职责更新主程序）

```c#
public class GeneralUpdateBootstrap : AbstractBootstrap<GeneralUpdateBootstrap, IStrategy>
```



### 示例

GeneralUpdateBootstrap使用代码示例 [进入](https://github.com/GeneralLibrary/GeneralUpdate-Samples/blob/main/src/Upgrade/Program.cs)。

![](imgs/muti_donwload.png)



### 注解

GeneralUpdateBootstrap提供以下能力。

#### 构造函数

| Constructors             |                                    |
| ------------------------ | ---------------------------------- |
| GeneralUpdateBootstrap() | 当前GeneralUpdateBootstrap构造函数 |
| base:AbstractBootstrap() | 父类AbstractBootstrap构造函数      |

#### 属性

| Properties   |                      |
| ------------ | -------------------- |
| Packet       | 更新包信息           |
| UpdateOption | 更新操作配置设置枚举 |

#### 方法

| Method                                 |                                                  |
| -------------------------------------- | ------------------------------------------------ |
| LaunchTaskAsync()                      | Task异步启动更新                                 |
| LaunchAsync()                          | 启动更新                                         |
| Option()                               | 设置更新配置。                                   |
| GetOption()                            | 获取更新配置。                                   |
| AddListenerMultiAllDownloadCompleted() | 监听所有更新版本下载完成事件                     |
| AddListenerMultiDownloadCompleted()    | 监听每个版本下载完成事件                         |
| AddListenerMultiDownloadError()        | 监听每个版本下载异常的事件                       |
| AddListenerMultiDownloadStatistics()   | 监听每个版本下载统计信息\下载进度事件            |
| AddListenerException()                 | 监听更新组件内部的所有异常                       |
| SetFieldMappings()                     | 设置字段映射表，用于解析所有驱动包的信息的字符串 |



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
