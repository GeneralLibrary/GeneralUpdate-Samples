---
sidebar_position: 3
sidebar_label: 🎨 界面生成
title: 🎨 generalupdate-ui — 更新界面生成
---

# 🎨 GeneralUpdate 更新界面生成指南

如果你的应用需要**让用户看到更新进度**（而不是在后台静默更新），这个页面会帮你生成合适的 UI 代码。

---

## 先理解：两个程序各自显示什么

GeneralUpdate 是双进程架构，更新时两个程序都会显示界面：

```
你的主程序（Client）
    显示 "检查更新 → 下载进度"
    用户能看到：正在下载 45% / 下载速度 / 剩余时间

    ↓ 下载完成后启动 ↓

升级程序（Upgrade）
    显示 "正在安装更新，请稍候..."
    用户能看到：安装进度条 / 安装失败提示
```

> **Client 端**：负责下载，适合显示进度条、速度、版本信息
> **Upgrade 端**：负责安装替换文件，过程很快，通常只显示等待界面

---

## 你属于哪种情况？

| 情况 | 需要做什么 |
|------|-----------|
| **已有主程序，想要加更新窗口** | 在现有项目上加一个更新窗口（本页会帮你生成） |
| **从零开始搭建** | 先用 [generalupdate-init](generalupdate-init) 搭建双项目结构，再回来加 UI |
| **不需要进度 UI（静默更新）** | 走 [generalupdate-strategy](generalupdate-strategy) 的静默模式 |

---

## 支持哪些 UI 框架

| 框架 | 模板文件 | 特点 |
|------|---------|------|
| **WPF + LayUI.Wpf** | `LayUIStyle.xaml` | 玻璃效果进度条 |
| **WPF + WPFDevelopers** | `WPFDevelopersStyle.xaml` | 圆形进度 + 呼吸灯动画 |
| **Avalonia + SemiUrsa** | `SemiUrsaClientView.axaml` | 跨平台，暗黑切换 |
| **WinForms + AntdUI** | `AntdUIStyle.cs` | 暗黑主题 |
| **MAUI** | `MauiUpdatePage.xaml` | 移动端 + 桌面端 |

无论哪种框架，更新窗口的**逻辑都是相同的**，只是界面样式不同。

---

## 更新窗口的 4 个关键状态

一个更新窗口本质上就是一个**状态机**，只有 4 种主要状态：

```
┌────────────────────────────────────────────────────┐
│  ① 检查中                                         │
│  "正在检查更新..."                                 │
│        │                                           │
│        ▼                                           │
│  ② 下载中（核心状态）                              │
│  ┌────────────────────────────────────────────────┐│
│  │ MyApp_2.0.0.0.zip   45%                        ││
│  │ ████████████████░░░░░░░░░                      ││
│  │ 下载速度: 3.2 MB/s  剩余: 12秒                 ││
│  └────────────────────────────────────────────────┘│
│        │                                           │
│        ▼                                           │
│  ③ 安装中                                         │
│  "正在安装更新，请勿关闭计算机..."                 │
│        │                                           │
│        ▼                                           │
│  ④ 完成 / 失败                                    │
│  "更新完成！" / "下载失败，请重试"                 │
└────────────────────────────────────────────────────┘
```

除了这 4 种主要状态，还可能有"已是最新版本"、"暂停"、"重试"等子状态，但**新手只需要关注 ① 和 ②**。

---

### 如何把更新进度连到 UI 上？

GeneralUpdate 的事件系统会告诉你进度变化，你只需要把事件里的数字填到 UI 控件上：

```csharp
// GeneralUpdate 更新进度事件 → 更新你的 UI 控件
bootstrap.AddListenerMultiDownloadStatistics((_, e) =>
{
    // e.ProgressPercentage  → 你的进度条 Value（0-100）
    // e.Speed              → 你的下载速度标签
    // e.Remaining          → 你的剩余时间标签
    // e.BytesReceived      → 已下载字节
    // e.TotalBytesToReceive → 总字节
});
```

就是这么简单。**核心工作就是把事件里的 5 个数字绑定到 UI 控件上。**

---

## 生成 UI 代码前的信息收集

```
### 你的项目
- UI 框架: ______（WPF / WinForms / Avalonia / MAUI / 控制台）
- 是否需要暗黑模式: ______（是/否）

### 更新窗口角色（看上面的"各自显示什么"）
- Client 端（主程序）: ______（需要显示下载进度？）
- Upgrade 端（升级程序）: ______（需要显示安装等待？）
```

---

## 简单示例：控制台进度条

即使你没有图形界面，一个控制台进度条也很简单：

```csharp
await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        // 控制台进度条：3 行代码
        Console.Write($"\r下载进度: {e.ProgressPercentage}% ");
        Console.Write($"[{new string('■', (int)(e.ProgressPercentage / 5))}");
        Console.WriteLine($"{new string('□', 20 - (int)(e.ProgressPercentage / 5))}]");
        Console.Write($"速度: {e.Speed}/s");
    })
    .LaunchAsync();
```

---

## 已知问题和注意事项

| # | 问题 | 说明 |
|---|------|------|
| 1 | **更新 UI 不要做耗时操作** | 事件回调里只更新 UI，不要写文件或发网络请求 |
| 2 | **跨线程更新控件** | WPF 用 `Dispatcher.Invoke`，WinForms 用 `Control.Invoke` |
| 3 | **Upgrade 端不需要下载进度** | Upgrade 只负责安装，显示 "请稍候" 即可 |
| 4 | **下载失败要显示重试按钮** | 否则用户卡在 99% 不知道怎么办 |
| 5 | **进度条不要一次跳到 100%** | 用户需要看到中间过程才有安全感 |

---

## 进阶：全状态 UI 模板

如果你需要更完整的 UI（包括暂停、恢复、重试、暗黑切换等 11 种状态），Skill 仓库提供了完整的模板代码。

生成方式：在 Claude Code 中描述你的 UI 框架，AI 会自动选择对应的模板并生成代码。

---

## 相关页面

- [generalupdate-init](generalupdate-init) — 先搭建双项目结构
- [generalupdate-strategy](generalupdate-strategy) — 如果不想要 UI（静默更新）
- [generalupdate-advanced](generalupdate-advanced) — 高级定制
