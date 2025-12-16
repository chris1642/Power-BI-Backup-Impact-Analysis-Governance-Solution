@echo off
echo ===================================================
echo   Power BI Governance - Building Application
echo ===================================================
echo.

cd /d "%~dp0PowerBIGovernanceApp"

echo Cleaning previous build...
if exist bin\Release rd /s /q bin\Release
if exist obj rd /s /q obj

echo.
echo Building single-file executable...
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=false

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ===================================================
    echo   BUILD SUCCESSFUL!
    echo ===================================================
    echo.
    echo   Executable location:
    echo   %~dp0PowerBIGovernanceApp\bin\Release\net8.0-windows\win-x64\publish\PowerBIGovernance.exe
    echo.
    echo ===================================================
    if exist "bin\Release\net8.0-windows\win-x64\publish\PowerBIGovernance.exe" (
        copy /Y "bin\Release\net8.0-windows\win-x64\publish\PowerBIGovernance.exe" "..\PowerBIGovernance.exe"
        echo.
        echo   Copied to: %~dp0PowerBIGovernance.exe
    )
) else (
    echo.
    echo ===================================================
    echo   BUILD FAILED!
    echo ===================================================
    echo.
    echo   Please ensure you have .NET 8.0 SDK installed.
    echo   Download from: https://dotnet.microsoft.com/download/dotnet/8.0
)

echo.
pause
