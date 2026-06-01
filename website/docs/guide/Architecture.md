---
sidebar_position: 1
---

# 系统架构

## 架构概览

GeneralUpdate 是一个跨平台 .NET 自动更新框架，采用**客户端-升级助手-服务端**三层架构。整个更新流程涉及多个独立进程和组件的协同工作。

![架构图](imgs/architecture.png)

---

## 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                        服务端 (Server)                       │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────────┐  │
│  │ 版本验证 API  │  │ 补丁包存储    │  │ UpgradeHub (推送) │  │
│  │ /Verification│  │ /patch/*.zip │  │ SignalR Hub       │  │
│  └─────────────┘  └──────────────┘  └───────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    客户端 (Client Process)                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              GeneralUpdate.ClientCore                  │   │
│  │  ┌──────────┐  ┌──────────┐  ┌────────────────────┐  │   │
│  │  │ 版本检查  │  │ 下载管理  │  │ 事件通知/预检回调   │  │   │
│  │  └──────────┘  └──────────┘  └────────────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
│                              │                               │
│              IPC (AES 加密参数传递)                            │
│                              ▼                               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  升级助手 (Upgrade Process)                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                GeneralUpdate.Core                      │   │
│  │  ┌────────────────────────────────────────────────┐  │   │
│  │  │            Pipeline (中间件管道)                  │  │   │
│  │  │  HashMiddleware → CompressMiddleware →         │  │   │
│  │  │  PatchMiddleware → DrivelutionMiddleware       │  │   │
│  │  └────────────────────────────────────────────────┘  │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────────────┐  │   │
│  │  │ 文件替换  │  │ 差分包应用│  │  驱动更新         │  │   │
│  │  └──────────┘  └──────────┘  └──────────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
│                              │                               │
│              启动主程序 + Bowl 监控                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 组件关系

### 核心组件

| 组件 | NuGet 包 | 运行位置 | 职责 |
|------|----------|----------|------|
| **GeneralUpdate.ClientCore** | `GeneralUpdate.ClientCore` | 主程序进程 | 版本检查、下载管理、启动升级助手 |
| **GeneralUpdate.Core** | `GeneralUpdate.Core` | 独立升级进程 | 文件替换、差分包应用、驱动更新 |
| **GeneralUpdate.Differential** | `GeneralUpdate.Core` (内含) | 两个进程均可 | 二进制差分算法 (BSDiff/HDiffPatch) |
| **GeneralUpdate.Bowl** | `GeneralUpdate.Bowl` | 独立监控进程 | 崩溃监控、Dump 生成、诊断信息收集 |
| **GeneralUpdate.Drivelution** | `GeneralUpdate.Drivelution` | 升级进程 | 跨平台驱动更新 |
| **GeneralUpdate.Extension** | `GeneralUpdate.Extension` | 主程序进程 | 插件/扩展管理系统 |
| **GeneralUpdate.Tools** | 独立桌面应用 | 开发工具 | 补丁包构建、模拟更新、配置生成 |

### 组件依赖关系

```
GeneralUpdate.Tools ──→ GeneralUpdate.Core (DiffPipeline)
                                 │
                    ┌────────────┼────────────┐
                    ▼            ▼            ▼
           GeneralUpdate    GeneralUpdate  GeneralUpdate
           .Differential   .Drivelution   .Extension
                    │
                    ▼
           GeneralUpdate.Core (Bootstrap, Pipeline, Download, Security)
                    │
                    ▼
           GeneralUpdate.ClientCore (Client Bootstrap, Hub Service)
                    │
                    ▼
           GeneralUpdate.Bowl (进程监控)
```

---

## 更新流程详解

### 完整更新时序

```
  客户端 (Client)          服务端 (Server)         升级助手 (Upgrade)
  ─────────────              ────────────           ─────────────────
       │                         │                        │
       │  1. POST /Verification │                        │
       │ ─────────────────────► │                        │
       │                         │                        │
       │  2. 返回版本列表        │                        │
       │ ◄───────────────────── │                        │
       │                         │                        │
       │  3. 用户确认更新        │                        │
       │  (AddListenerUpdatePrecheck)                     │
       │                         │                        │
       │  4. GET /patch/v2.0.0  │                        │
       │ ─────────────────────► │                        │
       │                         │                        │
       │  5. 下载补丁包 (.zip)  │                        │
       │ ◄───────────────────── │                        │
       │                         │                        │
       │  6. 下载完成，退出主程序│                        │
       │                         │                        │
       │  7. 启动升级助手 (IPC)  │                        │
       │ ──────────────────────────────────────────────► │
       │                         │                        │
       │                         │   8. 读取 IPC 参数     │
       │                         │   9. 验证 Hash         │
       │                         │   10. 解压补丁包       │
       │                         │   11. 应用差分补丁     │
       │                         │   12. 安装驱动(可选)   │
       │                         │   13. 替换文件         │
       │                         │                        │
       │                         │   14. 启动新版主程序   │
       │ ◄────────────────────────────────────────────── │
       │                         │                        │
       │  15. POST /Report      │                        │
       │ ─────────────────────► │                        │
       │                         │                        │
```

### 阶段说明

#### 阶段 1：版本验证 (Version Verification)

客户端向服务端发送当前版本信息，服务端返回可用的更新版本列表。

**请求：**
```json
POST /Upgrade/Verification
{
  "AppSecretKey": "your-secret-key",
  "ClientVersion": "1.0.0.0",
  "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a"
}
```

**响应：**
```json
{
  "Code": 200,
  "Body": [
    {
      "Version": "1.0.1.0",
      "Hash": "abc123...",
      "UpdateLog": "修复了若干Bug",
      "IsForcibly": false,
      "ReleaseDate": "2025-06-01"
    }
  ]
}
```

#### 阶段 2：下载更新包 (Download)

客户端根据版本列表下载更新包。支持：
- **多版本串行/并行下载**：多个版本可同时下载
- **断点续传**：下载中断后从断点继续
- **SHA256 校验**：下载完成后自动校验完整性

#### 阶段 3：IPC 参数传递

客户端将更新配置（版本列表、安装路径、文件格式、编码等）通过 AES-CBC 加密写入临时文件，升级助手通过环境变量定位并解密读取。

**IPC 数据流：**
```
Client 写入: ProcessContract → JSON 序列化 → AES-256-CBC 加密 → 临时文件
                                    ↓
Upgrade 读取: 临时文件 → AES-256-CBC 解密 → JSON 反序列化 → ProcessContract
```

#### 阶段 4：Pipeline 执行

升级助手通过中间件管道依次处理下载的更新包：

1. **HashMiddleware** — SHA256 哈希校验
2. **CompressMiddleware** — 解压 ZIP 更新包
3. **PatchMiddleware** — 应用二进制差分补丁 (可选)
4. **DrivelutionMiddleware** — 安装驱动程序 (可选)

#### 阶段 5：启动新版本

升级完成后，升级助手启动新版本的主程序。如果配置了 Bowl，会同时启动 Bowl 进程监控新程序的运行状态。

---

## 更新模式

### 全量更新模式

当 `Patch` 选项设置为 `false` 时，执行全量更新：

```
下载完整包 → 解压 → 覆盖安装
```

**适用场景：**
- 首次安装
- 版本差异过大，增量包可能不划算
- 不需要保留旧版本文件

### 增量/差分更新模式 (默认)

当 `Patch` 选项设置为 `true` 时（默认），执行增量更新：

```
下载差分包 → 解压 → 应用 BSDiff 补丁 → 删除过期文件
```

**差分包结构：**
```
patch_v1.0.1.zip
├── file1.dll.patch      ← 二进制差分补丁
├── file2.exe.patch      ← 二进制差分补丁
├── newfile.dll           ← 新增文件(原样包含)
├── delete_files.json     ← 待删除文件列表
└── ...
```

**差分包优势：**
- 补丁包体积通常仅为全量包的 5%-20%
- 下载速度快 5-20 倍
- 节省带宽和存储成本

### OSS 更新模式

基于对象存储服务（如阿里云 OSS、AWS S3、MinIO）的更新模式：

```
客户端 → 读取 version.json → 对比版本 → 下载差分包 → 升级
```

**version.json 格式：**
```json
{
  "Records": [
    {
      "PacketName": "patch_v1.0.1.zip",
      "Hash": "sha256:abc123...",
      "Version": "1.0.1.0",
      "Url": "https://oss.example.com/packages/patch_v1.0.1.zip",
      "ReleaseDate": "2025-06-01T00:00:00"
    }
  ]
}
```

OSS 模式**不需要服务端 API**，只需将 `version.json` 和补丁包上传到对象存储即可。

---

## 平台策略 (Strategy Pattern)

GeneralUpdate 通过策略模式适配不同操作系统：

| 平台 | 策略类 | 特殊处理 |
|------|--------|----------|
| Windows | `WindowsStrategy` | Bowl 集成、文件权限、PnPUtil 驱动安装 |
| Linux | `LinuxStrategy` | chmod 权限脚本、无 Bowl 支持 |
| macOS | `MacStrategy` | chmod 权限脚本、`ConfigureAwait(false)` |

每个策略类实现 `IStrategy` 接口，负责：
- `Create(UpdateContext)` — 创建策略实例
- `ExecuteAsync()` — 执行更新（模板方法）
- `StartAppAsync()` — 启动更新后的应用程序

---

## 扩展点 (Extension Points)

GeneralUpdate 通过 `AbstractBootstrap` 的 CRTP 模式提供丰富的扩展点：

### 可替换组件

| 扩展点 | 接口 | 默认实现 | 用途 |
|--------|------|----------|------|
| **Strategy** | `IStrategy` | `WindowsStrategy`/`LinuxStrategy`/`MacStrategy` | 平台策略 |
| **Hooks** | `IUpdateHooks` | `NoOpUpdateHooks` | 更新生命周期钩子 |
| **SSL Policy** | `ISslValidationPolicy` | `StrictSslValidationPolicy` | SSL 证书验证策略 |
| **Download Policy** | `IDownloadPolicy` | `DefaultDownloadPolicy` | 下载重试/超时策略 |
| **Download Executor** | `IDownloadExecutor` | `HttpDownloadExecutor` | 下载协议执行器 |
| **Download Source** | `IDownloadSource` | 默认 HTTP 源 | 下载数据源 |
| **Download Pipeline** | `IDownloadPipeline` | 默认管道 | 下载后处理 |
| **Auth Provider** | `IHttpAuthProvider` | `NoOpAuthProvider` | HTTP 认证提供者 |
| **Download Orchestrator** | `IDownloadOrchestrator` | `DefaultDownloadOrchestrator` | 批量下载调度器 |
| **Update Reporter** | `IUpdateReporter` | 默认上报器 | 更新状态上报 |

### 扩展注册示例

```csharp
await new GeneralClientBootstrap()
    // 自定义 SSL 验证策略
    .SslPolicy<CustomSslPolicy>()
    // 自定义下载重试策略
    .DownloadPolicy<CustomRetryPolicy>()
    // 自定义认证提供者
    .UpdateAuth<BearerTokenAuthProvider>()
    // 生命周期钩子
    .Hooks<CustomUpdateHooks>()
    .SetConfig(config)
    .LaunchAsync();
```

---

## 安全架构

### 多层安全机制

```
┌─────────────────────────────────────────┐
│         传输安全 (HTTPS/TLS)              │
│  ┌─────────────────────────────────────┐│
│  │       身份认证 (Token/Scheme)         ││
│  │  ┌─────────────────────────────────┐││
│  │  │    数据完整性 (SHA256 哈希)       │││
│  │  │  ┌─────────────────────────────┐│││
│  │  │  │  IPC 加密 (AES-256-CBC)      ││││
│  │  │  │  ┌─────────────────────────┐││││
│  │  │  │  │  文件校验 (签名/哈希)     │││││
│  │  │  │  └─────────────────────────┘││││
│  │  │  └─────────────────────────────┘│││
│  │  └─────────────────────────────────┘││
│  └─────────────────────────────────────┘│
└─────────────────────────────────────────┘
```

详细的安全机制请参考 [安全指南](./Security.md)。

---

## 相关资源

- **[Pipeline 详解](./Pipeline.md)** — 中间件管道工作原理
- **[配置参考](./Configuration.md)** — 完整配置选项
- **[安全指南](./Security.md)** — 安全架构详解
- **[快速入门](../quickstart/Quik%20start.md)** — 5 分钟快速上手
