---
sidebar_position: 6
sidebar_label: 🩺 Troubleshoot
title: 🩺 generalupdate-troubleshoot — Troubleshooting
---

# 🩺 GeneralUpdate Troubleshooting

Comprehensive diagnostic system covering 50+ known issues, all traceable to GitHub/Gitee Issues or code audit findings.

---

## 📋 User Symptom Collection

```
### Required Information
- Symptom description: ______
- Error message/stack trace: ______
- GeneralUpdate version: ______
- Platform: ______ (Windows / Linux / macOS)
- .NET version: ______
- Update strategy: ______ (Standard / OSS / Silent / Differential / CVP / Push)
- Recent config changes: ______ (Yes/No, what changed)

### Optional Information
- Any ExceptionEventArgs in event listeners: ______
- Any logs (Logs/generalupdate-trace *.log): ______
- Issue reproducible: ______ (Yes/No, frequency)
- First occurrence: ______
```

---

## Workflow

```
1. Symptom Collection
   ├── What is the user's symptom?
   ├── Error message/stack trace?
   ├── GeneralUpdate version?
   ├── Platform (Windows/Linux/macOS)?
   └── Update strategy (Standard/OSS/Silent)?

2. Symptom Matching
   ├── Priority: python3 scripts/search.py "<symptom>" --domain issue
   │   └── Matched → Provide root cause + fix + code
   └── No match → Fall back to reference.md full text search

3. Provide Fix
   ├── Specific code changes, config adjustments, version upgrade suggestions
   └── Preventive measures

4. Verify
   └── Confirm the fix resolves the issue
```

## Symptom Search (Recommended)

Prefer using the BM25 search engine for precise matching of known issues:

```bash
# Natural language search for known issues
python3 skills/generalupdate-troubleshoot/scripts/search.py "update succeeded but app crashes" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "method not found" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "garbled Chinese text" --domain issue

# Search for strategy-related issues
python3 skills/generalupdate-troubleshoot/scripts/search.py "OSS permission error" --domain strategy
```

## Symptom Severity Levels

| Level | Color | Meaning | Count |
|:-----:|:-----:|---------|:-----:|
| C | 🔴 **Critical** | Blocking failure, data corruption, security vulnerability | 8 |
| H | 🟠 **High** | Scenario blocking, feature failure, upgrade needed | 11 |
| M | 🟡 **Medium** | Functionality degraded, configuration adjustment needed | 20 |
| L | 🔵 **Low** | Code smell, edge cases, known behavior | 12 |

**Full list available in `reference.md`**

---

## ✅ Universal Pre-Diagnosis Checklist

### Runtime Environment Check
- [ ] Target machine has correct .NET runtime installed (matches publish framework)
- [ ] Target machine has write permissions (InstallPath directory writable)
- [ ] Firewall not blocking UpdateUrl communication port
- [ ] Sufficient disk space (at least 2× update package size)
- [ ] Linux/macOS: UpgradeApp has `chmod +x` executable permissions

### Version Check
- [ ] Client and Upgrade projects use the **exact same** NuGet version
- [ ] Server returns version numbers in 4-part format (e.g. 1.0.0.0)
- [ ] manifest.json `mainAppName` matches actual process name
- [ ] `AppType` set correctly (Client = 1, Upgrade = 2)

### Configuration Check
- [ ] All 6 required `UpdateRequest` fields are set
- [ ] `UpdateUrl` accessible via HTTP GET returning valid JSON
- [ ] `AppSecretKey` matches server config (length ≥ 16 characters)
- [ ] UpgradeApp.exe exists in `update/` subdirectory of publish directory

### Log Check
- [ ] Check `Logs/generalupdate-trace-*.log` (if available)
- [ ] Check `ExceptionEventArgs` in event listeners
- [ ] Check `MultiDownloadErrorEventArgs` for exceptions

---

## C Level (Critical) — Blocks Updates

| Issue | Cause | Solution |
|-------|-------|----------|
| **Update not starting** | `LaunchAsync()` not called / UpgradeApp.exe not deployed | Ensure `Bootstrap.LaunchAsync()` is called in `Main()` |
| **Method not found** | Client and Upgrade NuGet versions differ | Unify NuGet versions, clean bin/obj and rebuild |
| **Path too long (>260)** | Windows path limit | Shorten install path |
| **IPC exposure** | IPC encryption key hardcoded | Use strong AppSecretKey; upgrade to v10.4.6+ |
| **Cross-tenant leakage** | Missing server multi-tenant isolation | Independent ProductId + AppSecretKey per tenant |
| **ZIP traversal write** | Malicious ZIP with `../` paths | v10.4.6+ already fixed |
| **BSDIFF integer overflow** | Large file diff calculation overflow | Use HDiffPatch algorithm |
| **Silent mode not working** | Process exit not triggered | Ensure Close() or Dispose() is called correctly |

## H Level (High) — Severe but Non-Blocking

| Issue | Cause | Solution |
|-------|-------|----------|
| **Infinite update loop** | manifest.json version not updated | Upgrade to v10.4.6+ (WriteBack fixed) |
| **OSS no update found** | Bucket config wrong / incorrect versions.json | curl test OSS URL |
| **File in use** | Target file locked | Close main process before update |
| **SignalR push not responding** | Connection dropped or auth failure | Check SignalR Hub status and Token config |
| **Bowl not monitoring** | Process name misconfigured | Verify ProcessNameOrId matches actual process |

## M Level (Medium) — Functionality Degraded

| Issue | Cause | Solution |
|-------|-------|----------|
| **AOT build failure** | Reflection not adapted for NativeAOT | Add `[DynamicDependency]` attributes |
| **SignalR reconnection slow** | RetryDelay too long | Adjust retry parameters |
| **Logs not outputting** | Log path permission issue | Check %TEMP%/GeneralUpdate/logs/ permissions |
| **Multi-tenant config wrong** | ProductId collision | Ensure unique ProductId per tenant |

## L Level (Low) — Non-Critical

| Issue | Cause | Solution |
|-------|-------|----------|
| **Package too large** | Differential not enabled | Differential is embedded in Core, enable `PatchEnabled` |
| **First update slow** | CDN cold start | Warm up CDN |
| **Config lost after update** | Blacklist doesn't include config dir | Ensure `Directories` includes config folder |

---

## 6-Step Universal Diagnostic

When the issue can't be directly matched, follow these 6 steps:

1. **Version consistency check** — Are Client and Upgrade NuGet versions the same?
2. **manifest.json verification** — Does the file exist? Are fields correct?
3. **UpgradeApp existence** — Is UpgradeApp.exe in the expected directory?
4. **Network accessibility** — Can you curl the UpdateUrl?
5. **Log analysis** — Check logs in `Logs/generalupdate-trace-*.log`
6. **Minimal reproduction** — Start with Minimal integration, gradually add complexity

## Log File Locations

| Platform | Default Path |
|----------|-------------|
| Windows | `%TEMP%/GeneralUpdate/logs/` |
| Linux | `/tmp/GeneralUpdate/logs/` |

## Security Notes

- AppSecretKey management — hardcoding in the client is a last resort; prefer startup arguments or environment variables
- Rotate IPC encryption keys regularly
- Disable debug logging in production

---

## ⚠️ Anti-Pattern Checklist

| # | Anti-Pattern | Consequence | Correct Approach |
|---|-------------|-------------|------------------|
| 1 | **Only looking at error messages, not events** | Missing detailed info in ExceptionEventArgs | Subscribe to all 6 events |
| 2 | **Assuming no logs because wrong path** | Missing critical diagnostic info | Check InstallPath/Logs directory |
| 3 | **Only checking Client, not Upgrade process** | Issue is in Upgrade but diagnosis goes wrong | Check both processes |
| 4 | **Modifying code for update issues immediately** | Issue might be server config, not client bug | Check server version info first |
| 5 | **Ignoring NuGet version consistency** | "Method not found" root cause is version mismatch | Check versions first |
| 6 | **Testing only in Debug environment** | Release may lack runtime files | Reproduce in release/production environment |

---

## Related Skills

- `/generalupdate-init` — Bootstrap configuration
- `/generalupdate-ui` — Update UI diagnostics
- `/generalupdate-strategy` — Strategy-related issues
- `/generalupdate-advanced` — Advanced feature issues
