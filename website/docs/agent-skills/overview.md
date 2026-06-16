---
sidebar_position: 1
sidebar_label: 📋 总览
title: 🚀 Agent Skills 总览
---

# 🚀 GeneralUpdate Agent Skills 套件

**Claude Code 技能套件** — 帮助 .NET 开发者在 5 分钟内将 [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) 自动更新系统集成到任意 .NET 应用中。

覆盖 50+ 真实 Issue 发现的已知问题，提供即用型代码生成 + 深度故障排查。

> **Current Version: 0.0.1-bate.1** — targets NuGet `GeneralUpdate.Core ≥ 10.4.6` stable release  
> 兼容性：`v10.4.6`（NuGet 最新稳定版）  
> 所有 32 个模板文件已通过 `dotnet build` 编译验证（0 errors）。

---

## 安装 Skills

### 获取 Skill 文件

所有 Skill 文件位于 `GeneralUpdate-Samples` 仓库的 `tmpgeneralupdate-skill-codegen` 目录下：

```
samples-repo/
└── src/Hub/Samples/
    └── tmpgeneralupdate-skill-codegen/
        ├── SKILL.md                          ← 套件入口（必装）
        └── .claude/skills/
            ├── generalupdate-init/           ← 🚀 集成指南
            ├── generalupdate-ui/             ← 🎨 界面生成
            ├── generalupdate-strategy/       ← ⚙️ 策略指南
            ├── generalupdate-advanced/       ← 🔧 高级定制
            └── generalupdate-troubleshoot/   ← 🩺 故障排查
```

### 安装到 Claude Code

**方式一（推荐）**：将整个技能套件克隆到你项目的 `.claude` 目录：

```bash
# 在你的项目根目录下执行
mkdir -p .claude/skills
cp -r <samples-path>/src/Hub/Samples/tmpgeneralupdate-skill-codegen/.claude/skills/* .claude/skills/
cp <samples-path>/src/Hub/Samples/tmpgeneralupdate-skill-codegen/SKILL.md .claude/
```

**方式二**：如果已有 `.claude` 目录，只复制 skills 子目录：

```bash
# 在你的项目根目录下执行
cp -r <samples-path>/src/Hub/Samples/tmpgeneralupdate-skill-codegen/.claude/skills .claude/
```

### 验证安装

启动 Claude Code，输入 `/generalupdate-init`，如果看到集成指南输出，说明安装成功。你也可以直接描述需求让 Skills 自动激活，例如：

> *"给我的 WPF 应用添加自动更新"*

---

## 先决条件

1. **Claude Code**: 需要安装并配置 [Claude Code CLI](https://claude.com/claude-code)
2. **.NET SDK**: 目标项目需基于 .NET 8+（推荐 .NET 10）
3. **GeneralUpdate 服务端**: 对于标准策略，需要部署 [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) 或兼容的后端服务
4. **双进程架构**: 需要理解 Client + Upgrade 双进程的核心理念

---

## Skills 总览

| Skill | 命令 | 描述 | 覆盖范围 |
|-------|------|------|----------|
| 🚀 **generalupdate-init** | `/generalupdate-init` | 双项目脚手架 + Bootstrap 配置（4 种方式） | 4 大场景 + 4 种配置方式 + 完整 API |
| 🎨 **generalupdate-ui** | `/generalupdate-ui` | 自动识别 UI 框架，生成全状态更新窗口（11 种状态） | 6 UI 框架 + 全状态机 + 桥接代码 |
| ⚙️ **generalupdate-strategy** | `/generalupdate-strategy` | 6 种策略决策树 + 混合组合 + 平台差异 | 6 策略 + 4 组合 + 平台对照 |
| 🔧 **generalupdate-advanced** | `/generalupdate-advanced` | 10+ 扩展点 + 4 种 IPC + Bowl + AOT | 10+ 扩展点 + 完整架构图 |
| 🩺 **generalupdate-troubleshoot** | `/generalupdate-troubleshoot` | 50+ 已知问题诊断 + 6 步通用排查 | 8 致命 + 11 高 + 20 中 + 12 低 |

---

## 快速开始

在 Claude Code 中，只需描述你的需求，对应的 Skill 会自动激活：

```
"给我的 WPF 应用添加自动更新"
→ 自动激活 generalupdate-init + generalupdate-ui

"更新成功了但启动报错"
→ 自动激活 generalupdate-troubleshoot

"配置 OSS 静默更新"
→ 自动激活 generalupdate-strategy

"添加 Bowl 崩溃守护 + 自定义 Hooks"
→ 自动激活 generalupdate-advanced
```

---

## 数据来源

所有技能的内容基于以下真实数据：

- **GitHub Issues**: #308–#517（重构、Bug、功能、测试）
- **Gitee Issues**: 30 个真实用户反馈（中文社区痛点）
- **全面代码审计**: 17 CRITICAL/HIGH + 14 MEDIUM + 10 INFO 发现
- **Samples 源码**: CompleteUpdateSample、SilentUpdateSample、OssSample、DifferentialSample、PushSample、BowlSample、ExtensionSample、CompressSample、ImDiskQuickInstallSample
- **UI Samples**: SemiUrsa、LayUI、AntdUI、WPFDevelopers、MauiUpdate、AndroidUpdate

---

## Skill 文件结构

```
.claude/skills/
├── generalupdate-init/         (7 files)
│   ├── SKILL.md                ← 4 大场景 + 4 种配置 + API 详解
│   ├── reference.md            ← NuGet/API/协议/框架兼容性
│   └── templates/
│       ├── MinimalIntegration.cs
│       ├── FullIntegration.cs
│       ├── generalupdate.manifest.json
│       └── project-scaffold/
│
├── generalupdate-ui/           (10 files)
│   ├── SKILL.md                ← 11 状态 UI 状态机 + 框架检测逻辑
│   └── templates/
│       ├── RealDownloadService.cs
│       ├── DownloadViewModels.cs
│       ├── SemiUrsaClientView.axaml / SemiUrsaUpgradeView.axaml
│       ├── LayUIStyle.xaml / WPFDevelopersStyle.xaml
│       ├── AntdUIStyle.cs
│       └── MauiUpdatePage.xaml/.cs
│
├── generalupdate-strategy/     (7 files)
│   ├── SKILL.md                ← 决策树 + 6 策略详解 + 混合 + 平台对照
│   └── examples/
│       ├── ClientServerStrategy.cs / OssStrategy.cs
│       ├── SilentStrategy.cs / DifferentialStrategy.cs
│       └── CrossVersionStrategy.cs / PushStrategy.cs
│
├── generalupdate-advanced/     (6 files)
│   ├── SKILL.md                ← 10+ 扩展点 + 4 IPC + Bowl + 事件系统
│   ├── reference.md
│   └── templates/
│       ├── CustomHooks.cs / CustomStrategy.cs
│       ├── BowlIntegration.cs / NamedPipeIPC.cs
│
└── generalupdate-troubleshoot/ (2 files)
    ├── SKILL.md                ← 诊断工作流
    └── reference.md            ← 50+ 症状清单（C/H/M/L 四级）
```

---

## API 兼容性说明

> ⚠️ **NuGet 引用规则**:
> - Core only: `dotnet add package GeneralUpdate.Core`
> - With Bowl: reference **only** `GeneralUpdate.Bowl`（传递依赖 Core，两者不能共存）
> - Differential 已嵌入 Core，**无需**额外引用 `GeneralUpdate.Differential`

> ⚠️ **API Surface**: v10.4.6 稳定版 API 与开发分支（v10.5.0-beta.2）有根本性差异。当前稳定版不支持：
> - ❌ 无可编程 `Option` 配置系统（仅 `Configinfo` 属性）
> - ❌ 无 `IUpdateHooks` 生命周期钩子
> - ❌ 无 `IStrategy` 可替换策略接口
> - ❌ 无 `SilentPollOrchestrator`
> - ❌ 无 `ProcessContract` / IPC 替换接口

---

## 反馈与建议

使用过程中遇到任何问题或有改进建议，欢迎提交 Issue：

- **GitHub Issues**: [GeneralUpdate-Samples/issues](https://github.com/GeneralLibrary/GeneralUpdate-Samples/issues) — 报告 Bug、提出功能需求
- **GeneralUpdate Issues**: [GeneralUpdate/issues](https://github.com/GeneralLibrary/GeneralUpdate/issues) — 核心库 Bug 和功能请求

提交时请附上以下信息以便快速排查：
- GeneralUpdate 版本号（如 v10.4.6）
- 平台（Windows / Linux / macOS）
- 更新策略（标准 / OSS / 静默 / 差分 / 推送）
- 完整错误日志（可从 `%TEMP%/GeneralUpdate/logs/` 获取）

---

## 版本历史

### 0.0.1-bate.1 — 2026-06-16

Initial beta release. All templates rewritten for NuGet v10.4.6 stable API.

## 相关项目

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET 自动更新核心库
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — 更新服务端
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — 示例项目合集
