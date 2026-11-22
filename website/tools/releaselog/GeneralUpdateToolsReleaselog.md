---
title: GeneralUpdate.Tools
authors: juster
tags: [log, tools]
---

# GeneralUpdate.Tools Release Log

### 2026-06-14 — `v20260614061359`

- Added the Mobile packaging workflow for APK/AAB files and Android projects: metadata extraction, `dotnet publish`, SHA256 calculation, package upload, and version-record export.
- Added a real-time build and upload result window.

### 2026-06-04 — `v20260604102526`

- Updated the desktop window and navigation experience with a frameless Ursa window style, emoji navigation icons, and text-based theme and language controls.

### 2026-06-03 — `v20260603132551`

- Fixed the release workflow so published artifacts are retained and attached to the GitHub Release.

### 2026-05-21 — `v20260521100654`, `v20260521093859`

- Improved Simulation by reusing cached test-app builds and bundling the test applications with release artifacts.
- Fixed test-app lookup to use the executable directory rather than the process base directory.

### 2026-05-20 — `v20260520132608`

- Improved the Simulation workflow and removed trimming that caused runtime failures; added native library extraction.

### 2026-02-17 — `v20260217`

- Updated packaging workflow and project configuration.

### 2026-02-16 — `v20260216`

- Refactored the packaging functionality.

### 2026-02-14 — `v20260214`

- Added project configuration extraction and JSON package generation.

### 2026-01-03 — `v20260103`

- Updated the .NET CI workflow.
