---
sidebar_position: 1
---

### Permission 权限

#### （1）Windows平台

![](imgs\UAC.png)

在使用GeneralUpdate实现自动升级的时候，如果更新目录在C盘实现文件替换或打文件补丁时会出现权限问题。又因为windows11操作系统推出在C盘特定的目录相比之前推出的windows的操作系统加强了权限管理。

那么稍微不注意将会触碰到权限管理的边界，接下来我们看看操作哪些目录会导致出现权限问题：

| 名称           | 目录                                       |
| -------------- | ------------------------------------------ |
| 系统文件夹     | C:\Windows                                 |
| 注册表配置文件 | C:\Windows\System32\config                 |
| 驱动文件夹     | C:\Windows\System32\drivers                |
| 程序文件夹     | C:\Program Files 和 C:\Program Files (x86) |

推荐使用目录，避免权限问题：

| 名称         | 目录    |
| ------------ | ------- |
| 用户数据目录 | AppData |
| 系统临时目录 | Temp    |



### UAC降权

以下方法不推荐在生产环境中使用，以免给用户造成损失。如果在更新过程中出现UAC (User Account Control)提示或无权限、拒绝访问的情况可以考虑降低UAC控制等级，这个思路在代码层面可以通过修改以下注册表达到目的：

| 注册表名称                 | 修改值 | 默认值 |
| -------------------------- | ------ | ------ |
| enableLUA                  | 0      | 1      |
| ConsentPromptBehaviorAdmin | 0      | 5      |

更新之前修改以上注册表（重启计算机生效），切记更新完成之后需要恢复该内容。



c#修改注册表：

```c#
using Microsoft.Win32;

public void UpdateRegistry()
{
    const string keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
    {
        if (key != null)
        {
            key.SetValue("EnableLUA", 0, RegistryValueKind.DWord);
            key.SetValue("ConsentPromptBehaviorAdmin", 0, RegistryValueKind.DWord);
        }
    }
}
```



bat批处理修改注册表：

```bat
@echo off
REG ADD "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v EnableLUA /t REG_DWORD /d 0 /f
REG ADD "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v ConsentPromptBehaviorAdmin /t REG_DWORD /d 0 /f
```



参考资料：

- https://learn.microsoft.com/zh-cn/windows/security/application-security/application-control/user-account-control/how-it-works
- https://blog.walterlv.com/post/windows-user-account-control.html