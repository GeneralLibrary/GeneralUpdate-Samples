---
sidebar_position: 4
---

# 安全指南

本文档详细说明 GeneralUpdate 框架的多层安全机制。

---

## 安全架构总览

GeneralUpdate 的安全体系采用**纵深防御 (Defense in Depth)** 策略，在传输、认证、数据完整性、进程间通信等层面建立多层防护。

```
┌──────────────────────────────────────────────────┐
│                   传输安全 (TLS)                    │
│  ┌──────────────────────────────────────────────┐│
│  │             身份认证 (Token/Scheme)             ││
│  │  ┌──────────────────────────────────────────┐││
│  │  │          数据完整性 (SHA256 哈希)           │││
│  │  │  ┌──────────────────────────────────────┐│││
│  │  │  │        IPC 加密 (AES-256-CBC)         ││││
│  │  │  │  ┌──────────────────────────────────┐││││
│  │  │  │  │     文件级校验 (签名/哈希)          │││││
│  │  │  │  └──────────────────────────────────┘││││
│  │  │  └──────────────────────────────────────┘│││
│  │  └──────────────────────────────────────────┘││
│  └──────────────────────────────────────────────┘│
└──────────────────────────────────────────────────┘
```

---

## 一、传输安全 (TLS/SSL)

### HTTPS 强制

GeneralUpdate 的 HTTP 客户端默认要求 HTTPS 连接。所有与服务端的通信（版本检查、下载、上报）都应通过 HTTPS 进行。

### SSL 证书验证策略

**接口：** `ISslValidationPolicy`

```csharp
public interface ISslValidationPolicy
{
    bool ValidateCertificate(
        X509Certificate2 certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors);
}
```

**默认策略：** `StrictSslValidationPolicy`

```csharp
public class StrictSslValidationPolicy : ISslValidationPolicy
{
    public bool ValidateCertificate(...)
    {
        // 任何 SSL 错误都拒绝连接
        return sslPolicyErrors == SslPolicyErrors.None;
    }
}
```

**自定义 SSL 策略：**

```csharp
// 开发环境：接受自签名证书
public class DevelopmentSslPolicy : ISslValidationPolicy
{
    public bool ValidateCertificate(
        X509Certificate2 certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // 仅在开发环境使用！
        return true;
    }
}

// 注册自定义策略
await new GeneralClientBootstrap()
    .SslPolicy<DevelopmentSslPolicy>()
    .SetConfig(config)
    .LaunchAsync();
```

> **⚠️ 警告：** 切勿在生产环境禁用 SSL 验证！

---

## 二、身份认证 (Authentication)

### HTTP 认证提供者

**接口：** `IHttpAuthProvider`

```csharp
public interface IHttpAuthProvider
{
    Task ApplyAuthAsync(HttpRequestMessage request, 
        CancellationToken cancellationToken);
}
```

### 内置认证方案

#### 1. Bearer Token 认证

```csharp
// 客户端配置
var config = new Configinfo
{
    Scheme = "Bearer",
    Token = "your-access-token",
    // ...
};

// 或者通过 AbstractBootstrap 注入
await new GeneralClientBootstrap()
    .UpdateAuth<BearerTokenAuthProvider>()
    .SetConfig(config)
    .LaunchAsync();
```

**HTTP 请求头：**
```
Authorization: Bearer your-access-token
```

#### 2. API Key 认证

```csharp
public class ApiKeyAuthProvider : IHttpAuthProvider
{
    private readonly string _apiKey;
    private readonly string _headerName; // 默认 "X-Api-Key"

    public ApiKeyAuthProvider(string apiKey, string headerName = "X-Api-Key")
    {
        _apiKey = apiKey;
        _headerName = headerName;
    }

    public Task ApplyAuthAsync(HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        request.Headers.Add(_headerName, _apiKey);
        return Task.CompletedTask;
    }
}
```

**HTTP 请求头：**
```
X-Api-Key: your-api-key
```

#### 3. HMAC 签名认证

```csharp
public class HmacAuthProvider : IHttpAuthProvider
{
    private readonly string _secretKey;

    public async Task ApplyAuthAsync(HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // 生成时间戳
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        
        // 读取请求体
        var body = request.Content != null 
            ? await request.Content.ReadAsStringAsync() 
            : string.Empty;
        
        // 计算 HMAC-SHA256 签名
        var payload = $"{body}|{timestamp}";
        var signature = ComputeHmacSha256(payload, _secretKey);
        
        // 添加请求头
        request.Headers.Add("X-Update-Timestamp", timestamp);
        request.Headers.Add("X-Update-Signature", signature);
    }

    private static string ComputeHmacSha256(string payload, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}
```

**HTTP 请求头：**
```
X-Update-Timestamp: 1717488000
X-Update-Signature: base64_hmac_sha256_signature
```

#### 4. 无认证 (默认)

```csharp
public class NoOpAuthProvider : IHttpAuthProvider
{
    public Task ApplyAuthAsync(HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

### 工厂方法

```csharp
// 根据 Scheme 自动选择认证方案
var authProvider = HttpAuthProviderFactory.Create(
    scheme: "HMAC", 
    token: null, 
    secretKey: "your-secret-key");
// → 返回 HmacAuthProvider 实例
```

| Scheme | 创建的提供者 |
|--------|-------------|
| `"Bearer"` | `BearerTokenAuthProvider` |
| `"ApiKey"` | `ApiKeyAuthProvider` |
| `"HMAC"` | `HmacAuthProvider` |
| 其他 | `NoOpAuthProvider` |

---

## 三、数据完整性 (Integrity)

### SHA256 哈希校验

每个更新包下载完成后，管道中的 `HashMiddleware` 会自动计算并比对 SHA256 哈希值。

**流程：**
```
1. 服务端返回版本信息时附带 Hash 字段
2. 客户端下载更新包后，HashMiddleware 计算实际哈希
3. 比对期望哈希 vs 实际哈希
4. 不匹配 → CryptographicException → 回滚 + 上报失败
```

**服务端哈希生成（GeneralUpdate.Tools）：**
```
使用 Tools 的 OSS Config 模块 → 选择文件 → 计算哈希 → 填充到 version.json
```

### 差分补丁完整性

差分补丁本身也内嵌了完整性检查：
- BSDiff 格式包含控制区段的 CRC 校验
- 补丁应用过程中任何数据损坏都会导致还原失败并回滚

---

## 四、IPC 进程间通信安全

ClientCore 与 Core 之间的参数传递通过 AES 加密的临时文件完成。

### 加密算法

| 参数 | 值 |
|------|-----|
| 算法 | AES-256-CBC |
| 密钥派生 | SHA256("GeneralUpdate.IPC.EnvironmentProvider.v1") |
| 密钥长度 | 256 bits |
| IV 长度 | 128 bits (CBC 模式) |
| 临时文件位置 | `%TEMP%/GeneralUpdate/ipc/` |
| 文件生命周期 | 写入 → 读取 → 自动删除 |

### 环境变量传递

```
ClientCore 写入:
  1. ProcessContract 对象 → JSON 序列化
  2. JSON → AES-256-CBC 加密
  3. 密文写入临时文件 → 路径写入环境变量

Core 读取:
  1. 从环境变量获取临时文件路径
  2. 读取密文 → AES-256-CBC 解密
  3. JSON 反序列化 → ProcessContract 对象
  4. 自动删除临时文件
```

### 安全考量

- 临时文件在 `%TEMP%` 目录中，文件名包含时间戳和 PID，难以猜测
- 读取后立即删除，窗口期极短
- AES-256-CBC 加密防止本地进程窃听
- **局限性：** 固定密钥派生方式下，同一设备上运行的其他进程理论上可以解密。这是同机进程间通信的固有局限。

### 安全增强建议

对于高安全要求的场景：
1. 使用操作系统级别的安全 IPC 机制（如命名管道 + ACL）
2. 使用 DPAPI (Windows Data Protection API) 替代固定密钥
3. 在 Linux 上使用 `shm_open` + 文件权限控制

---

## 五、备份与回滚

### 自动备份机制

更新前自动备份当前文件到备份目录：

```
{InstallPath}/
├── app-1.0.0.0/          ← 备份目录（版本号命名）
│   ├── MyApp.exe
│   ├── MyApp.Core.dll
│   └── ...
├── app-1.0.1.0/          ← 另一个版本的备份
└── MyApp.exe             ← 当前运行的文件
```

### 备份配置

```csharp
// 通过 UpdateOption 控制
await new GeneralClientBootstrap()
    .Option(UpdateOption.BackUp, true)  // 启用自动备份
    .SetConfig(config)
    .LaunchAsync();
```

### 回滚触发条件

- 更新后主程序启动失败
- 更新后 Bowl 检测到崩溃
- 中间件执行异常

### 回滚流程

```
1. 检测到故障
2. 从备份目录恢复文件
3. 清理临时文件和失败的更新
4. 上报失败状态 (ReportType.Failure)
```

### 备份清理

```csharp
// StorageManager.CleanBackup 按版本号排序清理
StorageManager.CleanBackup(installPath, keepVersions: 3);
// 保留最近 3 个版本的备份，删除更早的
```

---

## 六、Bowl 进程安全监控

Bowl 在更新完成后监控新进程的运行状态：

### 工作原理

```
1. Bowl 使用 ProcDump（Windows）启动目标进程
2. ProcDump 监控未处理异常
3. 检测到崩溃：
   a. 生成 Dump 文件（内存快照）
   b. 导出驱动信息 (driverInfo.txt)
   c. 导出系统信息 (systeminfo.txt)
   d. 导出系统事件日志 (systemlog.evtx)
   e. 生成崩溃报告 (fail.json)
   f. 自动回滚到上一个版本
```

### 安全注意事项

- Dump 文件包含完整内存快照，可能包含敏感信息（密钥、密码等）
- 确保 FailDirectory 的访问权限受限
- 定期清理旧的 Dump 文件

---

## 七、API 安全最佳实践

### 服务端

1. **HTTPS 强制：** 所有 API 端点必须使用 HTTPS
2. **认证必须：** 使用 Token/HMAC 认证，不要暴露未认证的端点
3. **输入验证：** 验证 `AppSecretKey`、`ClientVersion`、`ProductId` 格式
4. **速率限制：** 对版本检查接口实施速率限制
5. **日志审计：** 记录所有更新请求和结果

### 客户端

1. **证书固定：** 在自定义 `ISslValidationPolicy` 中实现证书固定
2. **最小权限：** 仅在必要时请求管理员权限
3. **配置保护：** 不要在代码中硬编码 `AppSecretKey`
4. **Dump 保护：** 确保 `.dmp` 文件不被未授权访问

---

## 八、安全清单

在生产环境部署前，请确认：

- [ ] 所有通信使用 HTTPS
- [ ] 配置了合适的 `IHttpAuthProvider`（Bearer Token 或 HMAC）
- [ ] 生产环境使用 `StrictSslValidationPolicy`（默认）
- [ ] 更新包 SHA256 哈希已正确配置并在服务端维护
- [ ] `AppSecretKey` 使用强随机 GUID 且每个产品唯一
- [ ] 客户端应用以最小必要权限运行
- [ ] Bowl 的 Dump 目录有适当的访问控制
- [ ] 备份文件定期清理，避免磁盘占满
- [ ] 已实现完整的异常监听和错误处理

---

## 相关资源

- **[系统架构](./Architecture.md)** — 了解安全机制在整体架构中的位置
- **[配置参考](./Configuration.md)** — 安全相关配置项
- **[GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl)** — Bowl 崩溃监控
- **[GeneralUpdate.Core](../doc/GeneralUpdate.Core)** — 客户端配置与安全选项
