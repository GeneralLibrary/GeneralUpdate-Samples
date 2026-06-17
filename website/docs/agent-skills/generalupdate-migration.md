---
sidebar_position: 7
sidebar_label: 🔄 迁移指南
title: 🔄 generalupdate-migration — 迁移指南
---

# 🔄 GeneralUpdate 迁移指南

帮助开发者从旧版本 GeneralUpdate 迁移到最新稳定版 API（v10.5.0-beta.4）。

> ⚠️ **目标版本：NuGet v10.5.0-beta.4**
> 开发分支（v10.5.0-beta.2）API 与稳定版有根本性差异。

---

## 📋 迁移前需求提取

```
### 当前状态
- 当前 GeneralUpdate 版本: ______（v9.x / v10.0-10.3 / v10.5.0-beta.x / 不确定）
- 当前 .NET 版本: ______
- UI 框架: ______
- 是否使用了 Bowl: ______（是/否）
- 是否使用了 Differential: ______（是/否）

### 迁移后目标
- 目标版本: ______（v10.5.0-beta.4 / 继续用开发分支）
- 是否需要新的功能（Bowl/IPC 替换/AOT）: ______
```

---

## 迁移路径

### 路径 A：v9.x → v10.5.0-beta.4

这是最大的跳跃。v9.x 和 v10 的架构完全不同。

```
v9.x (单进程, HttpClient 直连)
         ↓
    Breaking Changes:
    ├── 单进程 → 双进程架构（Client + Upgrade）
    ├── HttpClient 直连 → GeneralSpacestation 服务端
    ├── 无 IPC → AES 加密 IPC 文件
    ├── 无 manifest.json → 必须携带 manifest
    └── API 命名空间全部重命名
         ↓
v10.5.0-beta.4 (双进程, UpdateRequest + Bootstrap)
```

**迁移步骤：**

```csharp
// ❌ v9.x 写法（不复存在）
// var updater = new GeneralUpdater("https://api/method");
// updater.Start();

// ✅ v10.5.0-beta.4 写法
await new GeneralUpdateBootstrap()
    .SetConfig(new UpdateRequest
    {
        UpdateUrl = "https://your-server.com/Upgrade/Verification",
        AppSecretKey = "your-secret-key",
        MainAppName = "MyApp.exe",
        ClientVersion = "1.0.0.0",
        ProductId = "my-product-001",
        InstallPath = "."
    })
    .LaunchAsync();
```

| v9.x API | v10.5.0-beta.4 对应 | 说明 |
|----------|---------------------|------|
| `GeneralUpdater` | `GeneralUpdateBootstrap` | 完全重命名 |
| `SetApiUrl()` / `SetMethod()` | `UpdateRequest.UpdateUrl` | 统一到 UpdateRequest |
| `CheckUpdateAsync()` | `.LaunchAsync()` | 异步改为返回 Bootstrap 实例 |
| 单进程直接更新 | Client + Upgrade 双进程 | 必须创建独立 Upgrade 项目 |
| N/A | `generalupdate.manifest.json` | 必须随首发版本发布 |

### 路径 B：v10.5.0-beta.x (开发分支) → v10.5.0-beta.4

| 开发分支 API (v10.5.0-beta.x) | 稳定版替代 (v10.5.0-beta.4) | 处理方式 |
|-------------------------------|---------------------------|---------|
| `new Option()` / `SetOption()` | 存在且兼容 | 直接使用，命名空间有调整 |
| `.Hooks<T>()` / `IUpdateHooks` | 存在且兼容 | 直接使用 |
| `.Strategy<T>()` / `IStrategy` | 存在且兼容 | 直接使用 |
| `SilentPollOrchestrator` | 存在 | `Option.Silent` + `SetOption()` |
| `ISslValidationPolicy` | 存在 | 直接使用 |
| `Configinfo` | 已移除 | 改用 `UpdateRequest` |
| `OssClient (AppType=3,4)` | 存在 | 使用 `AppType.OssClient` 和 `AppType.OssUpgrade` |
| 硬编码版本号 | `UpdateRequest.ClientVersion` | 建议使用 `Assembly.GetEntryAssembly()?.GetName()?.Version` |

---

## 迁移验证清单

### 编译验证
- [ ] `dotnet build` 无错误
- [ ] 无 `MissingMethodException` 的风险
- [ ] 无 `CS0433` 类型冲突（v10.5.0-beta.4 中 Core + Bowl 无冲突，可同时引用）

### 架构验证
- [ ] 项目已拆分为 Client + Upgrade 两个独立项目
- [ ] Upgrade 项目 `AppType = 2`
- [ ] Client 项目 `AppType = 1`
- [ ] `generalupdate.manifest.json` 存在且配置正确

### 运行验证
- [ ] 版本检查 API 可正常返回
- [ ] 下载后 Upgrade 进程可正常启动
- [ ] 更新完成后主程序可正常重启
- [ ] IPC 文件编码设为 `Encoding.UTF8`

---

## ⚠️ 迁移反模式

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **直接在项目中替换 NuGet 版本不修改代码** | 大量编译错误 | 先清理旧 API 引用再升级 NuGet |
| 2 | **认为 v9.x 的配置对象就是 UpdateRequest** | UpdateRequest 属性名完全不同 | 对照文档重新写 UpdateRequest |
| 3 | **试图在 v10.4.6 中使用 dev-branch 的 API** | MissingMethodException | 检查 API 可用性表 |
| 4 | **迁移后不测试 Upgrade 进程** | 主程序能更新但 Upgrade 崩溃 | 两端都要测试 |
| 5 | **保留旧的 v9.x 引用不删除** | 类型冲突 | 清空 csproj 重新添加引用 |

---

## 相关技能

- `/generalupdate-init` — 从零开始的集成指南
- `/generalupdate-troubleshoot` — 迁移后问题诊断
- `/generalupdate-security-audit` — 迁移后安全审计
