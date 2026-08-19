$ErrorActionPreference = "Stop"
$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition

$csproj = Join-Path $PSScriptRoot "ControlLicencias.csproj"
$match = [regex]::Match((Get-Content $csproj -Raw), '<Version>(.*?)</Version>')
$version = if ($match.Success) { $match.Groups[1].Value } else { "1.0" }

$outDir = Join-Path $PSScriptRoot "Instalador\ControlLicencias"
$zip = Join-Path $PSScriptRoot "Instalador\ControlLicencias-v$version.zip"

Write-Host "=== Publicando ControlLicencias v$version (self-contained win-x64) ===" -ForegroundColor Cyan
if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
dotnet publish $csproj -r win-x64 --self-contained true -c Release -o $outDir
if ($LASTEXITCODE -ne 0) { throw "Error en dotnet publish" }

Write-Host "=== Quitando PDBs (no necesarios) ===" -ForegroundColor Yellow
Get-ChildItem -Path $outDir -Filter *.pdb -File -Recurse | Remove-Item -Force

Write-Host "=== Comprimiendo ===" -ForegroundColor Yellow
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path "$outDir\*" -DestinationPath $zip

Write-Host "Listo: $zip" -ForegroundColor Green
