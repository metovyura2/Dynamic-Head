<#
    Сборка и публикация Dynamic-Head.

    Требуется .NET SDK 8. На этой машине он установлен в D:\dotnet
    (официальный установщик .exe не позволяет ставить SDK не на диск C:
    из-за non-overridable переменной DOTNETHOME, поэтому используется
    скрипт dotnet-install.ps1 с параметром -InstallDir).
#>

param(
    [string]$Configuration = 'Release',
    [string]$OutputDir = ''
)

$ErrorActionPreference = 'Stop'

# Корень проекта — на уровень выше папки tools
$root    = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\DynamicHead\DynamicHead.csproj'
$dist    = if ($OutputDir) { $OutputDir } else { Join-Path $root 'dist' }

# Ищем dotnet: сначала в D:\dotnet, затем в PATH
$dotnet = 'D:\dotnet\dotnet.exe'
if (-not (Test-Path $dotnet)) {
    $dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
}
if (-not $dotnet) {
    throw 'Не найден dotnet. Установите .NET SDK 8 в D:\dotnet.'
}

$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

Write-Host "Сборка через: $dotnet" -ForegroundColor Cyan

& $dotnet restore $project
if ($LASTEXITCODE -ne 0) { throw 'Ошибка восстановления пакетов.' }

& $dotnet build $project -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Ошибка сборки.' }

& $dotnet publish $project -c $Configuration -r win-x64 --self-contained false --no-restore -o $dist
if ($LASTEXITCODE -ne 0) { throw 'Ошибка публикации.' }

Write-Host "Готово: $dist\DynamicHead.exe" -ForegroundColor Green
