---
sidebar_position: 1
sidebar_label: 📋 总览
title: 🚀 Agent Skills 总览
---

# 🚀 GeneralUpdate Agent Skills 套件

**Claude Code 技能套件** — 帮助 .NET 开发者在 5 分钟内将 [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) 自动更新系统集成到任意 .NET 应用中。

覆盖 50+ 真实 Issue 发现的已知问题，提供即用型代码生成 + 深度故障排查。

> **Current Version: 0.0.2-beta.1** — targets NuGet `GeneralUpdate.Core 10.5.0-beta.4`  
> 兼容性：`v10.5.0-beta.4`（NuGet 最新预览版）  
> 所有模板已通过 `dotnet build` 编译验证（0 errors）。

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
            ├── generalupdate-troubleshoot/   ← 🩺 故障排查
            ├── generalupdate-migration/      ← 🔄 迁移指南
            └── generalupdate-security-audit/ ← 🔒 安全审计
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

## 🧭 开发者集成路线图

**你是哪种情况？找到你的入口，按步骤推进：**

| 你的场景 | 从哪开始 | 做什么 | 完成后下一步 |
|---------|---------|-------|-------------|
| 🆕 **第一次加更新，从零开始** | `/generalupdate-init` | ① 选集成模式 → ② 生成 Bootstrap → ③ 部署 | `/generalupdate-ui`（加界面） |
| 🎨 **已有集成，需要更新界面** | `/generalupdate-ui` | ① 自动检测框架 → ② 生成窗口 → ③ 桥接事件 | `/generalupdate-strategy`（选策略） |
| ⚙️ **要选更新策略（OSS/静默/差分）** | `/generalupdate-strategy` | ① 决策树选策略 → ② 配置服务端 → ③ 示例代码 | `/generalupdate-init`（配置 Bootstrap） |
| 🔧 **需要高级定制（Bowl/IPC/Hooks）** | `/generalupdate-advanced` | ① 选扩展点 → ② 生成模板代码 → ③ 集成 | 部署验证 |
| 🩺 **更新失败/报错/异常** | `/generalupdate-troubleshoot` | ① 症状收集 → ② 匹配已知问题 → ③ 修复 | 回到对应 skill 改配置 |
| 🔄 **已有 v9.x 要迁移到 v10** | `/generalupdate-migration` | 参考迁移路径 + API 对照表 | `/generalupdate-troubleshoot`（检查迁移问题） |
| 🔒 **需要安全审计** | `/generalupdate-security-audit` | 14 项安全检查 + 审计报告 | `/generalupdate-init`（修复发现的问题） |

---

## Skills 总览

| Skill | 命令 | 描述 | 覆盖范围 |
|-------|------|------|----------|
| 🚀 **generalupdate-init** | `/generalupdate-init` | 双项目脚手架 + Bootstrap 配置（4 种方式） | 4 大场景 + 4 种配置方式 + 完整 API |
| 🎨 **generalupdate-ui** | `/generalupdate-ui` | 自动识别 UI 框架，生成全状态更新窗口（11 种状态） | 6 UI 框架 + 全状态机 + 桥接代码 |
| ⚙️ **generalupdate-strategy** | `/generalupdate-strategy` | 6 种策略决策树 + 混合组合 + 平台差异 | 6 策略 + 4 组合 + 平台对照 |
| 🔧 **generalupdate-advanced** | `/generalupdate-advanced` | 10+ 扩展点 + 4 种 IPC + Bowl + AOT | 10+ 扩展点 + 完整架构图 |
| 🩺 **generalupdate-troubleshoot** | `/generalupdate-troubleshoot` | 50+ 已知问题诊断 + BM25 搜索引擎 | 8 致命 + 11 高 + 20 中 + 12 低 |
| 🔄 **generalupdate-migration** | `/generalupdate-migration` | v9.x → v10 / dev-branch → stable 迁移 | 2 条迁移路径 + API 对照表 |
| 🔒 **generalupdate-security-audit** | `/generalupdate-security-audit` | 安全审计 + 修复建议 | 14 项安全矩阵 + 审计报告模板 |

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

"把 v9.x 的项目迁移到 v10"
→ 自动激活 generalupdate-migration
```

---

## 通用集成验证清单

无论使用哪个 skill，完成集成后请逐项检查：

### Bootstrap 配置
- [ ] `UpdateRequest` 的 6 个必填字段都已设置（UpdateUrl, AppSecretKey, MainAppName, ClientVersion, ProductId, InstallPath）
- [ ] `UpdateUrl` 指向的服务端 API 可正常返回版本信息
- [ ] `AppSecretKey` 长度 ≥ 16 字符，与服务端一致
- [ ] `InstallPath` 指向正确的安装目录（生产环境用 `AppDomain.CurrentDomain.BaseDirectory`）
- [ ] `AppType` 设置正确（Client = 1, Upgrade = 2）

### NuGet & 编译
- [ ] Client 和 Upgrade 项目使用**完全相同**的 GeneralUpdate NuGet 版本
- [ ] 项目能正常 `dotnet build`（0 errors）

### 部署结构
- [ ] UpgradeApp.exe 存在于发布目录（首个版本就必须有）
- [ ] `generalupdate.manifest.json` 的 `UpdateAppName` 包含 `.exe`
- [ ] IPC 文件（`UpdateInfo.msg`）路径在 Client/Upgrade 间一致
- [ ] `Encoding` 设置为 `Encoding.UTF8`（防止 Linux/macOS 中文乱码）

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
├── generalupdate-troubleshoot/ (5+ files)
│   ├── SKILL.md                ← 诊断工作流
│   ├── reference.md            ← 50+ 症状清单（C/H/M/L 四级）
│   ├── scripts/search.py       ← BM25 搜索引擎
│   ├── scripts/core.py         ← BM25 算法核心
│   └── data/issues.csv         ← 51 条已知问题数据库
│
├── generalupdate-migration/    (1 file)
│   └── SKILL.md                ← v9.x→v10 / dev-branch→stable 迁移
│
└── generalupdate-security-audit/ (1 file)
    └── SKILL.md                ← 14 项安全审计矩阵
```

---

## API 兼容性说明

> ⚠️ **NuGet 引用规则（v10.5.0-beta.4）**:
> - Core only: `dotnet add package GeneralUpdate.Core --version 10.5.0-beta.4`
> - With Bowl: 同时引用 `GeneralUpdate.Core` 和 `GeneralUpdate.Bowl`（v10.5.0-beta.4 中无类型冲突）
> - Differential 已嵌入 Core，**无需**额外引用 `GeneralUpdate.Differential`

> ⚠️ **API Surface**: v10.5.0-beta.4 采用了全新的配置系统：
> - ✅ `UpdateRequest` / `UpdateRequestBuilder` — 替代旧的 Configinfo
> - ✅ `SetSource(updateUrl, appSecretKey)` — 零配置入口
> - ✅ `SetOption<T>(Option<T>, T)` — 可编程配置系统
> - ✅ `IUpdateHooks` — 生命周期钩子（`Hooks<T>()`）
> - ✅ `IStrategy` — 可替换策略接口（`Strategy<T>()`）
> - ✅ `UseDiffPipeline(Action<DiffPipelineBuilder>)` — 差分管道配置
> - ✅ `SilentPollOrchestrator` — 静默轮询
> - ✅ `AddListenerProgress` — 第 7 个事件监听器
> - ❌ `Configinfo` 类已被移除

---

## 反馈与建议

使用过程中遇到任何问题或有改进建议，欢迎提交 Issue：

- **GitHub Issues**: [GeneralUpdate-Samples/issues](https://github.com/GeneralLibrary/GeneralUpdate-Samples/issues) — 报告 Bug、提出功能需求
- **GeneralUpdate Issues**: [GeneralUpdate/issues](https://github.com/GeneralLibrary/GeneralUpdate/issues) — 核心库 Bug 和功能请求

提交时请附上以下信息以便快速排查：
- GeneralUpdate 版本号（如 v10.5.0-beta.4）
- 平台（Windows / Linux / macOS）
- 更新策略（标准 / OSS / 静默 / 差分 / 推送）
- 完整错误日志（可从 `Logs/generalupdate-trace-*.log` 获取）

---

## 版本历史

### 0.0.2-beta.1 — 2026-06-16

Updated for GeneralUpdate v10.5.0-beta.4 API:
- Configinfo → UpdateRequest (namespace: `GeneralUpdate.Core.Configuration`)
- Event args moved to `GeneralUpdate.Core.Download` and `GeneralUpdate.Core.Event`
- Added SetSource(), SetOption(), Hooks<T>(), Strategy<T>() API coverage
- Updated all strategy examples to use the new API
- Updated CustomHooks.cs and CustomStrategy.cs to show v10.5 capabilities
- Fixed IsComplated → IsCompleted
- NuGet version bumped to `10.5.0-beta.4`
- Added 2 new skills: generalupdate-migration, generalupdate-security-audit

### 0.0.1-beta.1 — 2026-06-16

Initial beta release. All templates rewritten for NuGet v10.4.6 stable API.

## 相关项目

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET 自动更新核心库
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — 更新服务端
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — 示例项目合集
