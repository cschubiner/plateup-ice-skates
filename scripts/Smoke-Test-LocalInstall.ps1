param(
    [string]$PlateUpGameFolder = "C:\Program Files (x86)\Steam\steamapps\common\PlateUp\PlateUp"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$target = Join-Path $PlateUpGameFolder "Mods\IceSkates\content"
$required = @(
    "IceSkates.Workshop-Workshop.dll",
    "IceSkates.Core.dll",
    "plateup_mod_metadata.json"
)

foreach ($file in $required) {
    $path = Join-Path $target $file
    if (!(Test-Path -LiteralPath $path)) {
        throw "Missing local mod file: $path"
    }

    $item = Get-Item -LiteralPath $path
    if ($item.Length -le 0) {
        throw "Local mod file is empty: $path"
    }
    $source = Join-Path $root "content\$file"
    if ((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $path).Hash) {
        throw "Installed file differs from current build: $path"
    }
}

$stale = Join-Path $PlateUpGameFolder "Mods\IceSkates.Workshop"
if (Test-Path -LiteralPath $stale) {
    throw "Stale duplicate Ice Skates mod folder still exists: $stale"
}

Write-Host "Local Ice Skates install looks complete at $target"
