---
title: GeneralUpdate.Avalonia
authors: juster
tags: [log, avalonia]
---

# GeneralUpdate.Avalonia Release Log

### 2026-10-03 — `v0.0.1-beta.10`

- 版本校验改为通过 `UpdateServerOptions` 查询更新服务；支持 JSON 包信息接口和自定义 `IUpdatePackageSource`。
- 新增 `PrepareUpdateAsync` 一体化流程及安装结果跟踪，包括持久化安装记录和确认事件。
- 新增更新前检查回调、内置消息本地化、下载安全与速度平滑配置，并加固 HTTP 重试和断点续传。
- 更新组件参考、流程说明与快速入门文档。

### 2026-06-14 — `v0.0.1-beta.9`

- `GeneralUpdate.Avalonia.Android` 首个公开版本，提供版本比较、APK 断点续传、SHA256 校验及系统安装器调用。
- 支持 HMAC、Bearer、API Key 和 Basic 认证。
- 新增 Avalonia Android 组件参考与快速入门文档。
