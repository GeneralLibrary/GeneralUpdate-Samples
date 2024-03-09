## GeneralUpdate.Differential

### 定义

命名控件：GeneralUpdate.Differential

程序集：GeneralUpdate.Differential.dll



该组件提供文件的二进制差分补丁文件生成（算法），文件增量识别（版本差异）、删除文件识别（版本差异）、黑名单、补丁还原等能力。

```c#
public sealed class DifferentialCore
```

nuget安装

```shell
NuGet\Install-Package GeneralUpdate.Differential -Version 1.0.0
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

//设置黑名单
public async Task TestSetBlocklist()
{
    //blackFiles集合中指定的文件不参与更新
    List<string> blackFiles = {"1.txt","abc.dll"};
    //blackFileFormats集合中指定的文件后缀文件不参与更新
    List<string> blackFileFormats = {".dll" , "txt"}
    await DifferentialCore.Instance.SetBlocklist(blackFiles, blackFileFormats);
}
```



### 注解

DifferentialCore提供以下能力。

#### 方法

| 名称           | 类型 | 备注                                                 |
| -------------- | ---- | ---------------------------------------------------- |
| Clean()        | 方法 | 增量识别，删除文件识别，生成二进制补丁文件           |
| Dirty()        | 方法 | 补丁还原（将补丁打到旧的客户端文件上达到更新的目的） |
| SetBlocklist() | 方法 | 设置黑名单（文件、文件后缀）                         |



### Clean()

Generate patch file [Cannot contain files with the same name but different extensions] .

**参数**

**sourcePath** Previous version folder path .

**targetPath** Recent version folder path.

**patchPath** Store discovered incremental update files in a temporary directory .



### Dirty()

Apply patch [Cannot contain files with the same name but different extensions] .

**参数**

**appPath** Client application directory .

**patchPath** Patch file path.



### SetBlocklist()

Set a blacklist.

**参数**

**blackFiles** A collection of blacklist files that are skipped when updated.

**blackFileFormats ** A collection of blacklist file name extensions that are skipped on update.