---
sidebar_position: 4
sidebar_label: ⚙️ 策略指南
title: ⚙️ generalupdate-strategy — 更新策略指南
---

# ⚙️ GeneralUpdate 更新策略完全指南

> ⚠️ **针对 NuGet v10.5.0-beta.4**。该版本使用 `UpdateRequest` 配置，支持可编程 `Option` 系统。

---

## 📋 用户需求提取

```
### 部署环境
- 是否有后端服务: ______（是/否/计划中）
- 服务端类型: ______（GeneralSpacestation / 自定义 API / S3/MinIO / 无）
- 客户端数量: ______（几十/几百/几千/万+）
- 客户端是否 7×24 运行: ______（是/否）

### 更新需求
- 是否需要节省带宽: ______（是/否 → 推荐差分）
- 是否需要跳过中间版本: ______（是/否 → 推荐 CVP）
- 是否需要服务端主动触发: ______（是/否 → 推荐 SignalR）
- 是否需要用户无感知: ______（是/否 → 推荐静默）
- 是否需要显示更新进度: ______（是/否 → 推荐标准 + UI）

### 约束条件
- 目标平台: ______（Windows/Linux/macOS/多平台）
- 网络环境: ______（内网/公网/离线）
- 是否需要崩溃恢复: ______（是/否 → 配合 Bowl）
```

---

## 策略决策树（详细版）

```
你的应用有后端服务吗？
├── 有
│   ├── 需要服务端主动推送更新？
│   │   └── YES → ⑥ SignalR 推送（需额外部署 SignalR Hub）
│   └── NO
│       ├── 需要节省下载带宽？
│       │   ├── YES → ④ 差分更新（生成补丁包，减少 60-90% 体积）
│       │   └── NO
│       │       ├── 需要跳过中间版本直达最新？
│       │       │   ├── YES → ⑤ 跨版本 CVP（需服务端额外构建）
│       │       │   └── NO
│       │       │       └── ① 标准客户端-服务端（推荐新手入门）
│       └── 需要后台无声升级？
│           └── YES → ③ 静默更新（基于标准或 OSS + 定时轮询）
│
└── 没有（只有对象存储 S3/MinIO）
    ├── 需要节省带宽？
    │   ├── YES → ④ 差分更新（OSS + 差分补丁）
    │   └── NO
    │       └── ② OSS 标准（最低成本，零服务端）
    │
    └── 需要后台无声升级？
        └── YES → ③ 静默更新（OSS + 定时检查）

### 混合策略组合

常见组合方案：
| 场景 | 策略组合 | 说明 |
|------|---------|------|
| 标准 Web 应用 | ① 标准 + 🎨 UI | 有后端，显示进度 |
| 无服务端节省带宽 | ② OSS + ④ 差分 | 零服务端 + 增量更新 |
| 长期运行后台服务 | ③ 静默（基于 ① 或 ②） | 用户无感知 |
| 强制升级 | ⑤ CVP + ⑥ SignalR | 跳过旧版本，主动推送 |
| 企业级高可靠 | ① 标准 + Bowl + ③ 静默 | 完整链路 |
```

---

## 6 种策略详细对比

| 策略 | 服务端 | 说明 |
|------|:------:|------|
| **① 标准客户端-服务端** | ✅ GeneralSpacestation | 有后端的中大型应用（推荐入门） |
| **② OSS 对象存储** | ❌ 仅 S3/MinIO | 无后端，最低成本 |
| **③ 静默更新** | ✅ 同①或② | 后台无声升级 |
| **④ 差分更新** | ✅ 需差分构建 | 增量补丁节省带宽 |
| **⑤ 跨版本 CVP** | ✅ 需 CVP 构建 | 跳过中间版本直跳 |
| **⑥ SignalR 推送** | ✅ 需 SignalR Hub | 服务端主动推送 |

---

## 集成代码

所有策略使用相同的配置模式：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-secret-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = ".",
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListener*(...)
    .LaunchAsync();
```

或使用零配置 `SetSource()` API：

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://your-server.com/api",
        appSecretKey: "your-secret-key")
    .AddListenerUpdateInfo(...)
    .LaunchAsync();
```

具体示例参见 `examples/` 目录下的策略文件。

---

## 平台特定差异

| 平台 | 特性 |
|------|------|
| **Windows** | 完整功能 |
| **Linux** | 部分功能（无 Bowl） |
| **macOS** | 同 Linux |

---

## 已知问题

| # | 问题 | 规避方案 |
|---|------|---------|
| 1 | OSS 模式不区分 Main/Upgrade 更新 | 接受此行为 |
| 2 | UpgradeApp.exe 必须放在 update/ 子目录 | 按规范部署 |
| 3 | NuGet 版本冲突导致 "Method not found" | Client 和 Upgrade 使用相同版本号 |
| 4 | 无限升级循环 | 确保 manifest.json 版本号正确 |
| 5 | SignalR HubConnection Dispose 后重连崩溃 | Dispose 时将连接置 null |

---

## ✅ 策略选择验证清单

### 策略匹配度
- [ ] 选定的策略与部署环境匹配（有后端→标准/无后端→OSS）
- [ ] 带宽需求与策略匹配（大文件→差分，版本多→CVP）
- [ ] 用户体验目标与策略匹配（需要交互→标准+UI，后台→静默）
- [ ] 平台兼容性确认（Linux/macOS 不支持 Bowl）

### OSS 策略
- [ ] Bucket 权限设置为私有
- [ ] 更新包的 URL 可公开访问或使用预签名 URL
- [ ] Upgrade.exe 放在 `update/` 子目录（OSS 特有要求）
- [ ] 没有区分 Main/Upgrade 独立更新包（OSS 限制，接受）

### 静默策略
- [ ] 轮询间隔合理（建议 30-60 分钟，太短耗电/流量）
- [ ] 有"新版本可用"的系统通知或托盘图标提示
- [ ] 下载完成后再通知用户重启，而非下载前
- [ ] 后台下载有流量/电量优化（WiFi 下才下载大包）

### SignalR 推送
- [ ] HubConnection 的生命周期管理完善
- [ ] 重连逻辑（自动重试 3 次，间隔递增）
- [ ] Dispose 时将 HubConnection 置 null（否则重连崩溃）
- [ ] 推送消息有超时保护和降级策略（推送失败→回退到轮询）

### 差分策略
- [ ] 服务端有差分包生成机制（`DifferentialCore.CleanAsync`）
- [ ] 客户端 Pipeline 配置了 PatchMiddleware
- [ ] 注意大文件差分可能触发的整数溢出（v10.4.6 已修复）
- [ ] Linux/macOS 上 BSDIFF 补丁兼容性已验证

---

## ⚠️ 反模式清单

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **有后端却选 OSS** | 浪费后端服务能力，失去版本管理 | 有后端 → 标准策略 |
| 2 | **低频轮询（每天 1 次）** | 用户等很久才收到更新 | 静默模式 30-60 分钟轮询 |
| 3 | **高频轮询（每分钟 1 次）** | 浪费带宽和电池 | 静默模式建议 ≥ 30 分钟 |
| 4 | **SignalR 连接永不释放** | 内存泄漏 | 页面/应用关闭时 Dispose HubConnection |
| 5 | **差分包太大（> 2GB）** | 整数溢出导致进程崩溃 | 分多个版本发布，或用全量包 |
| 6 | **CVP 跳版本不测试中间版本 API 变更** | 客户端数据迁移失败 | 在服务端做好版本兼容测试 |
| 7 | **OSS 包名不包含版本号** | 客户端版本比较逻辑异常 | `MyApp_1.0.0.0.zip` 格式命名 |
| 8 | **静默更新后不通知用户重启** | 用户不知道新版本已下载 | 下载完成后通知 + 延迟重启选项 |

---

## 相关技能

- `/generalupdate-init` — 如果还未配置 Bootstrap
- `/generalupdate-ui` — 如果需要更新界面
- `/generalupdate-troubleshoot` — 如果遇到问题
- `/generalupdate-advanced` — 高级定制
