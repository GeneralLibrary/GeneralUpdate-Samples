---
sidebar_position: 3
sidebar_label: 🎨 界面生成
title: 🎨 generalupdate-ui — 更新界面生成
---

# 🎨 GeneralUpdate 更新界面生成 — 全状态覆盖

自动检测开发者的 UI 框架类型，生成带真实 GeneralUpdate.Core 事件绑定的完整更新窗口代码。

> ⚠️ 针对 NuGet v10.5.0-beta.4。`RealDownloadService.cs` 已使用 `UpdateRequest` 和正确的命名空间。

---

## 📋 用户需求提取

```
### UI 框架（必需）
- 目标框架: ______（WPF/WinForms/Avalonia/MAUI/控制台/不确定）
- 偏好 UI 库: ______（默认推荐 / LayUI.Wpf / WPFDevelopers / AntdUI / SemiUrsa / 原生）
- 是否已有项目模板: ______（是/否，如果否，从 generalupdate-init 开始）

### 更新场景（必需）
- 更新窗口角色: ______（Client 端/ Upgrade 端/ 两端都需要）
- 是否需要手动触发更新: ______（是/否，自动启动时检查）
- 是否支持暗黑模式: ______（是/否）

### 高级 UI 需求（可选）
- 需要自定义品牌色/Logo: ______（是/否）
- 需要多语言支持: ______（是/否）
- 需要无障碍支持: ______（是/否）
```

---

## 工作流程

```
1. 框架探测
   ├── 扫描 .csproj → PackageReference 识别 UI 库
   ├── 如果无法识别 → 询问用户
   └── 如果无 UI 框架 → 控制台进度条

2. 状态代码生成
   ├── IDownloadService 桥接接口
   ├── RealDownloadService 桥接代码（手动适配 GeneralUpdate.Core 事件）
   ├── ViewModel（MVVM）或 Code-Behind
   └── 窗口/页面 XAML

3. 集成指导
   ├── 如何引入 GeneralUpdateBootstrap
   └── Bootstrap 配置（与 generalupdate-init 配合）
```

---

## UI 状态机（所有模板覆盖以下状态）

```
                   ┌─────────────┐
                   │    Idle     │ ← 初始状态
                   └──────┬──────┘
                          │ 自动/手动触发
                          ▼
                   ┌─────────────┐
            ┌─────│  Checking    │ ← "正在检查更新..."
            │     └──────┬──────┘
            │            │
            │     ┌──────┴──────┐
            │     ▼             ▼
            │  ┌────────┐  ┌──────────┐
            │  │ Latest │  │  Found!  │ ← 显示版本号/大小
            │  └────────┘  └────┬─────┘
            │                   │ 用户点击"开始更新"
            │                   ▼
            │            ┌──────────────┐
            │      ┌─────│ Downloading  │ ← 进度条/速度/剩余时间
            │      │     └──────┬───────┘
            │      │            │
            │      │     ┌──────┴──────┐
            │      │     ▼             ▼
            │      │  ┌────────┐  ┌──────────┐
            │      │  │ Paused │  │  Error   │ ← 显示错误 + "重试"
            │      │  └───┬────┘  └────┬─────┘
            │      │      │ 继续        │ 重试
            │      │      ▼             ▼
            │      │  ┌──────────────┐
            │      │  │ Downloading  │
            │      │  └──────────────┘
            │      │
            │      │     ┌──────────────┐
            │      └────→│  Applying    │ ← "正在安装更新..."
            │             └──────┬───────┘
            │                    │
            │             ┌──────┴──────┐
            │             ▼             ▼
            │       ┌─────────┐  ┌──────────┐
            │       │ Success │  │  Failed  │
            │       └────┬────┘  └──────────┘
            │            │
            │            ▼
            │       ┌──────────┐
            │       │ Restart  │ ← 重启应用
            │       └──────────┘
            │
            └── 回到 Idle
```

---

## 核心桥接：RealDownloadService

所有 UI 模板共享这个桥接类，将 GeneralUpdate.Core 的事件映射到 `IDownloadService` 接口。

### 桥接逻辑（v10.5.0-beta.4）

```csharp
// GeneralUpdate.Core 事件 → DownloadStatus 状态机映射：

GeneralUpdateBootstrap.AddListenerMultiDownloadStatistics
    → Downloading（更新 ProgressPercentage/Speed/Remaining）

GeneralUpdateBootstrap.AddListenerMultiDownloadCompleted
    → 文件处理中（解压/校验）

GeneralUpdateBootstrap.AddListenerMultiAllDownloadCompleted
    → Applying → Success

GeneralUpdateBootstrap.AddListenerMultiDownloadError
    → DownloadError（自动重试 N 次后）

GeneralUpdateBootstrap.AddListenerException
    → Failed（非致命异常不改变状态）
```

---

## UI 框架模板清单

| 模板文件 | 适用框架 | 包含特性 |
|---------|---------|---------|
| `SemiUrsaClientView.axaml` + `.cs` | Avalonia + SemiUrsa | 全状态机、暗黑切换、动画 |
| `SemiUrsaUpgradeView.axaml` + `.cs` | Avalonia + SemiUrsa (Upgrade) | 等待中 UI |
| `LayUIStyle.xaml` + `.cs` | WPF + LayUI.Wpf | 玻璃效果、进度条 |
| `WPFDevelopersStyle.xaml` + `.cs` | WPF + WPFDevelopers | 圆形进度、呼吸灯动画 |
| `AntdUIStyle.cs` | WinForms + AntdUI | 暗黑主题、波浪进度按钮 |
| `MauiUpdatePage.xaml` + `.cs` | MAUI | 深色模式、AppThemeBinding |
| `DownloadViewModels.cs` | 所有框架共用 | MVVM ViewModel |
| `RealDownloadService.cs` | 所有框架共用 | **核心桥接** |

---

## ✅ 集成验证清单

### 事件桥接
- [ ] 所有 7 个事件都已绑定（包括 AddListenerProgress）
- [ ] 桥接代码使用正确的 EventArgs 类型（检查命名空间 `GeneralUpdate.Core.Download` / `GeneralUpdate.Core.Event`）
- [ ] `IsCompleted` 属性名正确（v10.5.0-beta.4 使用 `IsCompleted`）

### 线程安全
- [ ] UI 更新操作在正确的线程上执行（WPF/Avalonia 用 `Dispatcher`，WinForms 用 `Invoke`，MAUI 用 `MainThread`）
- [ ] `MultiDownloadStatistics` 事件中不执行耗时操作（仅更新 UI）
- [ ] 下载完成后的"正在应用"状态有超时保护（建议 > 30 秒显示进度提示）

### 状态机覆盖
- [ ] 所有 11 个状态都已实现（Idle → Checking → Latest/Found → Downloading → Paused → Error → Retrying → Applying → Success/Failed → Restart）
- [ ] 下载错误的自动重试次数有限制（不超过 3 次）
- [ ] 用户可取消更新操作

### 框架特定检查
- [ ] **Avalonia**: ViewModel 实现 `INotifyPropertyChanged`，绑定使用 `{Binding}`
- [ ] **WPF**: 使用 `Dispatcher.Invoke` 更新绑定的属性
- [ ] **WinForms AntdUI**: 使用 `Control.Invoke` 进行跨线程更新
- [ ] **MAUI**: 检查 `Platform.CurrentActivity` 在 Android 上的生命周期

---

## ⚠️ 反模式清单

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **通用 ViewModel 直接用在不同框架** | 线程模型不兼容导致跨线程异常 | 按框架分别适配 Dispatcher/Invoke/MainThread |
| 2 | **在下载统计事件中做文件 IO 或网络请求** | 阻塞更新流程，UI 卡顿 | 仅更新 UI 绑定的属性 |
| 3 | **进度条绑定一次性更新到 100%** | 用户看不到中间过程，体验差 | 使用 `e.ProgressPercentage` 逐步更新 |
| 4 | **未处理 MultiDownloadError 事件** | 下载失败时用户无反馈，卡在等待状态 | 至少显示错误信息 + 重试按钮 |
| 5 | **未区分 Client 和 Upgrade 的 UI** | Upgrade 端显示不必要的"下载进度" | Upgrade 端只显示"正在安装，请稍候" |
| 6 | **直接使用 RealDownloadService.cs 不做适配** | 事件绑定不生效 | 必须根据项目结构调整 `IDownloadService` 实现 |
| 7 | **Avalonia/WPF 在 ViewModel 构造函数中启动更新** | UI 还未初始化完成，绑定不生效 | 在 Loaded 事件或 View 层触发检查更新 |

---

## 相关技能

- `/generalupdate-init` — 如果还未配置 Bootstrap
- `/generalupdate-strategy` — 如果想要 Silent 模式不需要 UI
- `/generalupdate-troubleshoot` — 如果 UI 显示异常
