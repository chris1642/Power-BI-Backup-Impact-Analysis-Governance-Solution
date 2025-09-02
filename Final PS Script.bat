@echo off
setlocal enableextensions
REM ======= Config =======
set "REPO_OWNER=chris1642"
set "REPO_NAME=Power-BI-Backup-Impact-Analysis-Governance-Solution"
set "BRANCH=main"
set "ZIP_URL=https://github.com/%REPO_OWNER%/%REPO_NAME%/archive/refs/heads/%BRANCH%.zip"
REM Install to Desktop by default
set "PREFERRED_TARGET=\Power BI Backups"
set "FALLBACK_TARGET=\Power BI Backups"
REM Temp working paths
set "TMP_DIR=%TEMP%\pbi_gov_%RANDOM%%RANDOM%"
set "ZIP_FILE=%TMP_DIR%\repo.zip"
set "UNZIP_DIR=%TMP_DIR%\unzipped"
mkdir "%TMP_DIR%" >nul 2>&1

REM ======= Pick install location =======
set "TARGET_DIR=%PREFERRED_TARGET%"

REM Check if target directory exists and warn about overwrite
if exist "%TARGET_DIR%" (
  echo [INFO] Target directory exists: "%TARGET_DIR%"
  echo [INFO] Existing files will be overwritten.
) else (
  mkdir "%TARGET_DIR%" >nul 2>&1
  if errorlevel 1 (
    echo [INFO] Desktop not writable. Using fallback: "%FALLBACK_TARGET%"
    set "TARGET_DIR=%FALLBACK_TARGET%"
    if exist "%TARGET_DIR%" (
      echo [INFO] Fallback directory exists: "%TARGET_DIR%"
      echo [INFO] Existing files will be overwritten.
    ) else (
      mkdir "%TARGET_DIR%" >nul 2>&1
    )
  )
)
echo [INFO] Installing to: "%TARGET_DIR%"

REM ======= Download repo ZIP =======
echo [INFO] Downloading repository...
where curl >nul 2>&1
if %errorlevel%==0 (
  curl -L -o "%ZIP_FILE%" "%ZIP_URL%"
) else (
  powershell -NoProfile -Command "Invoke-WebRequest -Uri '%ZIP_URL%' -OutFile '%ZIP_FILE%' -UseBasicParsing"
)

REM ======= Unzip =======
echo [INFO] Extracting files...
powershell -NoProfile -Command "Expand-Archive -Path '%ZIP_FILE%' -DestinationPath '%UNZIP_DIR%' -Force"

for /d %%D in ("%UNZIP_DIR%\%REPO_NAME%-%BRANCH%") do (
  set "TOPDIR=%%~fD"
)

REM ======= Copy files with forced overwrite =======
echo [INFO] Copying files and overwriting existing ones...
xcopy "%TOPDIR%\*" "%TARGET_DIR%\" /e /i /y /q >nul

REM Alternative method using robocopy for more reliable overwriting (uncomment if needed)
REM robocopy "%TOPDIR%" "%TARGET_DIR%" /e /is /it /np /nfl /ndl >nul

REM ======= Cleanup temp =======
rmdir /s /q "%TMP_DIR%" >nul 2>&1

REM ======= Locate Final Script in repo root =======
set "FINAL_TXT=%TARGET_DIR%\Final PS Script.txt"
set "FINAL_PS1=%TARGET_DIR%\Final PS Script.ps1"

if exist "%FINAL_TXT%" (
  echo [INFO] Found Final Script as TXT. Renaming to PS1...
  if exist "%FINAL_PS1%" del /f /q "%FINAL_PS1%" >nul 2>&1
  ren "%FINAL_TXT%" "Final PS Script.ps1"
)

REM ======= Replace hard-coded path with $PSScriptRoot =======
if exist "%FINAL_PS1%" (
  echo [INFO] Updating baseFolderPath to $PSScriptRoot...
  powershell -NoProfile -Command "& {$c = Get-Content '%FINAL_PS1%' -Raw; $c = $c -replace [regex]::Escape('$baseFolderPath = \"C:\Power BI Backups\"'), '$baseFolderPath = $PSScriptRoot'; Set-Content '%FINAL_PS1%' -Value $c -NoNewline}"
)

REM ======= Run in same window =======
if exist "%FINAL_PS1%" (
  echo [INFO] Running Final Script: "%FINAL_PS1%"
  powershell -NoExit -ExecutionPolicy Bypass -File "%FINAL_PS1%"
  echo [INFO] Script finished.
) else (
  echo [ERROR] Final PS Script not found after installation.
  pause
)

REM ======= Restore original hard-coded path =======
if exist "%FINAL_PS1%" (
  echo [INFO] Restoring baseFolderPath back to C:\Power BI Backups...
  powershell -NoProfile -Command "& {$c = Get-Content '%FINAL_PS1%' -Raw; $c = $c -replace [regex]::Escape('$baseFolderPath = $PSScriptRoot'), '$baseFolderPath = \"C:\Power BI Backups\"'; Set-Content '%FINAL_PS1%' -Value $c -NoNewline}"
  if exist "%TARGET_DIR%\Final PS Script.txt" del /f /q "%TARGET_DIR%\Final PS Script.txt" >nul 2>&1
  ren "%FINAL_PS1%" "Final PS Script.txt"
)

echo [INFO] Installation complete.
exit /b 0