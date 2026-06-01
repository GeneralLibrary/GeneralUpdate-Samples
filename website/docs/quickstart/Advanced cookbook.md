---
sidebar_position: 4
title: 高级实战手册
---

# GeneralUpdate 高级实战手册

> **目标读者**：已有基础使用经验的开发者，正在将 GeneralUpdate 集成到生产产品、CI/CD 流水线和企业级发布工作流中。

本手册与[入门实战手册](./Beginner cookbook.md)互补。入门手册带你跑通第一次更新闭环；本手册假设你已经跑通，直接进入生产级方案设计。

---

## 1. 生产更新架构设计

### 1.1 拓扑总览

```
┌──────────┐  GET version    ┌──────────┐
│  Client  │ ──────────────> │  Server  │
│ (主程序)  │ <── version.json │ (更新服务) │
└────┬─────┘                 └────┬─────┘
     │                            │
     │ 下载补丁 ZIP                │ 读取 packages/
     ▼                            ▼
┌──────────┐  启动并传参    ┌──────────┐
│ Upgrade  │ <──────────── │  Client  │
│ (独立进程) │               │          │
└────┬─────┘               └──────────┘
     │ 应用补丁、上报状态
     ▼
┌──────────┐
│  新版本   │
│  主程序   │
└──────────┘
```

### 1.2 各角色职责

| 角色 | 部署位置 | 职责 | 关键配置 |
|------|----------|------|----------|
| Client | 用户终端 | 版本检查、补丁下载、拉起 Upgrade | `UpdateUrl`、`AppSecretKey`、`ClientVersion` |
| Upgrade | 与 Client 同目录 | 独立升级进程，接收上下文并应用补丁 | 由 Client 通过 `GeneralUpdateBootstrap` 传递 |
| Server | 服务器 | 版本验证、补丁下载、状态上报 | `versions.json`、`/Upgrade/Verification`、`/File/Download` |
| Tools | 开发/CI 环境 | 生成补丁包、OSS 清单、Hash 计算 | 输入 old/new 目录，输出 `.zip` |
| Bowl | 与 Client 同目录 | 进程监控、崩溃检测、备份恢复 | `MonitorParameter`、dump 路径 |

### 1.3 最小生产拓扑

对于大多数产品，推荐以下拓扑：

- **开发端**：CI 环境运行 `generate_packages.ps1` + Tools 生成补丁
- **服务端**：一台 HTTP Server 托管 `versions.json` 和 `.zip` 文件；可选 OSS 模式用静态存储
- **客户端**：Client 引用 `GeneralUpdate.ClientCore`，Upgrade 引用 `GeneralUpdate.Core`

```text
CI (GitHub Actions)                     Server (HTTP / OSS)
┌─────────────────┐                    ┌──────────────────┐
│ build + publish │ ── 推送补丁 ZIP ──> │ versions.json    │
│ gen_packages.ps1│                    │ *.zip (补丁包)    │
│ Tools (可选)     │                    │ 静态文件服务       │
└─────────────────┘                    └──────────────────┘
```

---

## 2. 标准化发布物

### 2.1 发布物类型

| 类型 | 生成方式 | 文件格式 | 适用场景 |
|------|----------|----------|----------|
| 全量包 (Full) | `ZipFile.CreateFromDirectory` 打包完整发布目录 | `packet_{timestamp}_{version}.zip` | 跨多个版本升级、首次安装 |
| 差分包 (Patch) | Tools Patch 页面或 `DifferentialCore.Clean` 对比 old/new | `patch_{timestamp}.zip` | 仅变更少量文件时减少下载量 |
| 扩展包 (Extension) | Tools Extension 页面 | `.zip`（含 manifest） | 插件/扩展独立分发与更新 |
| OSS 清单 | Tools OSS 页面 | `version.json` | OSS 模式更新，适合静态文件服务 |

### 2.2 `versions.json` 字段说明

Samples Server 的 `versions.json` 每条记录控制一次版本匹配与下载：

```json
{
  "PacketName": "packet_20260529221936_client_2.0.0.0",
  "Hash": "a1b2c3...sha256",
  "Version": "2.0.0.0",
  "Url": "http://localhost:5000/File/Download/a1b2c3...",
  "PubTime": "2026-05-29T14:19:36.0000000",
  "AppType": 1,
  "Platform": 1,
  "ProductId": "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
  "IsForcibly": false,
  "Format": ".zip",
  "Size": 123456,
  "IsFreeze": false,
  "IsCrossVersion": true,
  "FromVersion": "1.0.0.0",
  "ToVersion": "2.0.0.0"
}
```

| 字段 | 说明 |
|------|------|
| `AppType` | `1` = Client（主程序），`2` = Upgrade（升级程序） |
| `Platform` | `1` = Windows，`2` = Linux，`3` = Mac |
| `IsCrossVersion` | `true` = 差分更新包（需要 `FromVersion`/`ToVersion`），`false` = 全量包 |
| `IsForcibly` | `true` = 强制更新，用户无法跳过 |
| `IsFreeze` | `true` = 冻结当前版本，Server 返回已是最新 |
| `ProductId` | 产品标识，与 Client 配置中的 `ProductId` 对应 |

### 2.3 生成脚本的使用

Samples 提供了完整的 PowerShell 打包脚本：

```powershell
# 生成全量包和差分包
.\src\Server\generate_packages.ps1

# 仅生成全量包（快速模式，跳过 Differential）
.\src\Server\generate_packages.ps1 -FullOnly

# 单独重建 versions.json
.\src\Server\create_versions_json.ps1
```

脚本流程：
1. 扫描 `src/content_client/` 和 `src/content_upgrade/` 中的示例内容
2. 为每个版本目录生成全量 ZIP
3. 调用 `PatchGenerator` C# 项目生成差分 ZIP
4. 计算每个 ZIP 的 SHA256 并写入 `versions.json`

**生产环境建议**：将这些脚本改造为 CI 步骤，输入参数接受旧版本路径和新版本路径，输出到制品仓库。

---

## 3. CI/CD 集成

### 3.1 GitHub Actions 示例

```yaml
name: Generate Update Packages

on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  generate-packages:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Build release
        run: dotnet publish src/Client/ClientSample.csproj -c Release -o publish/v${{ github.ref_name }}

      - name: Generate packages
        shell: pwsh
        run: |
          $oldVersion = "publish/v1.0.0"  # 从制品仓库下载上一个版本
          $newVersion = "publish/v${{ github.ref_name }}"
          dotnet run --project src/PatchGenerator -- $oldVersion $newVersion packages/

      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: update-packages
          path: packages/
```

### 3.2 版本映射策略

**关键问题**：CI 不能假设所有历史版本都在构建机器上。

**推荐方案**：

| 方案 | 描述 | 适用场景 |
|------|------|----------|
| 制品仓库缓存 | 每次发布后将 `publish/v{version}` 上传到制品仓库（如 GitHub Releases、Artifactory），CI 运行时下载需要对比的旧版本 | 中等规模，版本数量可控 |
| 基准版本 + 回退 | 每隔 N 个版本发布一次全量包，差分包只与最近 2-3 个版本比较 | 快速迭代的产品 |
| 按需下载 | Server 端对比版本时，按需从 OSS 下载旧版本包进行差分 | 版本很多的产品 |

### 3.3 发布清单

每次版本发布前请确认：

- [ ] `versions.json` 中新增的记录 `Hash` 与上传的 ZIP 文件一致
- [ ] 差分包的 `FromVersion` 正确指向旧版本
- [ ] 全量包的 `AppType` 区分了 Client 和 Upgrade
- [ ] 旧版本 `IsFreeze` 未设置为 `true`（除非需要冻结）
- [ ] 所有 ZIP 可通过 `GET /File/Download/{hash}` 正常下载
- [ ] 测试 Client 从当前最新旧版本升级到新版本

---

## 4. 版本策略

### 4.1 版本号规范

GeneralUpdate 遵循[语义化版本](https://semver.org/lang/zh-CN/)（SemVer）规范的核心原则。在 SemVer `MAJOR.MINOR.PATCH` 的基础上，为兼容 .NET `System.Version`，扩展了第四个 `Revision` 段，形成 `Major.Minor.Build.Revision` 四段式版本号：

```text
2.0.0.0
│ │ │ └── Revision（修订号）— 对应 .NET 第四段，非 SemVer 标准
│ │ └──── Build（构建号）   — 对应 SemVer PATCH
│ └────── Minor（次版本号） — 对应 SemVer MINOR
└──────── Major（主版本号） — 对应 SemVer MAJOR
```

版本号规则：
- **Major**（主版本号）：不兼容的 API 修改时递增
- **Minor**（次版本号）：向下兼容的功能新增时递增
- **Build**（构建号/补丁号）：向下兼容的问题修正时递增
- **Revision**（修订号）：内部修订，通常为 `0`，仅在 .NET 程序集版本中区分

Server 使用 `new Version(string)` 进行比较，因此请保持纯数字格式。更多细节参见 [语义化版本 2.0.0](https://semver.org/lang/zh-CN/)。

### 4.2 更新模式

| 模式 | `UpgradeMode` | 行为 |
|------|---------------|------|
| 全量更新 | `1` (VersionChain) | 仅匹配 `IsCrossVersion = false` 的包，按版本号由低到高逐级升级 |
| 差分更新 | `2` (CrossVersion) | 仅匹配 `IsCrossVersion = true` 的包，要求 `FromVersion` 与客户端当前版本精确匹配 |

### 4.3 渐进式发布

```text
                     ┌──────────────┐
v1.0.0.0 ───────────>│ 全量 → 2.0.0.0 │  (跨版本跳跃)
                     └──────────────┘
                     ┌──────────────┐
v1.0.0.1 ───────────>│ 差分 → 2.0.0.0 │  (精确匹配 FromVersion)
                     └──────────────┘
                     ┌──────────────┐
v1.0.0.2 ───────────>│ 差分 → 2.0.0.0 │
                     └──────────────┘
```

**建议**：
- 重要版本发布同时提供全量包和差分包
- 对最近 3 个活跃版本提供差分升级路径
- 超过 3 个版本差距的用户走全量升级

### 4.4 强制更新

在 `versions.json` 中将 `IsForcibly` 设为 `true`：

```json
{ "Version": "2.0.1.0", "IsForcibly": true }
```

Client 端配合 `AddListenerUpdatePrecheck` 处理：

```csharp
// Client 端的预检回调
bootstrap.AddListenerUpdatePrecheck(async (precheckArgs) =>
{
    var forcedCount = precheckArgs.Versions.Count(v => v.IsForcibly);
    if (forcedCount > 0)
    {
        // 强制更新：不弹出"跳过"按钮
        ShowDialog("检测到重要安全更新，必须立即更新。");
        return false; // false = 不跳过，继续更新
    }
    // 可选更新：询问用户
    return await AskUserWhetherToSkip();
});
```

### 4.5 多版本顺序升级

当客户端版本落后多个版本时：

1. Server 返回所有高于当前版本的记录（`IsCrossVersion = false` 的全量包）
2. Client 按版本号升序依次下载并应用
3. 每完成一个版本的升级，Upgrade 重启 Client，Client 再次请求下一版本

**注意**：如果希望一次跳跃到最新版本，只提供一个全量包（版本号设为最新的目标版本）。

---

## 5. 差分策略

### 5.1 何时使用差分

| 场景 | 推荐方式 | 原因 |
|------|----------|------|
| 用户版本紧跟最新 | 差分更新 | 仅传输变更文件，节省带宽 |
| 用户版本落后 3+ 个版本 | 全量更新 | 累积变更可能超过全量下载 |
| 紧急安全补丁 | 差分更新 | 快速分发，减小攻击窗口 |
| 框架/运行时升级 | 全量更新 | 文件变化范围大，差分收益低 |
| 大文件更新（>100MB） | 差分更新 | 即使变更量大，也能显著减小下载 |

### 5.2 差分生成与 `delete_files.json`

Tools 或 `DifferentialCore.Clean` 生成的补丁包包含：

```
patch_20260529221936.zip
├── file1.dll.patch       ← 二进制差分文件
├── file2_new.dll         ← 新增文件（直接复制）
└── generalupdate.delete.json  ← 需要删除的文件清单
```

`generalupdate.delete.json` 格式：

```json
{
  "deleteFiles": [
    "old_plugin.dll",
    "deprecated/config.json"
  ]
}
```

Upgrade 端应用补丁时会自动处理：
1. 对 `.patch` 文件应用差分还原
2. 复制新增文件
3. 删除清单中的文件

### 5.3 黑名单配置

某些文件不应参与更新（用户数据、本地配置、日志等）：

```csharp
// 在 Client 配置中使用黑名单
new GeneralUpdateBootstrap()
    .AddBlacklist(new List<string>
    {
        "appsettings.json",    // 用户本地配置
        "logs/",               // 日志目录
        "*.user",              // 用户文件
        "cache.db"             // 本地数据库
    })
```

C# 代码示例 — 通过 `IBinaryDiffer` 扩展点介入：

```csharp
// 自定义差分策略，跳过大于 100MB 的单文件
public class LargeFileAwareStrategy : DefaultCleanStrategy
{
    public override bool ShouldDiff(string relativePath, FileInfo oldFile, FileInfo newFile)
    {
        const long maxDiffSize = 100 * 1024 * 1024;
        if (newFile.Length > maxDiffSize)
            return false; // 直接复制新文件，不生成 patch
        return base.ShouldDiff(relativePath, oldFile, newFile);
    }
}
```

### 5.4 并行处理

Differential 默认使用并行处理（`StreamingHdiffDiffer`）提升性能。对于大目录（数千个文件），建议：

- 控制并行度：`new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }`
- 对超大文件使用 bsdiff 算法（内存占用较低但速度稍慢）
- 对一般文件使用 hdiff 算法（速度快但内存占用较高）

---

## 6. Bowl 可靠性设计

### 6.1 Bowl 在生命周期中的位置

```text
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│ 更新完成  │ -> │ Bowl 启动 │ -> │ 启动主程序 │ -> │ 监控运行  │
└──────────┘    └──────────┘    └──────────┘    └────┬─────┘
                                                     │
                                              进程异常退出
                                                     │
                                                     ▼
                                              ┌──────────┐
                                              │ 导出 Dump │
                                              │ 备份旧版本 │
                                              │ 写 Fail JSON│
                                              └──────────┘
```

### 6.2 生产配置示例

```csharp
public static async Task GuardAsync(string appPath, string installPath)
{
    var lastVersion = "2.0.0.0";
    var processInfo = new MonitorParameter
    {
        ProcessNameOrId = "MyProduct.App.exe",
        DumpFileName = $"{lastVersion}_fail.dmp",
        FailFileName = $"{lastVersion}_fail.json",
        TargetPath = installPath,
        FailDirectory = Path.Combine(installPath, "fail", lastVersion),
        BackupDirectory = Path.Combine(installPath, "backup", lastVersion),
        WorkModel = "Normal"
    };

    Bowl.Launch(processInfo);
}
```

### 6.3 备份与回滚流程

1. **升级前**：Bowl 将当前运行的程序文件备份到 `BackupDirectory`
2. **升级后**：Bowl 启动新版本主程序
3. **监控中**：Bowl 持续监控进程状态
4. **异常时**：
   - 导出 `.dmp` 文件
   - 写入 `fail.json`（包含版本、时间、系统信息、驱动信息）
   - 如果 `WorkModel = "Normal"`，自动从备份目录恢复旧版本
5. **恢复后**：Bowl 重新启动旧版本程序

### 6.4 平台差异处理

| 特性 | Windows | Linux | macOS |
|------|---------|-------|-------|
| Dump 导出 | MiniDumpWriteDump | 依赖环境（需 `dotnet-dump` 或 `createdump`） | 依赖环境 |
| 进程监控 | WMI / Win32 API | procfs (`/proc/{pid}`) | 类似 Linux |
| 备份恢复 | 文件复制 + 覆盖 | 文件复制 + 覆盖（注意权限） | 文件复制 + 覆盖 |
| 驱动信息 | `driverquery` | `lsmod` / `lspci` | `kextstat` |

### 6.5 日志集成

Bowl 会自动输出以下信息到 `fail.json`：

```json
{
  "version": "2.0.0.0",
  "timestamp": "2026-05-29T14:30:00Z",
  "processName": "MyProduct.App.exe",
  "exitCode": -1073741819,
  "systemInfo": { "os": "Windows 11", "arch": "x64" },
  "driverInfo": [],
  "dumpPath": "fail/2.0.0.0/2.0.0.0_fail.dmp"
}
```

建议在产品中加入日志上传步骤，将 `fail.json` 和 `.dmp` 上报到服务端进行分析。

---

## 7. 扩展系统

### 7.1 扩展生命周期

```text
┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐
│ 查询可用   │ → │ 下载扩展   │ → │ 安装扩展   │ → │ 加载扩展   │
│ 扩展列表   │   │ 包到本地   │   │ 到目录     │   │ 到运行时   │
└──────────┘   └──────────┘   └──────────┘   └────┬─────┘
                                                   │
                                              ┌────▼─────┐
                                              │ 更新/回滚  │
                                              │ 卸载扩展   │
                                              └──────────┘
```

### 7.2 扩展 Manifest 规范

每个扩展包应包含 `extension.json`：

```json
{
  "id": "com.mycompany.plugin.search",
  "name": "Search Plugin",
  "version": "1.2.0",
  "author": "MyCompany",
  "description": "Full-text search capability",
  "dependencies": {
    "com.mycompany.core": ">=1.0.0"
  },
  "platforms": ["win-x64", "linux-x64"],
  "minAppVersion": "2.0.0",
  "type": "plugin"
}
```

### 7.3 查询与安装

```csharp
var manager = new ExtensionManager(options =>
{
    options.ExtensionsRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions");
    options.TempDownloadPath = Path.Combine(Path.GetTempPath(), "ext_downloads");
    options.ManifestFileName = "extension.json";
});

// 查询远程可用扩展
var available = await manager.QueryRemoteExtensionsAsync("https://extensions.example.com/api/v1/extensions");

// 安装
await manager.InstallAsync("com.mycompany.plugin.search", "1.2.0",
    progress => Console.WriteLine($"Downloading: {progress.Percentage}%"));

// 更新
await manager.UpdateAsync("com.mycompany.plugin.search", "1.3.0");

// 回滚
await manager.RollbackAsync("com.mycompany.plugin.search", "1.2.0");

// 卸载
await manager.UninstallAsync("com.mycompany.plugin.search");
```

### 7.4 Tools 扩展包生成

使用 Tools 的 Extension Package 页面：

| 输入 | 说明 |
|------|------|
| Extension Source Directory | 扩展文件的源目录 |
| Manifest File | `extension.json` 文件路径 |
| Output Directory | 扩展包 ZIP 输出目录 |

生成的扩展包 ZIP 包含 manifest 和所有扩展文件，可直接推送到扩展服务器。

### 7.5 服务端 API 约定

扩展服务器需要提供以下端点：

```
GET  /api/v1/extensions                    → 返回可用扩展列表
GET  /api/v1/extensions/{id}               → 返回指定扩展详情和版本列表
GET  /api/v1/extensions/{id}/versions/{v}  → 返回指定版本元数据和下载链接
GET  /api/v1/extensions/download/{hash}    → 下载扩展包
POST /api/v1/extensions/check              → 批量兼容性检查
```

---

## 8. 驱动更新

### 8.1 驱动更新与普通文件更新的区别

| 维度 | 应用文件更新 | 驱动更新 |
|------|-------------|----------|
| 文件位置 | 应用程序目录 | 系统驱动目录 |
| 权限要求 | 常规用户权限 | 管理员/root 权限 |
| 生效方式 | 重启应用程序 | 可能需要重启系统 |
| 签名要求 | 无 | 需要数字签名（Windows WHQL / Linux DKMS） |
| 回滚复杂度 | 直接替换文件 | 需卸载当前驱动，安装旧版本 |
| 兼容性检查 | 框架版本 | 内核版本、硬件 ID |

### 8.2 Drivelution 更新流程

```csharp
var updater = new DriverUpdater(options =>
{
    options.DriverStorePath = @"C:\ProgramData\MyProduct\Drivers";
    options.TempPath = Path.GetTempPath();
    options.RequireSignature = true;
    options.BackupBeforeInstall = true;
});

// 1. 验证驱动包
var validation = await updater.ValidatePackageAsync(driverPackagePath);
if (!validation.IsValid)
{
    Console.WriteLine($"Driver validation failed: {validation.Errors}");
    return;
}

// 2. 备份当前驱动
await updater.BackupCurrentDriverAsync("MyDeviceDriver");

// 3. 安装新驱动
var result = await updater.InstallAsync(driverPackagePath);
if (!result.Success)
{
    // 4. 安装失败，自动回滚
    await updater.RollbackAsync("MyDeviceDriver");
    Console.WriteLine($"Install failed: {result.ErrorMessage}");
    return;
}

// 5. 验证新驱动是否正常工作
var health = await updater.HealthCheckAsync("MyDeviceDriver");
Console.WriteLine($"Driver health: {health.Status}");
```

### 8.3 Windows 驱动签名

生产环境中，Windows 驱动必须签名：

1. 获取 EV Code Signing 证书
2. 通过 WHQL（Windows Hardware Quality Labs）认证
3. 在 `DriverUpdater` 中启用 `RequireSignature = true`

### 8.4 Linux 驱动注意事项

- 使用 `dkms`（Dynamic Kernel Module Support）管理内核模块
- 更新前检查内核版本兼容性
- `/lib/modules/$(uname -r)/` 路径通常需要 root 权限
- 更新后可能需要 `depmod` 和 `modprobe`

### 8.5 批量/并行驱动更新

```csharp
var updaters = new[]
{
    new DriverUpdateTask { DeviceId = "PCI\\VEN_8086", PackagePath = @"drivers\gpu_2.1.0.zip" },
    new DriverUpdateTask { DeviceId = "USB\\VID_0BDA", PackagePath = @"drivers\wifi_1.5.0.zip" },
};

// 并行更新多个驱动
var results = await Parallel.ForEachAsync(updaters,
    new ParallelOptions { MaxDegreeOfParallelism = 4 },
    async (task, ct) =>
    {
        var updater = new DriverUpdater(/* ... */);
        return await updater.InstallAsync(task.PackagePath);
    });

foreach (var r in results)
{
    Console.WriteLine($"Driver {r.DeviceId}: {(r.Success ? "OK" : "FAILED")}");
}
```

---

## 9. 安全与合规

### 9.1 Hash 校验

GeneralUpdate 在下载完成后自动校验 SHA256：

```csharp
// Server 端：生成 Hash（PowerShell）
(Get-FileHash -Path .\packet.zip -Algorithm SHA256).Hash.ToLowerInvariant()

// 或使用 Tools OSS 页面计算
```

Client 端自动校验：下载完成后，Core 会对下载的文件计算 SHA256 并与 `versions.json` 中的 `Hash` 进行比对。不匹配则丢弃并重试（最多 3 次）。

### 9.2 认证下载

Server 支持在下载请求中传递 Token：

```csharp
// Client 端：在请求中添加认证头
new GeneralUpdateBootstrap()
    .AddListenerDownloadConfig(config =>
    {
        config.Headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {GetAccessToken()}"
        };
    });
```

**生产环境建议**：
- 使用短期有效的 JWT Token
- 下载端点验证 Token 作用域（仅允许下载指定 `ProductId` 的包）
- 记录每次下载的 Token、IP、时间到审计日志

### 9.3 OSS 模式安全

OSS 模式使用静态文件服务（如阿里云 OSS、AWS S3）：

```
# 推荐的 OSS Bucket 策略
{
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {"Anonymous": ["*"]},
      "Action": ["oss:GetObject"],
      "Resource": ["acs:oss:my-bucket:packages/*.zip"],
      "Condition": {
        "IpAddress": {"acs:SourceIp": ["你的服务器 IP 段"]}
      }
    },
    {
      "Effect": "Allow",
      "Principal": {"Anonymous": ["*"]},
      "Action": ["oss:GetObject"],
      "Resource": ["acs:oss:my-bucket:packages/versions.json"]
    }
  ]
}
```

### 9.4 发布审计清单

每次生产发布前请确认：

- [ ] 所有 `versions.json` 中的 `Hash` 与上传 ZIP 的 SHA256 一致
- [ ] 下载端点使用 HTTPS（生产环境禁止 HTTP）
- [ ] `AppSecretKey` 正确配置，防止未授权的版本查询
- [ ] 旧版本的 `IsFreeze` 标记正确（不应冻结仍在使用中的版本）
- [ ] 差分包的 `FromVersion`/`ToVersion` 路径正确
- [ ] 所有文件由 CI 签名，未被篡改
- [ ] 生产发布记录已写入审计日志

---

## 10. 故障演练

### 10.1 场景一：下载失败 / 网络中断

**模拟方法**：在 Client 下载过程中拔掉网线或关闭 Server。

**预期行为**：
- Client 抛出 `DownloadException`
- 断点续传功能自动生效：下次启动继续下载
- 下载进度通过 `AddListenerMultiDownloadStatistics` 回调报告

**验证方式**：
```csharp
bootstrap.AddListenerMultiDownloadError((version, exception) =>
{
    Console.WriteLine($"[Download Error] {version.Version}: {exception.Message}");
    // 产品集成：告警上报
});
```

### 10.2 场景二：文件锁定

**模拟方法**：在 Upgrade 应用补丁前，手动打开目标目录中的某个 DLL（如用记事本打开）。

**预期行为**：
- Upgrade 尝试替换文件时遇到 `IOException`
- 重试 3 次（每次间隔 500ms）
- 仍失败则触发 `AddListenerException` 回调
- 不破坏已更新的其他文件

### 10.3 场景三：升级后启动崩溃

**模拟方法**：将 `content_upgrade` 中的 `UpgradeSample.exe` 替换为一个会立即崩溃的假 exe。

**预期行为**：
- Client 完成更新后启动新版本
- Bowl 检测到进程异常退出
- Bowl 导出 `.dmp` 和 `fail.json` 到 `fail/{version}/`
- Bowl 从 `backup/{version}/` 恢复旧版本
- 旧版本启动成功

**验证方式**：
1. 检查 `fail/` 目录中是否出现了 dump 和 JSON 文件
2. 确认旧版本程序成功恢复并启动
3. Server 收到 `/Upgrade/Report` 中的失败状态

### 10.4 场景四：补丁包损坏

**模拟方法**：手动修改 `packages/` 中的补丁 ZIP 文件（例如用文本编辑器打开并修改若干字节）。

**预期行为**：
- Client 下载完成后校验 SHA256 失败
- 自动丢弃损坏文件并重新下载（最多 3 次）
- 3 次均失败后通过 `AddListenerMultiDownloadError` 通知
- 不会应用损坏的补丁

### 10.5 场景五：权限不足

**模拟方法**：在 Linux 上以非 root 用户运行 Upgrade，目标目录的写权限被移除。

**预期行为**：
- Upgrade 尝试写入文件时遇到 `UnauthorizedAccessException`
- 通过 `AddListenerException` 通知
- 不会部分更新（保持原子性）

**预防措施**：
```csharp
// Client 在启动 Upgrade 前验证目标目录权限
if (!HasWritePermission(installDirectory))
{
    // 提示用户以管理员权限运行
    RequestAdminPrivilege();
    return;
}
```

### 10.6 故障演练时间表

建议每个发布周期执行一次完整的故障演练：

| 演练项 | 频率 | 负责人 | 验证方式 |
|--------|------|--------|----------|
| 下载中断恢复 | 每次发布 | QA | 观察断点续传日志 |
| 文件锁定处理 | 每次发布 | QA | 检查回滚行为 |
| 启动崩溃恢复 | 每月 | DevOps | Bowl fail 目录 + Server 报告 |
| 补丁损坏校验 | 每次发布 | QA | 下载日志 SHA256 不匹配 |
| 权限不足处理 | 每次大版本 | DevOps | Linux 平台测试 |
| 差分包完整性 | 每次发布 | CI | 自动化测试对比文件树 |

---

## 附录：相关资源

- [GeneralUpdate.Core 组件文档](../doc/GeneralUpdate.Core.md)
- [GeneralUpdate.Differential 组件文档](../doc/GeneralUpdate.Differential.md)
- [GeneralUpdate.Bowl 组件文档](../doc/GeneralUpdate.Bowl.md)
- [GeneralUpdate.Drivelution 组件文档](../doc/GeneralUpdate.Drivelution.md)
- [GeneralUpdate.Extension 组件文档](../doc/GeneralUpdate.Extension.md)
- [GeneralUpdate.Tools 工具文档](../doc/GeneralUpdate.PacketTool.md)
- [入门实战手册](./Beginner cookbook.md)
- [Samples 仓库](https://github.com/GeneralLibrary/GeneralUpdate-Samples)
- [Tools 仓库](https://github.com/GeneralLibrary/GeneralUpdate.Tools)

> **贡献指南**：如果你在生产环境中积累了实战经验，欢迎通过 [Issue](https://github.com/GeneralLibrary/GeneralUpdate-Samples/issues) 或 Pull Request 贡献案例与最佳实践。
