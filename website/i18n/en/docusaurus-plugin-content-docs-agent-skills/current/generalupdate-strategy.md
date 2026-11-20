---
sidebar_position: 4
sidebar_label: ⚙️ Strategy
title: ⚙️ generalupdate-strategy — Update Strategy Guide
---

# ⚙️ Choose Your Update Strategy

An update strategy = how GeneralUpdate discovers and downloads new versions.

Based on whether you have a backend server, whether you need to save bandwidth, and whether users need to see progress, pick the strategy that fits.

---

## First: What a strategy really does

Every strategy answers the same 3 questions:

```
① Where to check for updates?   ② Where to download from?    ③ How to download?
    │                                │                            │
    ├ Poll the server yourself        ├ Your own server           ├ Full download
    ├ Server pushes to you            ├ Object storage (OSS)      └ Only the diff
    └                                └
```

---

## Quick strategy picker

Don't worry if the terms are unfamiliar — just read the first column:

| Your situation | Recommended strategy | One-liner |
|---------------|---------------------|-----------|
| **Have a backend, just getting started** | **① Standard** | Simplest option — server returns versions, client downloads |
| **No backend, just want cloud storage** | **② OSS** | Upload packages to S3/MinIO, zero server cost |
| **Limited bandwidth, many users** | **④ Differential (Diff)** | Only download what changed — saves 60-90% bandwidth |
| **Need users to update directly to the latest** | **⑤ Cross-version (CVP)** | Jump from v1.0 straight to v3.0, no intermediate steps |
| **Users shouldn't notice the update** | **③ Silent** | Download in background, apply on next launch |
| **Emergency security patch** | **⑥ Push (SignalR)** | Server tells clients "update right now" |

---

## Strategy details

### ① Standard Client-Server (recommended for beginners)

**Use when**: You have a backend service (e.g., GeneralSpacestation) and want the simplest setup.

```
Flow: Client → asks server "any new versions?"
      Server → returns version list
      Client → downloads → launches upgrade
```

**Pros**: Simplest to implement, one API endpoint
**Cons**: Requires and maintains a backend server

### ② OSS Object Storage

**Use when**: You don't have a backend but have object storage (Alibaba OSS / AWS S3 / MinIO).

```
Flow: Client → periodically reads versions.json from OSS
      OSS → returns version list
      Client → downloads from OSS → launches upgrade
```

**Pros**: Zero server, lowest cost
**Cons**: Doesn't distinguish main app vs upgrade app packages

### ③ Silent Update

**Use when**: Users shouldn't see the update process.

```
Flow: Client checks in background → finds update → downloads silently
      After download → notifies user "update ready, restart?"
      Or: applies automatically on next launch
```

**Pros**: Great user experience, no interruption
**Cons**: Need to handle "downloaded but user hasn't restarted" state
**Poll frequency**: Every 30-60 minutes (shorter = battery drain, longer = user waits)

### ④ Differential Update

**Use when**: Your packages are large (>100MB) with many users, and you want to save bandwidth.

```
Flow: Server generates "patches" (only the differences between versions)
      Client downloads patch → applies it locally → new version ready
```

**Pros**: Saves 60-90% download size
**Cons**: Server needs extra build step for patches; large files (>2GB) may overflow

### ⑤ Cross-Version Update (CVP)

**Use when**: Your users are on wildly different versions (v1.0, v2.5, etc.) and all need to get to v3.0.

```
Flow: Server keeps upgrade paths for all versions
      v1.0 user → downloads v3.0 full package directly
      v2.5 user → downloads only v2.5→v3.0 diff patch
```

**Pros**: No need to install intermediate versions sequentially
**Cons**: Server needs extra build and maintenance

### ⑥ SignalR Push

**Use when**: You need to push urgent security fixes — can't wait for users to poll.

```
Flow: Server pushes "new version available" via SignalR connection
      Client immediately starts downloading
```

**Pros**: Real-time push, second-level response
**Cons**: Requires SignalR Hub deployment; need fallback to polling on disconnect

---

## Mixed strategy combinations

In real projects, you can combine strategies:

| Combo | Best for | How it works |
|-------|----------|-------------|
| ① Standard + UI | Almost everyone | Standard check + show download progress bar |
| ② OSS + ④ Diff | No backend + save bandwidth | Packages on OSS, download only diffs |
| ③ Silent + ① Standard | Background services | Periodic check, background download |
| ⑤ CVP + ⑥ Push | Forced upgrade | Server pushes "update now", skip all intermediate versions |

---

## If you're not sure which to pick

```
Have a backend server?
├── Yes → ① Standard (get it running first)
│        Add ④ Diff or ③ Silent later if needed
│
└── No  → ② OSS (zero cost to start)
         Upload packages to S3/MinIO
```

**This path won't go wrong.** Get it working first, optimize later.

---

## Things to watch out for

| # | Issue | Suggestion |
|---|-------|-----------|
| 1 | NuGet version mismatch → "Method not found" | Client and Upgrade must use **exactly the same** NuGet version |
| 2 | OSS mode doesn't distinguish Main/Upgrade | This is a normal OSS limitation — just accept it |
| 3 | Upgrade app must be in `update/` subdirectory | Build this directory structure from the first release |
| 4 | Linux/macOS don't support Bowl crash daemon | Bowl is Windows-only |
| 5 | Diff patches over 2GB can overflow | Use full packages for very large files |

---

## Related pages

- [generalupdate-init](generalupdate-init) — Bootstrap configuration
- [generalupdate-ui](generalupdate-ui) — Update UI
- [generalupdate-advanced](generalupdate-advanced) — Advanced customization
- [generalupdate-troubleshoot](generalupdate-troubleshoot) — Troubleshooting
