param([string]$PlateUpGameFolder = 'C:\Program Files (x86)\Steam\steamapps\common\PlateUp\PlateUp')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$game = (Resolve-Path -LiteralPath $PlateUpGameFolder).Path
if (!(Test-Path -LiteralPath (Join-Path $game 'PlateUp.exe'))) { throw 'Not a PlateUp game folder.' }
if (Get-Process PlateUp -ErrorAction SilentlyContinue) { throw 'Close PlateUp before installing.' }
$mods = Join-Path $game 'Mods'
$target = Join-Path $mods 'IceSkates\content'
$files = @('IceSkates.Workshop-Workshop.dll', 'IceSkates.Core.dll', 'plateup_mod_metadata.json')
foreach ($name in $files) {
    if (!(Test-Path -LiteralPath (Join-Path $root "content\$name"))) { throw "Missing build output: $name" }
}
# Preserve prior installs outside the loader directory. Only replace this mod's known files.
$backup = Join-Path $root ('artifacts\install-backup-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
foreach ($folder in @('IceSkates', 'IceSkates.Workshop')) {
    $candidate = Join-Path $mods $folder
    if (Test-Path -LiteralPath $candidate) {
        $resolved = (Resolve-Path -LiteralPath $candidate).Path
        if ([IO.Path]::GetDirectoryName($resolved) -ne $mods) { throw "Unexpected install path: $resolved" }
        if ((Get-Item -LiteralPath $candidate).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Refusing to move linked mod folder: $candidate" }
        New-Item -ItemType Directory -Path $backup -Force | Out-Null
        Move-Item -LiteralPath $resolved -Destination (Join-Path $backup $folder)
    }
}
New-Item -ItemType Directory -Path $target -Force | Out-Null
foreach ($name in $files) {
    $source = Join-Path $root "content\$name"
    $dest = Join-Path $target $name
    Copy-Item -LiteralPath $source -Destination $dest -Force
    if ((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $dest).Hash) { throw "Install hash mismatch: $name" }
}
Write-Host "Installed and SHA256-verified Ice Skates at $target"
