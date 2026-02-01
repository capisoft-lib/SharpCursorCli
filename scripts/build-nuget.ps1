# Build NuGet packages and increment the patch (smallest) version.
# Run from repository root. Output: nupkgs\

$ErrorActionPreference = "Stop"
$propsPath = Join-Path $PSScriptRoot "..\Directory.Build.props"
$nupkgsPath = Join-Path $PSScriptRoot "..\nupkgs"
$slnPath = Join-Path $PSScriptRoot "..\SharpCursorCli.sln"

if (-not (Test-Path $propsPath)) {
    Write-Error "Directory.Build.props not found at $propsPath"
}
if (-not (Test-Path $slnPath)) {
    Write-Error "SharpCursorCli.sln not found at $slnPath"
}

$content = Get-Content $propsPath -Raw
if ($content -notmatch '<Version>(\d+)\.(\d+)\.(\d+)</Version>') {
    Write-Error "Could not find <Version>X.Y.Z</Version> in Directory.Build.props"
}
$major = [int]$Matches[1]
$minor = [int]$Matches[2]
$patch = [int]$Matches[3]
$patch++
$newVersion = "$major.$minor.$patch"

$content = $content -replace '<Version>\d+\.\d+\.\d+</Version>', "<Version>$newVersion</Version>"
Set-Content -Path $propsPath -Value $content
Write-Host "Version updated to $newVersion" -ForegroundColor Green

New-Item -ItemType Directory -Force -Path $nupkgsPath | Out-Null
Push-Location (Join-Path $PSScriptRoot "..")
try {
    dotnet pack $slnPath -c Release -o nupkgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }
} finally {
    Pop-Location
}

Get-ChildItem $nupkgsPath -Filter "*.nupkg" | ForEach-Object { Write-Host "  $($_.Name)" }
Write-Host "Done. Packages in nupkgs\" -ForegroundColor Green
