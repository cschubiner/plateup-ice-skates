param(
    [string]$PlateUpGameFolder = 'C:\Program Files (x86)\Steam\steamapps\common\PlateUp\PlateUp',
    [int]$TimeoutSeconds = 180,
    [switch]$VisualCheck,
    [string[]]$AdditionalArguments = @()
)
$ErrorActionPreference = 'Stop'
if (Get-Process PlateUp -ErrorAction SilentlyContinue) { throw 'Close PlateUp before starting runtime checks.' }
$game = (Resolve-Path -LiteralPath $PlateUpGameFolder).Path
$exe = Join-Path $game 'PlateUp.exe'
if (!(Test-Path -LiteralPath $exe)) { throw 'Not a PlateUp game folder.' }
$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root 'artifacts'
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
$log = Join-Path $artifacts ('runtime-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log')
$launchArgs = @('--iceskates-self-test', '-logFile', ('"' + $log + '"'))
$launchArgs += $AdditionalArguments
if ($VisualCheck) { $launchArgs += '--iceskates-visual-check'; $launchArgs += ('"--iceskates-render-dir=' + (Join-Path $artifacts 'visual-0.3.1') + '"') }
$process = Start-Process -FilePath $exe -WorkingDirectory $game -WindowStyle Hidden -PassThru -ArgumentList $launchArgs
Write-Host "Runtime check PID $($process.Id); log: $log"
try {
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $passed = $false
    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Seconds 2
        $contents = if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Raw } else { '' }
        if ($contents -match '\[Ice Skates\] SELFTEST FAIL:') { throw "Runtime checks failed. See $log" }
        if ($contents -match '\[Ice Skates\] VISUAL CHECK FAIL:') { throw "Visual checks failed. See $log" }
        if ($contents -match '\[Ice Skates\] SELFTEST PASS:' -and (!$VisualCheck -or $contents -match '\[Ice Skates\] VISUAL CHECK PASS:')) { $passed = $true; break }
        if ($process.HasExited) { throw "PlateUp exited before runtime checks completed. See $log" }
    }
    if (!$passed) { throw "Runtime checks did not finish in $TimeoutSeconds seconds. See $log" }
    Start-Sleep -Seconds 10
    Get-Content -LiteralPath $log | Select-String '\[Ice Skates\]'
    Write-Host 'Isolated native checks passed. Live controller feel and multiplayer QA remain separate.'
}
finally {
    # Never close another session: this Process object belongs to this invocation only.
    if (!$process.HasExited) {
        $null = $process.CloseMainWindow()
        if (!$process.WaitForExit(10000)) { $process.Kill(); $process.WaitForExit() }
    }
    $process.Dispose()
}
