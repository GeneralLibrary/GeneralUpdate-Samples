---
sidebar_position: 6
---

### 定义

命名空间：GeneralUpdate.Differential

程序集：GeneralUpdate.Core.dll



该组件提供文件的二进制差分补丁文件生成（算法），文件增量识别（版本差异）、删除文件识别（版本差异）、黑名单、补丁还原等能力。

```c#
public sealed class DifferentialCore
```



### 示例

以下示例定义方法，以包含增量识别，生成二进制补丁、补丁还原、设置黑名单示例GeneralUpdate.Tools项目中的打包工具也是强依赖此组件。

```c#
//增量识别，生成二进制补丁
public async Task TestDifferentialClean()
{
    //上一个版本的客户端文件夹路径
    var path1 = "D:\\packet\\source";
    //最新版本客户端文件夹路径
    var path2 = "D:\\packet\\target";
    //补丁文件生成路径
    var path3 = "D:\\packet\\patchs";
    await DifferentialCore.Instance.Clean(path1, path2, path3);
}

//补丁还原
public async Task TestDifferentialDirty()
{
    //当前版本的客户端文件夹路径
    var path1 = "D:\\packet\\source";
    //补丁文件生成路径
    var path2 = "D:\\packet\\patchs";
    await DifferentialCore.Instance.Dirty(path1, path2);
}
```



### 注解

DifferentialCore提供增量识别，生成二进制补丁、补丁还原、设置黑名单能力。

#### 方法

| 名称    | 类型 | 备注                                                 |
| ------- | ---- | ---------------------------------------------------- |
| Clean() | 方法 | 增量识别，删除文件识别，生成二进制补丁文件           |
| Dirty() | 方法 | 补丁还原（将补丁打到旧的客户端文件上达到更新的目的） |



### 🌼Clean()

**方法**

生成补丁文件[不能包含文件名相同但扩展名不同的文件]。

```c#
public async Task Clean(string sourcePath, string targetPath, string patchPath = null);
```

**参数**

**sourcePath** 上一个版本的文件夹路径。

**targetPath** 最近版本的文件夹路径。

**patchPath** 将发现的增量更新文件存储在临时目录中。



### 🌼Dirty()

**方法**

应用补丁[不能包含文件名相同但扩展名不同的文件]。

```c#
public async Task Dirty(string appPath, string patchPath);
```

**参数**

**appPath** 客户端应用程序目录。

**patchPath** 补丁文件路径。



### 适用于

| 产品           | 版本          |
| -------------- | ------------- |
| .NET           | 5、6、7、8、9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |