param(
    [Parameter(Mandatory = $true)]
    [string]$LatestVersion,

    [Parameter(Mandatory = $true)]
    [string]$DownloadUrl,

    [string]$ReleaseNotes = "Korrekturen und Verbesserungen.",

    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $root "Releases\update.json"
}

$manifest = [ordered]@{
    latestVersion = $LatestVersion
    downloadUrl   = $DownloadUrl
    publishedAt   = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    releaseNotes  = $ReleaseNotes
}

$dir = Split-Path -Parent $OutputPath
if (-not (Test-Path $dir)) {
    New-Item -ItemType Directory -Path $dir | Out-Null
}

$manifest | ConvertTo-Json -Depth 5 | Set-Content -Path $OutputPath -Encoding UTF8
Write-Host "Update-Manifest erstellt:" -ForegroundColor Green
Write-Host $OutputPath
