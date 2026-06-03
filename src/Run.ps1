<#
.SYNOPSIS
    GeneralUpdate Sample Hub Launcher

.DESCRIPTION
    One-click launcher for the interactive sample menu.
    Add -BuildLibs to recompile component DLLs from source.

.PARAMETER BuildLibs
    Rebuild all component DLLs from source and copy to Hub/libs/

.PARAMETER GeneralUpdateSrc
    Path to GeneralUpdate source root (default: auto-detect sibling repo)

.EXAMPLE
    .\Run.ps1                              # Normal use
    .\Run.ps1 -BuildLibs                    # Rebuild DLLs after component code changes
    .\Run.ps1 -BuildLibs -GeneralUpdateSrc "D:\repos\GeneralUpdate\src\c#"
#>

param(
    [switch]$BuildLibs,
    [string]$GeneralUpdateSrc
)

$ErrorActionPreference = "Stop"
# Script is in src/ — Hub is in src/Hub/
$hubDir = Join-Path $PSScriptRoot "Hub"

# ── Optional: Rebuild component DLLs from source ──
if ($BuildLibs) {
    Write-Host "[BuildLibs] Building components and copying DLLs..." -ForegroundColor Cyan

    # Auto-detect GeneralUpdate source root if not provided.
    # Default: sibling repo at ../GeneralUpdate/src/c# relative to the Samples repo root.
    if (-not $GeneralUpdateSrc) {
        $samplesRepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
        $candidate = Join-Path $samplesRepoRoot "..\GeneralUpdate\src\c#"
        if (Test-Path $candidate) {
            $GeneralUpdateSrc = $candidate
        }
        else {
            Write-Host "  [ERROR] Cannot find GeneralUpdate source. Provide -GeneralUpdateSrc <path>." -ForegroundColor Red
            Write-Host "  Expected sibling repo at: $candidate" -ForegroundColor Yellow
            exit 1
        }
    }
    $sourceRoot = $GeneralUpdateSrc
    Write-Host "  Source: $sourceRoot" -ForegroundColor Gray
    $libsDir     = Join-Path $hubDir "libs"

    Remove-Item -Recurse -Force $libsDir -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force $libsDir | Out-Null

    $projects = @(
        "GeneralUpdate.Differential",
        "GeneralUpdate.Core",
        "GeneralUpdate.Bowl",
        "GeneralUpdate.Extension",
        "GeneralUpdate.Drivelution"
    )

    foreach ($proj in $projects) {
        $projPath = Join-Path $sourceRoot $proj
        Write-Host "  Building $proj ..." -ForegroundColor Gray
        dotnet build $projPath -c Release | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  [ERROR] Build failed: $proj" -ForegroundColor Red
            exit 1
        }

        # Find all GeneralUpdate.*.dll files in the build output (any TFM subfolder)
        $dlls = Get-ChildItem "$projPath\bin\Release" -Recurse -Filter "GeneralUpdate.*.dll" |
                Where-Object { $_.Name -like "GeneralUpdate.*.dll" }
        foreach ($dll in $dlls) {
            Copy-Item $dll.FullName $libsDir -ErrorAction SilentlyContinue
        }
    }

    # Copy Bowl Applications (procdump tool for process monitoring)
    $bowlAppsSrc = Join-Path $sourceRoot "GeneralUpdate.Bowl\Applications"
    $bowlAppsDst = Join-Path $libsDir "Applications"
    if (Test-Path $bowlAppsSrc) {
        Copy-Item -Recurse -Force $bowlAppsSrc $bowlAppsDst
        Write-Host "  Bowl Applications copied" -ForegroundColor Gray
    }

    Write-Host "[BuildLibs] Done — $((Get-ChildItem $libsDir -File).Count) DLLs + Applications" -ForegroundColor Green
    Write-Host ""
}

# ── Launch Hub ──
Push-Location $hubDir
try {
    dotnet run --project Hub.csproj
}
finally {
    Pop-Location
}
