param(
    [string]$Runtime = "win-x64"
)

$projectRoot = $PSScriptRoot
$sourceDir = Join-Path $projectRoot "source"
$projectFile = Join-Path $sourceDir "PagerTracker.csproj"
$publishDir = Join-Path $projectRoot "publish"
$exePath = Join-Path $publishDir "PagerTracker.exe"
$configPath = Join-Path $publishDir "PagerTracker.json"

function Remove-BuildFolders {
    $folders = @(
        (Join-Path $sourceDir "bin"),
        (Join-Path $sourceDir "obj")
    )

    foreach ($folder in $folders) {
        if (Test-Path -LiteralPath $folder) {
            Remove-Item -LiteralPath $folder -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

$env:MSBUILDDISABLENODEREUSE = "1"
& dotnet publish $projectFile -c Release -r $Runtime --self-contained true -o $publishDir
$publishExitCode = $LASTEXITCODE

if ($publishExitCode -ne 0) {
    Remove-BuildFolders
    throw "dotnet publish failed with exit code $publishExitCode."
}

if (-not (Test-Path $exePath)) {
    Remove-BuildFolders
    throw "Publish finished, but PagerTracker.exe was not found in $publishDir."
}

Remove-BuildFolders

Get-ChildItem -LiteralPath $publishDir -Force |
    Where-Object { $_.FullName -ne $exePath -and $_.FullName -ne $configPath } |
    Remove-Item -Recurse -Force

$exe = Get-Item $exePath
$sizeMb = [Math]::Round($exe.Length / 1MB, 2)

Write-Host ""
Write-Host "Build complete." -ForegroundColor Green
Write-Host "Output: $exePath"
Write-Host "Size: $sizeMb MB"
Write-Host ""
Write-Host "Run default port 80, with automatic fallback to 5000-5099:"
Write-Host "  .\publish\PagerTracker.exe"
Write-Host ""
Write-Host "Run a custom port:"
Write-Host "  .\publish\PagerTracker.exe 8080"
