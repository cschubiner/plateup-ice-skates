param(
    [string]$PlateUpGameFolder = "C:\Program Files (x86)\Steam\steamapps\common\PlateUp\PlateUp"
)

$uploader = Join-Path $PlateUpGameFolder "PlateUp_Data\ModUploader.exe"
if (!(Test-Path -LiteralPath $uploader)) {
    throw "ModUploader.exe was not found at $uploader"
}

Start-Process -FilePath $uploader -WorkingDirectory $PlateUpGameFolder
