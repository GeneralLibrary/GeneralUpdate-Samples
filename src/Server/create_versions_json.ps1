# Creates versions.json from the .zip files in wwwroot/packages
$packagesDir = Join-Path $PSScriptRoot "wwwroot\packages"
$baseUrl = "http://localhost:5000/"
$productId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a"

# Map filenames to metadata
$packages = @(
    @{Name="packet_*_full_client_2.0.0.0.zip";     Ver="2.0.0.0"; AppType=1; IsCross=$false; From=$null; To=$null},
    @{Name="packet_*_full_client_1.0.0.2.zip";     Ver="1.0.0.2"; AppType=1; IsCross=$false; From=$null; To=$null},
    @{Name="packet_*_full_client_1.0.0.1.zip";     Ver="1.0.0.1"; AppType=1; IsCross=$false; From=$null; To=$null},
    @{Name="packet_*_full_upgrade_2.0.0.0.zip";    Ver="2.0.0.0"; AppType=2; IsCross=$false; From=$null; To=$null},
    @{Name="packet_*_full_upgrade_1.0.0.2.zip";    Ver="1.0.0.2"; AppType=2; IsCross=$false; From=$null; To=$null},
    @{Name="packet_*_full_upgrade_1.0.0.1.zip";    Ver="1.0.0.1"; AppType=2; IsCross=$false; From=$null; To=$null},
    @{Name="packet_*_cross_client_*.zip";           Ver="2.0.0.0"; AppType=1; IsCross=$true;  From="1.0.0";   To="2.0.0.0"},
    @{Name="packet_*_cross_upgrade_*.zip";          Ver="2.0.0.0"; AppType=2; IsCross=$true;  From="1.0.0.1"; To="2.0.0.0"}
)

$entries = @()
foreach ($p in $packages) {
    $pattern = $p.Name
    $files = Get-ChildItem -Path $packagesDir -Filter $pattern -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        # Skip non-.zip or the old patch_*.zip
        if ($file.Extension -ne ".zip") { continue }
        if ($file.Name -like "patch_*") { continue }

        $hash = (Get-FileHash -Path $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        $size = $file.Length
        $name = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)

        # Extract FromVersion/ToVersion from cross-version filenames
        $fromVer = $p.From
        $toVer = $p.To
        if ($p.IsCross -and $file.Name -match "_([^_]+)_to_") {
            $fromVer = $Matches[1]
        }

        $entry = [PSCustomObject]@{
            PacketName     = $name
            Hash           = $hash
            Version        = $p.Ver
            Url            = "$baseUrl`File/Download/$hash"
            PubTime        = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
            AppType        = $p.AppType
            Platform       = 1
            ProductId      = $productId
            IsForcibly     = $false
            Format         = ".zip"
            Size           = $size
            IsFreeze       = $false
            IsCrossVersion = $p.IsCross
            FromVersion    = $fromVer
            ToVersion      = $toVer
        }
        $entries += $entry
        $crossLabel = if ($p.IsCross) { "Cross $fromVer -> $toVer" } else { "Full" }
        Write-Host "  AppType=$($p.AppType) v$($p.Ver) [$crossLabel] $($file.Name) ($size bytes)"
    }
}

if ($entries.Count -eq 0) {
    Write-Host "[Error] No .zip packages found in $packagesDir" -ForegroundColor Red
    exit 1
}

$json = ConvertTo-Json -InputObject @($entries) -Depth 5
$versionsPath = Join-Path $packagesDir "versions.json"
[System.IO.File]::WriteAllText($versionsPath, $json, [System.Text.UTF8Encoding]::new($false))
Write-Host "`nGenerated $($entries.Count) entries -> $versionsPath" -ForegroundColor Green
