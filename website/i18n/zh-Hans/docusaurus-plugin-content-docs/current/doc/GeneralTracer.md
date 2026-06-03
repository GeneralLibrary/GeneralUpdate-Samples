---
sidebar_position: 8
---

### GeneralTracer

> **合并说明**：`GeneralUpdate.Common` 已合并到 `GeneralUpdate.Core`。`GeneralTracer` 现在位于 `GeneralUpdate.Core` 命名空间内，引用 `GeneralUpdate.Core` 包即可自动获得，无需单独引用。

命名空间：`GeneralUpdate.Core`

程序集：`GeneralUpdate.Core.dll`

`GeneralTracer` 提供组件内部异常信息和执行流程的追踪输出。它默认启用，引用 `GeneralUpdate.Core` 时会自动包含。

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

![](imgs\DebugView.png)



### 适用于

| Product        | Versions         |
| -------------- | ---------------- |
| .NET           | 5, 6, 7, 8, 9, 10 |
| .NET Framework | 4.6.1            |
| .NET Standard  | 2.0              |
| .NET Core      | 2.0              |

