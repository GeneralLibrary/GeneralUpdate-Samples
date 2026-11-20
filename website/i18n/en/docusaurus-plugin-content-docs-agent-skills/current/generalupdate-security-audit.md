---
sidebar_position: 8
sidebar_label: 🔒 Security Audit
title: 🔒 generalupdate-security-audit — Security Audit
---

# 🔒 GeneralUpdate Security Audit Guide

Comprehensive coverage of security risks in GeneralUpdate deployments. Based on code audit findings (17 CRITICAL/HIGH items) and best practices.

---

## 📋 Pre-Audit Information Collection

```
### Deployment Environment
- Deployment mode: ______ (Intranet / Public network / Hybrid)
- Tenant mode: ______ (Single-tenant / Multi-tenant)
- Number of clients: ______
- Client OS: ______ (Windows / Linux / macOS / Mixed)

### Server
- Backend type: ______ (GeneralSpacestation / Custom / OSS)
- Transport protocol: ______ (HTTP / HTTPS)
- Authentication: ______ (Bearer / Basic / HMAC / None)
- API publicly accessible: ______ (Yes/No, network isolated)

### Client
- GeneralUpdate version: ______
- Using IPC: ______ (Yes/No)
- Using Bowl: ______ (Yes/No)
- Using Differential: ______ (Yes/No)
```

---

## Security Audit Matrix

| # | Check | Severity | Description | Remediation |
|---|-------|----------|-------------|-------------|
| S01 | **AppSecretKey Strength** | 🔴 CRITICAL | Key too short, all letters, identical to example code | Use ≥ 32 chars, mixed case+digits+symbols random key |
| S02 | **IPC Encryption** | 🔴 CRITICAL | Default IPC encryption key hardcoded in binary | Ensure unique AppSecretKey consistent server/client |
| S03 | **HTTPS Transport** | 🟠 HIGH | UpdateUrl uses HTTP not HTTPS | Force HTTPS in production; configure HSTS |
| S04 | **ZipSlip Path Traversal** | 🔴 CRITICAL | ZIP extraction not validating ../ paths | Verify archive entry paths are within target directory |
| S05 | **Multi-Tenant Isolation** | 🔴 CRITICAL | Server not isolating tenants by ProductId | Add tenant identity middleware on server |
| S06 | **Event Log Leakage** | 🟡 MEDIUM | ExceptionEventArgs logs may contain sensitive paths | Sanitize before logging, filter paths and keys |
| S07 | **Patch Signing** | 🟠 HIGH | Differential patches have no digital signature verification | Apply Authenticode signing to update packages |
| S08 | **Temp Directory Permissions** | 🟡 MEDIUM | Temp extraction directory permissions may be too permissive | Set read/write for current user only |
| S09 | **OSS Bucket Permissions** | 🟠 HIGH | Update package storage bucket set to public read | Set to private, use pre-signed URLs |
| S10 | **Dependency Vulnerabilities** | 🟡 MEDIUM | GeneralUpdate and its dependencies may have known CVEs | Regularly check NuGet dependency security advisories |
| S11 | **Rollback Attack** | 🟠 HIGH | Attacker can submit lower version to force install old version | Server must validate monotonic version increase |
| S12 | **Download Integrity** | 🟠 HIGH | Downloaded update packages have no integrity check | Ensure Pipeline includes HashMiddleware |
| S13 | **Bowl Privilege Escalation** | 🟡 MEDIUM | Bowl crash daemon running at high privilege may be abused | Run Bowl with minimum required privileges |
| S14 | **Information Leak via manifest** | 🔵 LOW | ProductId, version numbers in manifest.json enumerable | Don't expose manifest file in non-public environments |

---

## Audit Report Output Format

After completing the audit, output in the following format:

```
## 🔒 GeneralUpdate Security Audit Report

### Summary
- Project: ______
- Audit Date: ______
- Overall Rating: A/B/C/D/F
- Critical issues: ______
- High risks: ______
- Medium risks: ______
- Low risks: ______

### Critical Issues (Must Fix Immediately)
- S01 AppSecretKey Strength: ⚠️ Current key length is X, needs ≥ 32
  Fix: ______

### High Risks (Fix Soon)
...

### Medium Risks (Evaluate and Fix)
...

### Low Risks (Document)
...

### Remediation Priority
1. Immediate: S01, S03, S04
2. This Week: S05, S07, S09
3. This Month: S08, S10, S11
```

---

## Security Configuration Checklist

- [ ] AppSecretKey length ≥ 32 characters, mixed case+digits+symbols
- [ ] Use HTTPS in production
- [ ] IPC file encoding set to Encoding.UTF8
- [ ] Pipeline includes HashMiddleware for integrity verification
- [ ] OSS Bucket permissions set to private
- [ ] Server isolates tenants by ProductId
- [ ] Version numbers strictly monotonically increasing
- [ ] Update packages Authenticode signed
- [ ] Zip extraction has path traversal protection
- [ ] Logs don't record sensitive information

---

## Related Skills

- `/generalupdate-init` — Fix audit findings
- `/generalupdate-advanced` — IPC replacement, custom authentication
- `/generalupdate-troubleshoot` — Known security issues reference
