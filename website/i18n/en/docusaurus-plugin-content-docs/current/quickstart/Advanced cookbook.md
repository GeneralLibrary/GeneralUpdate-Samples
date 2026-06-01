---
sidebar_position: 4
title: Advanced cookbook
---

# GeneralUpdate advanced cookbook

> **Target audience**: developers who have basic GeneralUpdate experience and are integrating it into production products, CI/CD pipelines, and enterprise release workflows.

## Overview

This cookbook covers:

1. **Production update architecture** — overall topology of Client, Upgrade, Server, OSS, Tools, and CI
2. **Standardized release artifacts** — application packages, patch packages, extension packages, OSS configuration
3. **CI/CD integration** — triggering patch generation in GitHub Actions / internal pipelines
4. **Version strategy** — SemVer, branch releases, staged rollout, forced updates, multi-version sequential upgrades
5. **Differential strategy** — when to use differential, handling deleted files, blacklists, large-file scenarios
6. **Reliability design** — Bowl monitoring, backup, rollback, startup validation, log collection
7. **Extension system** — manifest design, extension package publishing, compatibility, dependency management
8. **Driver updates** — Drivelution permissions, signatures, platform differences, rollback and restart strategy
9. **Security and compliance** — hash validation, authenticated downloads, OSS permissions, release audit
10. **Failure drills** — simulate download failure, file lock, startup crash, corrupted patch, insufficient permissions

## Chapter index

Detailed content for each chapter is under active development. See the sub-pages below:

> **Status**: this document is under active construction. If you have production experience to share, PRs and issues are welcome.
