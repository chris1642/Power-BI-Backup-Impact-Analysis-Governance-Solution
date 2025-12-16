# Power BI Governance - Build Script
# This script builds the single-file executable for the Power BI Governance application

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "  Power BI Governance - Building Application" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptPath "PowerBIGovernanceApp"

Set-Location $projectPath

Write-Host "Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path "bin\Release") { Remove-Item -Path "bin\Release" -Recurse -Force }
if (Test-Path "obj") { Remove-Item -Path "obj" -Recurse -Force }

Write-Host ""
Write-Host "Building single-file executable..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=false

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "===================================================" -ForegroundColor Green
    Write-Host "  BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "===================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Executable location:" -ForegroundColor White
    Write-Host "  $projectPath\bin\Release\net8.0-windows\win-x64\publish\PowerBIGovernance.exe" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "===================================================" -ForegroundColor Green
    
    $exePath = Join-Path $projectPath "bin\Release\net8.0-windows\win-x64\publish\PowerBIGovernance.exe"
    $destPath = Join-Path $scriptPath "PowerBIGovernance.exe"
    
    if (Test-Path $exePath) {
        Copy-Item -Path $exePath -Destination $destPath -Force
        Write-Host ""
        Write-Host "  Copied to: $destPath" -ForegroundColor Green
    }
} else {
    Write-Host ""
    Write-Host "===================================================" -ForegroundColor Red
    Write-Host "  BUILD FAILED!" -ForegroundColor Red
    Write-Host "===================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Please ensure you have .NET 8.0 SDK installed." -ForegroundColor Yellow
    Write-Host "  Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
}

Write-Host ""
Read-Host "Press Enter to exit"
