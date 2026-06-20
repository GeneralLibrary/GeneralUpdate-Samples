# Finalize all upgrade packages in wwwroot/packages
# - Adds mock content packages alongside PatchGenerator output
# - Generates a single comprehensive versions.json

Add-Type -AssemblyName System.IO.Compression.FileSystem
$packagesDir = Join-Path $PSScriptRoot "wwwroot\packages"
$contentDir = Join-Path (Split-Path -Parent $PSScriptRoot) "content"
$baseUrl = "http://localhost:5000/"
$productId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a"
$timestamp = Get-Date -Format "yyyyMMddHHmmssfff"

function Get-Hash($path) {
    $bytes = [System.IO.File]::ReadAllBytes($path)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    return [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace("-", "").ToLowerInvariant()
}

# Step 1: Add mock content packages (don't delete existing ones!)
Write-Host "=== Adding mock content packages ===" -ForegroundColor Cyan
foreach ($ver in @("1.0.0.1", "1.0.0.2")) {
    $srcPath = Join-Path $contentDir "v$ver"
    if (-not (Test-Path $srcPath)) { continue }
    foreach ($appType in @(1, 2)) {
        $label = if ($appType -eq 1) { "client" } else { "upgrade" }
        $pkgName = "packet_${timestamp}_full_${label}_$ver"
        $zipPath = Join-Path $packagesDir "$pkgName.zip"
        if (-not (Test-Path $zipPath)) {
            [System.IO.Compression.ZipFile]::CreateFromDirectory($srcPath, $zipPath,
                [System.IO.Compression.CompressionLevel]::Optimal, $false)
        }
        Write-Host "  $pkgName.zip"
    }
}

# Step 2: Build comprehensive versions.json from ALL packet_*.zip files
Write-Host ""
Write-Host "=== Building versions.json ===" -ForegroundColor Cyan
$zipFiles = Get-ChildItem -Path $packagesDir -Filter "packet_*.zip" | Sort-Object Name
$entries = @()

foreach ($f in $zipFiles) {
    $hash = Get-Hash $f.FullName
    $size = $f.Length
    $name = [System.IO.Path]::GetFileNameWithoutExtension($f.Name)

    # Determine if cross-version
    $isCross = $f.Name -match "_cross_"

    # Determine AppType
    $appType = if ($f.Name -match "_client_") { 1 } else { 2 }

    # Extract version from filename
    $verMatch = [regex]::Match($f.Name, "_(\d+\.\d+\.\d+\.\d+)\.zip$")
    if (-not $verMatch.Success) {
        $verMatch = [regex]::Match($f.Name, "_(\d+\.\d+\.\d+)\.zip$")
    }
    $version = if ($verMatch.Success) { $verMatch.Groups[1].Value } else { "0.0.0.0" }

    # Extract cross-version range
    $fromVer = $null
    $toVer = $null
    if ($isCross) {
        $crossMatch = [regex]::Match($f.Name, "_cross_.*?_(\d+\.\d+\.\d+\.\d+|\d+\.\d+\.\d+)_to_(\d+\.\d+\.\d+\.\d+|\d+\.\d+\.\d+)\.zip$")
        if (-not $crossMatch.Success) {
            $crossMatch = [regex]::Match($f.Name, "_cross_.*?_(\d+\.\d+\.\d+)_to_(\d+\.\d+\.\d+)\.zip$")
        }
        if ($crossMatch.Success) {
            $fromVer = $crossMatch.Groups[1].Value
            $toVer = $crossMatch.Groups[2].Value
        }
    }

    $entry = [PSCustomObject]@{
        PacketName     = $name
        Hash           = $hash
        Version        = $version
        Url            = "$baseUrl" + "File/Download/$hash"
        PubTime        = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
        AppType        = $appType
        Platform       = 1
        ProductId      = $productId
        IsForcibly     = $false
        Format         = ".zip"
        Size           = $size
        IsFreeze       = $false
        PackageType    = $(if ($isCross) { 1 } else { 2 })
    }
    $entries += $entry
    $crossLabel = if ($isCross) { "Chain" } else { "Full" }
    Write-Host "  AppType=$appType v$version [$crossLabel] $($f.Name) ($size bytes)"
}

$json = ConvertTo-Json -InputObject @($entries) -Depth 5
$versionsPath = Join-Path $packagesDir "versions.json"
[System.IO.File]::WriteAllText($versionsPath, $json, [System.Text.UTF8Encoding]::new($false))
Write-Host ""
Write-Host "Generated $($entries.Count) entries -> $versionsPath" -ForegroundColor Green
