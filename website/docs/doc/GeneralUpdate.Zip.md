---
sidebar_position: 5
---

### 定义

命名空间：GeneralUpdate.Zip

程序集：GeneralUpdate.Zip.dll



该组件提供压缩包文件的压缩和加压功能，GeneralZipFactory是该组件的核心类提供了zip、7z的压缩/解压缩的能力。

```c#
public class GeneralZipFactory : IFactory
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.Zip -Version 1.0.0
```



### 示例

以下示例定义方法，以包含压缩和解压缩的示例。

```c#
//创建压缩包
public void CreatZip()
{
    try
    {
        string sourcePath = "D:\\Updatetest_hub\\Run_app";
        string destinationPath = "D:\\Updatetest_hub";
        string name = "testpacket.zip";
        var factory = new GeneralZipFactory();
        factory.CompressProgress += (a, e) =>
        {
            Console.WriteLine($"fileName:{e.Name},fileSize:
        { e.Size},fileIndex: { e.Index},filePath: { e.Path},fileCount: { e.Count}
            ");
        };
        factory.Completed += (a, e) =>
        {
            Console.WriteLine($"IsCompleted:{e.IsCompleted}");
        };
        factory.CreatefOperate(OperationType.GZip, name, sourcePath,
        destinationPath, false, System.Text.Encoding.Default).
        CreatZip();
    }
    catch
    {
        Assert.Fail();
    }
    Assert.Pass();
}
        
//解压缩包
protected bool UnZip(BaseContext context)
{
    try
    {
        bool isComplated = false;
        var generalZipfactory = new GeneralZipFactory();
        generalZipfactory.UnZipProgress += (sender, e) =>
        EventManager.Instance.Dispatch<Action<object, MultiDownloadProgressChangedEventArgs>>(this, new MultiDownloadProgressChangedEventArgs(context.Version, ProgressType.Updatefile, "Updatting file..."));
        generalZipfactory.Completed += (sender, e) => isComplated = true;
        generalZipfactory.CreateOperate(MatchType(context.Format), context.Name, context.ZipfilePath, context.TargetPath, false, context.Encoding).
            UnZip();
        return isComplated;
    }
    catch (Exception exception)
    {
        EventManager.Instance.Dispatch<Action<object, ExceptionEventArgs>>(this, new ExceptionEventArgs(exception));
        return false;
    }
}
```



### 注解

GeneralZipFactory提供以下能力。

| 功能   | 是否支持 | 备注                                      |
| ------ | -------- | ----------------------------------------- |
| 压缩   | 是       |                                           |
| 解压缩 | 是       |                                           |
| ZIP    | 是       | 支持ZIP压缩格式，压缩速度快文件体积适中。 |
| 7Z     | 是       | 支持7Z压缩格式，压缩速度慢文件体积小。    |

#### 方法

| 名称             | 类型 | 备注             |
| ---------------- | ---- | ---------------- |
| UnZip()          | 方法 | 解压             |
| CreatZip()       | 方法 | 创建压缩包       |
| CreatefOperate() | 方法 | 压缩文件操作配置 |

#### 事件

| 名称             | 类型 | 备注         |
| ---------------- | ---- | ------------ |
| UnZipProgress    | 事件 | 解压进度     |
| CompressProgress | 事件 | 压缩进度     |
| Completed        | 事件 | 操作完成通知 |



### 🌼CreatefOperate()

**方法**

初始化压缩包操作参数。

```c#
public IFactory CreateOperate(OperationType type, string name, string sourcePath, string destinationPath, bool includeBaseDirectory = false, Encoding encoding = null);
```



**参数**

**type** Enumeration selects the compressed package format to operate on.(OperationType.GZip , OperationType.G7z)

**name** Compressed package Name.

**sourcePath** Source file path.

**destinationPath** The target path.

**includeBaseDirectory** Whether to include the root directory when packing.

**encoding** Compressed package encoding format.



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |