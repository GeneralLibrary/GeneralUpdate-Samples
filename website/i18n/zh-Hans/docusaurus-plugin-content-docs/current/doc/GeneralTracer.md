---
sidebar_position: 8
---

### GeneralTracer

命名空间：GeneralUpdate.Common.Shared

程序集：GeneralUpdate.Common.dll



GeneralUpdate.Common是公共组件，提供核心组件基础代码一般不需要手动引用，引用GeneralUpdate.ClientCore或GeneralUpdate.Core时自动引用。

```c#
public static class GeneralTracer
```



### Example

该机制默认开启，将组件内部的异常信息、部分执行流程信息同时可通过IDE控制台、log文件、Dbgview调试工具进行查看和分析。

```c#
//如果在生产环境或不需要使用Trace机制，可通过以下方法关闭，默认开启。
GeneralTracer.SetTracingEnabled(false);

//在引导类之前调用即可。
 _ = await new GeneralClientBootstrap()
               .LaunchAsync();
```



#### IDE控制台

![](imgs\IDE.png)



#### Log文件

```c#
该文件默认生成在运行目录下的Logs文件夹中，文件名格式为generalupdate-trace [today].log 该log未依赖第三方组件。
```

![](imgs\log.png)



#### Dbgview调试工具

![](imgs\debugview.png)



### 适用于

| Product        | Versions         |
| -------------- | ---------------- |
| .NET           | 5, 6, 7, 8, 9,10 |
| .NET Framework | 4.6.1            |
| .NET Standard  | 2.0              |
| .NET Core      | 2.0              |

