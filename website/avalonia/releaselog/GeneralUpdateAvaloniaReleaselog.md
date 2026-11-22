---
title: GeneralUpdate.Avalonia
authors: juster
tags: [log, avalonia]
---

# GeneralUpdate.Avalonia Release Log

### 2026-10-03 — `v0.0.1-beta.10`

- Changed version validation to query the update server using `UpdateServerOptions`; added support for a JSON package endpoint and custom `IUpdatePackageSource` implementations.
- Added the integrated `PrepareUpdateAsync` workflow and installation-result tracking, including persistent installation records and confirmation events.
- Added update pre-check callbacks, localized built-in messages, configurable download security and speed smoothing, and HTTP transport retry/resume hardening.
- Updated the reference documentation, flow guide, and quickstart for this release.

### 2026-06-14 — `v0.0.1-beta.9`

- First public release of `GeneralUpdate.Avalonia.Android`, with version comparison, resumable APK downloads, SHA256 verification, and Package Installer orchestration.
- Added HMAC, Bearer, API Key, and Basic authentication support.
- Added the Avalonia Android component reference and quickstart documentation.
