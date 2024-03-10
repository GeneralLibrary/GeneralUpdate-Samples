## GeneralUpdate.SystemService

### 定义

命名空间：GeneralUpdate.SystemService

程序集：GeneralUpdate.SystemService.dll



GeneralUpdate.SystemService是一个跨平台的操作系统服务，它几乎不参与任何更新（如果需要更新则需要考虑附加在客户端的安装包里）。它的主要作用是帮助恢复客户端文件损坏导致无法正常运行，无差别回滚到上一个版本。

```c#
using GeneralUpdate.SystemService.Services;

namespace GeneralUpdate.SystemService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<WillMessageService>();
            var host = builder.Build();
            host.Run();
        }
    }
}
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



### 适用于

| 产品 | 版本 |
| ---- | ---- |
| .NET | 8、9 |