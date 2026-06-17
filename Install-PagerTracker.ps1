param(
    [switch]$Uninstall
)

$serviceName = "PagerTracker"
$displayName = "Pager Tracker"
$installDir = Join-Path $env:ProgramFiles "Pager Tracker"
$sourceDir = Join-Path $PSScriptRoot "publish"
$sourceExe = Join-Path $sourceDir "PagerTracker.exe"
$sourceConfig = Join-Path $sourceDir "PagerTracker.json"
$installExe = Join-Path $installDir "PagerTracker.exe"

function Assert-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)

    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run this script as Administrator."
    }
}

function Stop-PagerTrackerService {
    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($service -and $service.Status -ne "Stopped") {
        Stop-Service -Name $serviceName -Force
        $service.WaitForStatus("Stopped", [TimeSpan]::FromSeconds(20))
    }
}

Assert-Admin

if ($Uninstall) {
    Stop-PagerTrackerService

    if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
        sc.exe delete $serviceName | Out-Null
        Start-Sleep -Seconds 1
    }

    if (Test-Path -LiteralPath $installDir) {
        Remove-Item -LiteralPath $installDir -Recurse -Force
    }

    Write-Host "Pager Tracker service uninstalled."
    exit 0
}

if (-not (Test-Path -LiteralPath $sourceExe)) {
    throw "Build first. Missing $sourceExe"
}

if (-not (Test-Path -LiteralPath $sourceConfig)) {
    throw "Build first. Missing $sourceConfig"
}

Stop-PagerTrackerService

if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    sc.exe delete $serviceName | Out-Null
    Start-Sleep -Seconds 1
}

New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Copy-Item -LiteralPath $sourceExe -Destination $installExe -Force
Copy-Item -LiteralPath $sourceConfig -Destination (Join-Path $installDir "PagerTracker.json") -Force

New-Service `
    -Name $serviceName `
    -DisplayName $displayName `
    -BinaryPathName "`"$installExe`"" `
    -StartupType Automatic `
    -Description "Runs the Pager Tracker web app."

Start-Service -Name $serviceName

Write-Host "Pager Tracker service installed and started."
Write-Host "Installed to: $installDir"
