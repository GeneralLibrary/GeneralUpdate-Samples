# ================================================================
# GeneralUpdate Sample Server — Upgrade Package Generator
#
# Generates full (VersionChain) and differential (CrossVersion)
# upgrade packages from the sample content directories.
#
# Output: wwwroot/packages/*.zip + wwwroot/packages/versions.json
#
# For differential packages, this script invokes the PatchGenerator
# C# project when available; otherwise generates only full packages.
# ================================================================
param(
    [switch]$FullOnly       # Skip differential packages (fast mode)
)

$ErrorActionPreference = "Stop"

$srcDir = Split-Path -Parent $PSScriptRoot
$contentClientDir = Join-Path $srcDir "content_client"
$contentUpgradeDir = Join-Path $srcDir "content_upgrade"
$packagesDir = Join-Path $PSScriptRoot "wwwroot\packages"
$timestamp = Get-Date -Format "yyyyMMddHHmmssfff"
$baseUrl = "http://localhost:5000/"
$productId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a"

# Ensure output directory exists
New-Item -ItemType Directory -Force -Path $packagesDir | Out-Null

# Clean old generated packages
Get-ChildItem -Path $packagesDir -Filter "packet_*.zip" | Remove-Item -Force
Write-Host "Cleaned old packages." -ForegroundColor Cyan

$allVersions = @()

# ===================================================================
# Helper functions
# ===================================================================

function Get-SHA256Hash($filePath) {
    $stream = [System.IO.File]::OpenRead($filePath)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $hash = [System.BitConverter]::ToString($sha.ComputeHash($stream)).Replace("-", "").ToLowerInvariant()
    $stream.Close()
    return $hash
}

function New-VersionEntry {
    param(
        [string]$PacketName, [string]$Version, [int]$AppType,
        [int]$Platform = 1, [string]$ProductId,
        [bool]$IsForcibly = $false, [string]$Format = ".zip",
        [bool]$IsCrossVersion = $false,
        [string]$FromVersion = $null, [string]$ToVersion = $null,
        [bool]$IsFreeze = $false
    )
    $zipPath = Join-Path $packagesDir "$PacketName.zip"
    $hash = Get-SHA256Hash $zipPath
    $size = (Get-Item $zipPath).Length

    return @{
        PacketName    = $PacketName
        Hash          = $hash
        Version       = $Version
        Url           = "$baseUrl`File/Download/$hash"
        PubTime       = (Get-Date).ToString("o")
        AppType       = $AppType
        Platform      = $Platform
        ProductId     = $ProductId
        IsForcibly    = $IsForcibly
        Format        = $Format
        Size          = $size
        IsFreeze      = $IsFreeze
        IsCrossVersion = $(if ($IsCrossVersion) { $true } else { $false })
        FromVersion   = $FromVersion
        ToVersion     = $ToVersion
    }
}

function Add-ZipPackage {
    param(
        [string]$SourceDir, [string]$PacketName,
        [string]$Version, [int]$AppType,
        [bool]$IsCrossVersion = $false,
        [string]$FromVersion = $null, [string]$ToVersion = $null
    )
    $zipPath = Join-Path $packagesDir "$PacketName.zip"
    Write-Host "  Creating: $PacketName.zip" -ForegroundColor Yellow

    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

    # Use .NET for compression (available in PowerShell 5+)
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory(
        $SourceDir, $zipPath,
        [System.IO.Compression.CompressionLevel]::Optimal,
        $false  # includeBaseDirectory = false
    )

    $size = (Get-Item $zipPath).Length
    Write-Host "    Size: $size bytes" -ForegroundColor Gray

    $entry = New-VersionEntry -PacketName $PacketName -Version $Version `
        -AppType $AppType -ProductId $productId `
        -IsCrossVersion $IsCrossVersion -FromVersion $FromVersion -ToVersion $ToVersion
    $script:allVersions += $entry
}

# ===================================================================
# CLIENT packages (AppType=1)
#   Current version: 1.0.0  →  Target version: 2.0.0.0
# ===================================================================
Write-Host ""
Write-Host "===== CLIENT Packages (AppType=1) =====" -ForegroundColor Green

$clientV1 = Join-Path $contentClientDir "v1.0.0.0"
$clientV2 = Join-Path $contentClientDir "v2.0.0.0"

if ((Test-Path $clientV2) -and (Test-Path $clientV1)) {
    # Full (VersionChain) package
    $pkgName = "packet_${timestamp}_full_client_2.0.0.0"
    Add-ZipPackage -SourceDir $clientV2 -PacketName $pkgName `
        -Version "2.0.0.0" -AppType 1 -IsCrossVersion $false

    if (-not $FullOnly) {
        # Differential (CrossVersion) package via PatchGenerator
        Write-Host "  [CrossVersion] Generating differential patches via PatchGenerator..." -ForegroundColor Yellow
        $patchGeneratorProj = Join-Path $srcDir "PatchGenerator\PatchGenerator.csproj"
        if (Test-Path $patchGeneratorProj) {
            Push-Location (Join-Path $srcDir "PatchGenerator")
            try {
                dotnet run --project $patchGeneratorProj
                Write-Host "    PatchGenerator completed." -ForegroundColor Gray
            }
            catch {
                Write-Host "    [Warn] PatchGenerator failed: $_" -ForegroundColor Magenta
                Write-Host "    Run 'dotnet run --project $patchGeneratorProj' manually for differential packages." -ForegroundColor Magenta
            }
            finally { Pop-Location }

            # PatchGenerator writes directly to Server/wwwroot/packages/
            # Also write its output to versions.json
            $patchVersionsJson = Join-Path $packagesDir "versions.json"
            if (Test-Path $patchVersionsJson) {
                $patchEntries = Get-Content $patchVersionsJson -Raw | ConvertFrom-Json
                foreach ($pe in $patchEntries) {
                    $script:allVersions += @{
                        PacketName    = $pe.PacketName
                        Hash          = $pe.Hash
                        Version       = $pe.Version
                        Url           = $pe.Url
                        PubTime       = $pe.PubTime
                        AppType       = $pe.AppType
                        Platform      = $pe.Platform
                        ProductId     = $pe.ProductId
                        IsForcibly    = $pe.IsForcibly
                        Format        = $pe.Format
                        Size          = $pe.Size
                        IsFreeze      = $pe.IsFreeze
                        IsCrossVersion = $(if ($pe.IsCrossVersion) { $true } else { $false })
                        FromVersion   = $pe.FromVersion
                        ToVersion     = $pe.ToVersion
                    }
                }
            }
        }
        else {
            Write-Host "    [Skip] PatchGenerator project not found at $patchGeneratorProj" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "  [Skip] content_client directories not found: $clientV1 or $clientV2" -ForegroundColor Yellow
}

# ===================================================================
# UPGRADE packages (AppType=2)
#   Current version: 1.0.0.1  →  Target version: 2.0.0.0
# ===================================================================
Write-Host ""
Write-Host "===== UPGRADE Packages (AppType=2) =====" -ForegroundColor Green

$upgradeV1 = Join-Path $contentUpgradeDir "v1.0.0.0"
$upgradeV2 = Join-Path $contentUpgradeDir "v2.0.0.0"

if ((Test-Path $upgradeV2) -and (Test-Path $upgradeV1)) {
    # Full (VersionChain) package
    $pkgName = "packet_${timestamp}_full_upgrade_2.0.0.0"
    Add-ZipPackage -SourceDir $upgradeV2 -PacketName $pkgName `
        -Version "2.0.0.0" -AppType 2 -IsCrossVersion $false
}
else {
    Write-Host "  [Skip] content_upgrade directories not found: $upgradeV1 or $upgradeV2" -ForegroundColor Yellow
}

# ===================================================================
# Also generate smaller mock packages from content/ dirs
# (for quick testing without large binary packages)
# ===================================================================
Write-Host ""
Write-Host "===== MOCK Content Packages =====" -ForegroundColor Green

$contentDir = Join-Path $srcDir "content"

foreach ($ver in @("1.0.0.1", "1.0.0.2")) {
    $srcPath = Join-Path $contentDir "v$ver"
    if (Test-Path $srcPath) {
        # Client full package
        $pkgName = "packet_${timestamp}_full_client_$ver"
        Add-ZipPackage -SourceDir $srcPath -PacketName $pkgName `
            -Version $ver -AppType 1 -IsCrossVersion $false

        # Upgrade full package
        $pkgName = "packet_${timestamp}_full_upgrade_$ver"
        Add-ZipPackage -SourceDir $srcPath -PacketName $pkgName `
            -Version $ver -AppType 2 -IsCrossVersion $false
    }
}

# ===================================================================
# Write versions.json
# ===================================================================
# Deduplicate entries (PatchGenerator may have added some already)
$uniqueVersions = $allVersions | Sort-Object PacketName -Unique
$versionsJsonPath = Join-Path $packagesDir "versions.json"
$json = $uniqueVersions | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText($versionsJsonPath, $json, [System.Text.Encoding]::UTF8)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Generated $($uniqueVersions.Count) package(s)" -ForegroundColor Cyan
Write-Host "  Output: $packagesDir" -ForegroundColor Cyan
Write-Host "  Metadata: $versionsJsonPath" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

foreach ($v in $uniqueVersions) {
    $cross = if ($v.IsCrossVersion) { "Cross $($v.FromVersion) → $($v.ToVersion)" } else { "Full" }
    Write-Host "  AppType=$($v.AppType) v$($v.Version) [$cross] $($v.PacketName)" -ForegroundColor Gray
}
