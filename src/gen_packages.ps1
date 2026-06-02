# Quick package generation for Hub testing
$srcDir = $PSScriptRoot
$packagesDir = Join-Path $srcDir "Server\wwwroot\packages"
$productId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a"
$timestamp = Get-Date -Format "yyyyMMddHHmmssfff"
$baseUrl = "http://localhost:5000/"

New-Item -ItemType Directory -Force -Path $packagesDir | Out-Null
Get-ChildItem -Path $packagesDir -Filter "packet_*.zip" | Remove-Item -Force -ErrorAction SilentlyContinue

$allVersions = @()

function New-Package($SourceDir, $PacketName, $Version, $AppType) {
    $zipPath = Join-Path $packagesDir "$PacketName.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($SourceDir, $zipPath,
        [System.IO.Compression.CompressionLevel]::Optimal, $false)
    $hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $size = (Get-Item $zipPath).Length
    $entry = @{
        PacketName = $PacketName
        Hash = $hash
        Version = $Version
        Url = "$baseUrl`File/Download/$hash"
        PubTime = (Get-Date).ToString("o")
        AppType = $AppType
        Platform = 1
        ProductId = $productId
        IsForcibly = $false
        Format = ".zip"
        Size = $size
        IsFreeze = $false
        IsCrossVersion = $false
        FromVersion = $null
        ToVersion = $null
    }
    $script:allVersions += $entry
    Write-Host "  $PacketName ($size bytes)" -ForegroundColor Gray
}

# Client v2.0.0.0 (AppType=1)
Write-Host "=== Client (AppType=1) ===" -ForegroundColor Green
$clientV2 = Join-Path $srcDir "content_client\v2.0.0.0"
New-Package -SourceDir $clientV2 -PacketName "packet_${timestamp}_full_client_2.0.0.0" -Version "2.0.0.0" -AppType 1

# Upgrade v2.0.0.0 (AppType=2)
Write-Host "=== Upgrade (AppType=2) ===" -ForegroundColor Green
$upgradeV2 = Join-Path $srcDir "content_upgrade\v2.0.0.0"
New-Package -SourceDir $upgradeV2 -PacketName "packet_${timestamp}_full_upgrade_2.0.0.0" -Version "2.0.0.0" -AppType 2

# Intermediate versions
Write-Host "=== Intermediate ===" -ForegroundColor Green
foreach ($ver in @("1.0.0.1", "1.0.0.2")) {
    $src = Join-Path $srcDir "content\v$ver"
    if (Test-Path $src) {
        New-Package -SourceDir $src -PacketName "packet_${timestamp}_full_client_$ver" -Version $ver -AppType 1
        New-Package -SourceDir $src -PacketName "packet_${timestamp}_full_upgrade_$ver" -Version $ver -AppType 2
    }
}

# Write versions.json
$json = $allVersions | ConvertTo-Json -Depth 10
$versionsPath = Join-Path $packagesDir "versions.json"
[System.IO.File]::WriteAllText($versionsPath, $json, [System.Text.Encoding]::UTF8)
Write-Host "`nGenerated $($allVersions.Count) packages -> $versionsPath" -ForegroundColor Cyan
