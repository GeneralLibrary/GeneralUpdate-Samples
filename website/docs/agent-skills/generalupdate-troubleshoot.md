---
sidebar_position: 6
sidebar_label: 🩺 故障排查
title: 🩺 generalupdate-troubleshoot — 故障排查
---

# 🩺 GeneralUpdate 故障排查

综合性诊断系统 — 覆盖 50+ 已知问题，均可追溯到 GitHub/Gitee Issue 或代码审计发现。

## 工作流程

```
1. 症状收集
   ├── 用户描述的症状是什么？
   ├── 错误信息/堆栈是什么？
   ├── GeneralUpdate 版本号？
   ├── 平台（Windows/Linux/macOS）？
   └── 更新策略（标准/OSS/静默）？

2. 症状匹配 → 查找 reference.md
   ├── 找到匹配的 Q/C/H/M/L → 给出根因 + 修复 + 代码
   └── 未找到匹配 → 执行通用诊断流程（6步骤）

3. 提供修复
   ├── 具体的代码修改、配置调整、版本升级建议
   └── 预防措施（如何避免再发生）

4. 验证
   └── 确认修复后问题解决
```

## 症状分级

reference.md 中的问题按严重度分级：

| 级别 | 颜色 | 含义 | 数量 |
|:----:|:----:|------|:----:|
| C | 🔴 **致命** | 阻断性故障、数据损坏、安全漏洞 | 8 |
| H | 🟠 **高** | 场景阻断、功能失效、需要升级 | 11 |
| M | 🟡 **中** | 功能异常、需要配置调整 | 20 |
| L | 🔵 **低** | 代码气味、边缘情况、已知行为 | 12 |

---

## C 级（Critical）— 阻塞升级

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **升级没启动** | `LaunchAsync()` 未调用 / UpgradeApp.exe 未部署 | 确认 `Bootstrap.LaunchAsync()` 在 `Main()` 中调用 |
| **Method not found** | Client 和 Upgrade NuGet 版本不一致 | 统一 NuGet 版本, 清理 bin/obj 后重新生成 |
| **路径超长 (>260)** | Windows 路径限制 | 缩短安装路径 |
| **IPC 暴露** | IPC 加密密钥硬编码 | 使用强 AppSecretKey; 更新到 v10.4.6+ |
| **跨租户泄露** | 服务端多租户隔离缺失 | 每个租户独立 ProductId + AppSecretKey |
| **ZIP 遍历写入** | 恶意 ZIP 含 `../` 路径 | v10.4.6+ 已修复 |
| **BSDIFF 整数溢出** | 大文件差分补丁计算溢出 | 使用 HDiffPatch 算法 |
| **静默不生效** | 进程退出时未触发 | 确保正确调用 Close() 或 Dispose() |

## H 级（High）— 严重但不阻塞启动

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **无限循环更新** | manifest.json 版本号未回写 | 更新到 v10.4.6+（已修复 WriteBack） |
| **OSS 无更新** | Bucket 配置错误 / versions.json 格式不对 | curl 测试 OSS URL 是否可下载 |
| **文件占用** | 目标文件被占用 | 关闭主进程后更新; 排除杀软扫描目录 |
| **SignalR 推送无响应** | 连接断开或认证失败 | 检查 SignalR Hub 状态和 Token 配置 |
| **Bowl 不守护** | 进程名配置错误 | 确认 ProcessNameOrId 与实际进程名一致 |
| **差分更新失败** | 基准文件不匹配 | 校验原始文件哈希匹配 |
| **下载进度不动** | IDownloadService 未正确绑定 | 确认桥接实现是否正确 |
| **跨版本跳跃失败** | 中间版本包缺失 | 确保服务端保留了所有版本的补丁 |

## M 级（Medium）— 功能降级

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **AOT 编译失败** | 反射代码未适配 NativeAOT | 添加 `[DynamicDependency]` 属性 |
| **SignalR 重连慢** | RetryDelay 配置过长 | 调整重连参数 |
| **日志不输出** | 日志路径权限不足 | 检查 %TEMP%/GeneralUpdate/logs/ 权限 |
| **多租户配置错误** | ProductId 冲突 | 确保每个租户唯一 ProductId |
| **黑名单不生效** | 格式配置错误 | 确认文件名和扩展名格式正确 |

## L 级（Low）— 非关键

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **分发包过大** | 未使用差分 | 差分已内嵌在 Core，启用 `PatchEnabled` 即可 |
| **首次更新慢** | CDN 冷启动 | 预热 CDN |
| **更新后配置丢失** | 黑名单未包含配置目录 | 确认 `SkipDirectorys` 包含配置文件夹 |

---

## 通用诊断流程（6 步）

当问题无法直接匹配到已知症状时，执行以下 6 步排查：

1. **版本一致性检查** — Client 和 Upgrade 的 NuGet 版本是否一致？
2. **manifest.json 验证** — 文件是否存在？字段值是否正确？
3. **UpgradeApp 存在性** — UpgradeApp.exe 是否在预期目录？
4. **网络可访问性** — UpdateUrl 能否用 curl 访问？
5. **日志分析** — 查看 `%TEMP%/GeneralUpdate/logs/` 下的日志文件
6. **最小重现** — 从 Minimal 集成开始，逐步增加复杂度

## 日志文件位置

| 平台 | 默认路径 |
|------|----------|
| Windows | `%TEMP%/GeneralUpdate/logs/` |
| Linux | `/tmp/GeneralUpdate/logs/` |

## 安全注意事项

- AppSecretKey 管理 — 硬编码在客户端是最后手段; 优先从启动参数或环境变量注入
- 定期轮换 IPC 加密密钥
- 生产环境禁用调试日志

---

## 相关技能

- [🚀 generalupdate-init — Bootstrap 配置](./generalupdate-init.md)
- [🎨 generalupdate-ui — 更新界面诊断](./generalupdate-ui.md)
- [⚙️ generalupdate-strategy — 策略相关故障](./generalupdate-strategy.md)
- [🔧 generalupdate-advanced — 高级功能故障](./generalupdate-advanced.md)
