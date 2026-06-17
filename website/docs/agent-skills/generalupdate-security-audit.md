---
sidebar_position: 8
sidebar_label: 🔒 安全审计
title: 🔒 generalupdate-security-audit — 安全审计
---

# 🔒 GeneralUpdate 安全审计指南

全面覆盖 GeneralUpdate 部署的安全风险面。基于代码审计发现（17 CRITICAL/HIGH 项）和最佳实践。

---

## 📋 审计前信息收集

```
### 部署环境
- 部署模式: ______（内网 / 公网 / 混合）
- 租户模式: ______（单租户 / 多租户）
- 客户端数量: ______
- 客户端操作系统: ______（Windows / Linux / macOS / 混合）

### 服务端
- 后端类型: ______（GeneralSpacestation / 自定义 / OSS）
- 传输协议: ______（HTTP / HTTPS）
- 认证方式: ______（Bearer / Basic / HMAC / 无）
- API 是否公开访问: ______（是 / 否，有网络隔离）

### 客户端
- GeneralUpdate 版本: ______
- 是否使用 IPC: ______（是 / 否）
- 是否使用 Bowl: ______（是 / 否）
- 是否使用 Differential: ______（是 / 否）
```

---

## 安全审计矩阵

| # | 检查项 | 严重度 | 描述 | 修复措施 |
|---|--------|--------|------|---------|
| S01 | **AppSecretKey 强度** | 🔴 CRITICAL | 密钥长度不足、纯字母、与示例代码相同 | 使用 ≥ 32 字符，大小写+数字+符号的随机密钥 |
| S02 | **IPC 加密** | 🔴 CRITICAL | 默认 IPC 加密密钥硬编码在二进制中 | 确保 AppSecretKey 唯一且服务端/客户端一致 |
| S03 | **HTTPS 传输** | 🟠 HIGH | UpdateUrl 使用 HTTP 而非 HTTPS | 生产环境强制 HTTPS；配置 HSTS |
| S04 | **ZipSlip 路径穿越** | 🔴 CRITICAL | 解压 ZIP 时未验证 ../ 路径 | 验证压缩包条目路径是否在目标目录内 |
| S05 | **多租户隔离** | 🔴 CRITICAL | 服务端未按 ProductId 隔离租户 | 服务端添加租户身份验证中间件 |
| S06 | **事件日志泄露** | 🟡 MEDIUM | ExceptionEventArgs 日志可能包含敏感路径 | 脱敏后记录，过滤路径和密钥 |
| S07 | **差分包签名** | 🟠 HIGH | 差分补丁无数字签名验证 | 对更新包进行 Authenticode 签名 |
| S08 | **临时目录权限** | 🟡 MEDIUM | 临时解压目录权限可能过大 | 设置仅为当前用户可读写 |
| S09 | **OSS Bucket 权限** | 🟠 HIGH | 更新包存储 Bucket 设为公共读 | 设置为私有，使用预签名 URL |
| S10 | **依赖版本漏洞** | 🟡 MEDIUM | GeneralUpdate 及其依赖可能存在已知 CVE | 定期检查 NuGet 依赖安全公告 |
| S11 | **回滚攻击** | 🟠 HIGH | 攻击者可提交降级版本号强制安装旧版本 | 服务端校验版本号单调递增 |
| S12 | **下载完整性** | 🟠 HIGH | 下载的更新包无完整性校验 | 确保 Pipeline 包含 HashMiddleware |
| S13 | **Bowl 提权** | 🟡 MEDIUM | Bowl 崩溃守护以高权限运行可能被滥用 | 以最小必要权限运行 Bowl |
| S14 | **信息泄露通过 manifest** | 🔵 LOW | manifest.json 中的 ProductId、版本号可被枚举 | 非公开环境下不暴露 manifest 文件 |

---

## 审计报告输出格式

完成审计后按以下格式输出：

```
## 🔒 GeneralUpdate 安全审计报告

### 概要
- 项目: ______
- 审计日期: ______
- 总体评分: A/B/C/D/F
- 严重问题: ______ 个
- 高风险: ______ 个
- 中风险: ______ 个
- 低风险: ______ 个

### 严重问题（必须立即修复）
- S01 AppSecretKey 强度: ⚠️ 当前密钥长度为 X，需要 ≥ 32
  修复: ______

### 高风险（建议尽快修复）
...

### 中风险（评估后修复）
...

### 低风险（记录在案）
...

### 修复建议优先级
1. 立即：S01, S03, S04
2. 本周：S05, S07, S09
3. 本月：S08, S10, S11
```

---

## 安全配置检查清单

- [ ] AppSecretKey 长度 ≥ 32 字符，混合大小写+数字+符号
- [ ] 生产环境使用 HTTPS
- [ ] IPC 文件编码设为 Encoding.UTF8
- [ ] Pipeline 包含 HashMiddleware 做完整性校验
- [ ] OSS Bucket 权限设为私有
- [ ] 服务端按 ProductId 隔离租户
- [ ] 版本号严格单调递增
- [ ] 更新包进行 Authenticode 签名
- [ ] Zip 解压有路径穿越防护
- [ ] 日志中不记录敏感信息

---

## 相关技能

- `/generalupdate-init` — 修复审计发现的问题
- `/generalupdate-advanced` — IPC 替换、自定义认证
- `/generalupdate-troubleshoot` — 已知安全问题参考
