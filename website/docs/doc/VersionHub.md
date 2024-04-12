---
sidebar_position: 10
---

### 定义

命名空间：GeneralUpdate.ClientCore.Hubs

程序集：GeneralUpdate.ClientCore.dll



VersionHub是基于SignalR实现的推送更新版本信息的机制，可以实现一对一和一对多的推送。

```c#
public sealed class VersionHub<TParameter> where TParameter : class
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.ClientCore -Version 1.0.0
```



### 示例

以下示例定义方法，包含VersionHub使用。

```c#
//Receive sample code pushed by the server
VersionHub<string>.Instance.Subscribe($"{baseUrl}/{hubName}", "TESTNAME", new Action<string>(GetMessage));
```

**（1）点对点推送**

![](imgs/maui_windows_push_version.png)



**（2）一次推送更新给多个客户端**

![push_version_mutil](imgs/push_version_mutil.png)



### 注解

VersionHub提供服务器对客户端一对一、一对多的推送功能。

#### 属性

| Properties |                            |
| ---------- | -------------------------- |
| Instance   | VersionHub的单例实例对象。 |

#### 方法

| Method      |                                    |
| ----------- | ---------------------------------- |
| Subscribe() | 实时订阅服务端推送的最新版本信息。 |
| Send()      | 向服务器发送消息。[目前不建议使用] |



### 🌴Instance

VersionHub的单例实例对象（线程安全双检索实现的单例模式）。



### 🌼Subscribe()

**方法**

订阅推送最新版本信息.

```c#
public void Subscribe(string url, string name, Action<TParameter> receiveMessageCallback, Action<string> onlineMessageCallback = null, Action<string> reconnectedCallback = null);
```



**参数类型**

TParameter

服务端推送的最新版本信息，可以是任何约定好的对象（推荐用法直接推送json序列化后的对象字符串）。



**参数**

```c#
url string http请求的api地址。

name string 名称需要保证是唯一的。

receiveMessageCallback Action<TParameter> 接收服务端推送信息的回调函数。

onlineMessageCallback Action<string> 接收是否在线或离线的回调函数。

reconnectedCallback Action<string> 接收是否重新连接服务端的回调函数。
```



### 🌼Send()

**方法**

向服务器发送消息。[暂不推荐使用]

```c#
public async Task Send(string msg);
```



**参数**

**msg** string 向服务端发送消息（推荐用法当接收最新版本推送且更新完成之后，使用此方法上报更新结果）。发送的消息内容需要进行约定没有固定的数据格式。



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |
| ASP.NET        | Any           |