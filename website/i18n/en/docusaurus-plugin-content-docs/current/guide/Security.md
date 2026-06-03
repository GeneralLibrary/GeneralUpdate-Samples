---
sidebar_position: 4
---

# Security Guide

This document details the multi-layered security mechanisms of the GeneralUpdate framework.

---

## Security Architecture Overview

GeneralUpdate's security model adopts a **Defense in Depth** strategy, establishing multiple layers of protection across transport, authentication, data integrity, and inter-process communication.

---

## 1. Transport Security (TLS/SSL)

### HTTPS Enforcement

GeneralUpdate's HTTP client requires HTTPS connections by default. All server communication (version check, download, reporting) should use HTTPS.

### SSL Certificate Validation

**Interface:** `ISslValidationPolicy`

```csharp
public interface ISslValidationPolicy
{
    bool ValidateCertificate(
        X509Certificate2 certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors);
}
```

**Default Policy:** `StrictSslValidationPolicy` — rejects on any `SslPolicyErrors`.

**Custom SSL Policy:**

```csharp
// Development only: accept self-signed certificates
public class DevelopmentSslPolicy : ISslValidationPolicy
{
    public bool ValidateCertificate(...)
    {
        return true; // NEVER use in production!
    }
}

await new GeneralClientBootstrap()
    .SslPolicy<DevelopmentSslPolicy>()
    .SetConfig(config)
    .LaunchAsync();
```

> **⚠️ Warning:** Never disable SSL validation in production!

---

## 2. Authentication

### HTTP Auth Provider Interface

```csharp
public interface IHttpAuthProvider
{
    Task ApplyAuthAsync(HttpRequestMessage request, 
        CancellationToken cancellationToken);
}
```

### Built-in Auth Schemes

| Scheme | Provider Class | Header |
|--------|---------------|--------|
| **Bearer Token** | `BearerTokenAuthProvider` | `Authorization: Bearer {token}` |
| **API Key** | `ApiKeyAuthProvider` | `X-Api-Key: {key}` (configurable) |
| **HMAC** | `HmacAuthProvider` | `X-Update-Timestamp` + `X-Update-Signature` |
| **None** | `NoOpAuthProvider` | — |

### HMAC Authentication

The `HmacAuthProvider` computes HMAC-SHA256 over `body|timestamp`:

```
Signature = HMAC-SHA256(requestBody + "|" + timestamp, secretKey)
Headers:
  X-Update-Timestamp: 1717488000
  X-Update-Signature: base64_signature
```

### Factory

```csharp
// Auto-select auth scheme by name
var authProvider = HttpAuthProviderFactory.Create(
    scheme: "HMAC", 
    token: null, 
    secretKey: "your-secret-key");
```

---

## 3. Data Integrity

### SHA256 Hash Verification

Every downloaded update package is verified by `HashMiddleware` in the pipeline:

1. Server returns expected hash with version info
2. `HashMiddleware` computes actual hash of downloaded file
3. Compare expected vs actual
4. Mismatch → `CryptographicException` → rollback + failure report

### Differential Patch Integrity

BSDiff patches include embedded CRC checksums in control sections. Any data corruption during patch application causes automatic rollback.

---

## 4. IPC Security

Parameters passed between ClientCore and Core are encrypted using AES-256-CBC.

| Parameter | Value |
|-----------|-------|
| Algorithm | AES-256-CBC |
| Key Derivation | SHA256("GeneralUpdate.IPC.EnvironmentProvider.v1") |
| Key Length | 256 bits |
| IV Length | 128 bits |
| Temp File Location | `%TEMP%/GeneralUpdate/ipc/` |
| File Lifecycle | Write → Read → Auto-delete |

### Data Flow

```
ClientCore writes:
  1. ProcessContract → JSON serialize
  2. JSON → AES-256-CBC encrypt
  3. Ciphertext → temp file → path to environment variable

Core reads:
  1. Get temp file path from environment variable
  2. Read ciphertext → AES-256-CBC decrypt
  3. JSON deserialize → ProcessContract
  4. Auto-delete temp file
```

---

## 5. Backup & Rollback

### Automatic Backup

Files are backed up before update to version-named directory:

```
{InstallPath}/
├── app-1.0.0.0/          ← Backup (version-named)
├── app-1.0.1.0/          ← Another version backup
└── MyApp.exe             ← Current files
```

### Rollback Triggers

- Main app fails to start after update
- Bowl detects crash
- Middleware execution exception

### Rollback Flow

```
1. Detect failure
2. Restore files from backup directory
3. Clean up temp files
4. Report failure status (ReportType.Failure)
```

---

## 6. Bowl Process Security

Bowl monitors the new process after update completion:

1. Uses ProcDump (Windows) to launch the target process
2. Monitors for unhandled exceptions
3. On crash:
   - Generate dump file (memory snapshot)
   - Export driver info
   - Export system info
   - Export system event log
   - Auto-rollback to previous version

**⚠️ Dump files contain full memory snapshots including sensitive data. Restrict access to FailDirectory.**

---

## 7. API Security Best Practices

### Server Side

1. **Enforce HTTPS** — All API endpoints must use HTTPS
2. **Require Authentication** — Use Token/HMAC, never expose unauthenticated endpoints
3. **Input Validation** — Validate `AppSecretKey`, `ClientVersion`, `ProductId` formats
4. **Rate Limiting** — Apply rate limits to verification endpoints
5. **Audit Logging** — Log all update requests and results

### Client Side

1. **Certificate Pinning** — Implement in custom `ISslValidationPolicy`
2. **Least Privilege** — Only request admin rights when necessary
3. **Config Protection** — Never hardcode `AppSecretKey`
4. **Dump Protection** — Ensure `.dmp` files are not accessible to unauthorized users

---

## 8. Production Security Checklist

- [ ] All communication uses HTTPS
- [ ] Appropriate `IHttpAuthProvider` configured (Bearer Token or HMAC)
- [ ] Production uses `StrictSslValidationPolicy` (default)
- [ ] Update package SHA256 hashes properly maintained on server
- [ ] `AppSecretKey` uses strong random GUID, unique per product
- [ ] Client app runs with least necessary privileges
- [ ] Bowl dump directory has proper access controls
- [ ] Backup files regularly cleaned to prevent disk exhaustion
- [ ] Complete exception monitoring and error handling implemented

---

## Related Resources

- **[Architecture](./Architecture.md)** — Security in overall architecture
- **[Configuration Reference](./Configuration.md)** — Security-related config options
- **[GeneralUpdate.Bowl](../doc/GeneralUpdate.Bowl)** — Crash monitoring
- **[GeneralUpdate.Core](../doc/GeneralUpdate.Core)** — Client security options
