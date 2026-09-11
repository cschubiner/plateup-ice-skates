param(
    [string]$Configuration = "Release",
    [string]$PlateUpGameFolder,
    [string]$KitchenLibWorkshopDll,
    [string]$HarmonyWorkshopDll
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$buildArgs = @('build', "$root\src\IceSkates.Workshop\IceSkates.Workshop.csproj", '-c', $Configuration, '-p:EnableModDeployLocal=true')
# Omitted arguments allow Local.props and Directory.Build.props to supply their defaults.
foreach ($name in @('PlateUpGameFolder', 'KitchenLibWorkshopDll', 'HarmonyWorkshopDll')) {
    if ($PSBoundParameters.ContainsKey($name)) { $buildArgs += "-p:${name}=$($PSBoundParameters[$name])" }
}
dotnet @buildArgs
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }
