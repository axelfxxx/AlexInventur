param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $root "publish\$Runtime"

Write-Host "Publishing Alex Inventur ($Configuration / $Runtime)..." -ForegroundColor Cyan
Remove-Item $publishDir -Recurse -Force -ErrorAction SilentlyContinue

dotnet publish (Join-Path $root "AlexInventur.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    /p:PublishSingleFile=false `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishReadyToRun=true `
    -o $publishDir

Write-Host "Publish fertig: $publishDir" -ForegroundColor Green
Write-Host "Optional: Inno Setup öffnen und Installer\AlexInventur.iss kompilieren." -ForegroundColor Yellow
