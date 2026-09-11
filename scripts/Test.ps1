$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
dotnet test "$root\tests\IceSkates.Core.Tests\IceSkates.Core.Tests.csproj"
if ($LASTEXITCODE -ne 0) { throw "Tests failed with exit code $LASTEXITCODE" }
