@echo off
setlocal enableextensions
REM ======= Config =======
set "REPO_OWNER=chris1642"
set "REPO_NAME=Power-BI-Backup-Impact-Analysis-Governance-Solution"
set "BRANCH=main"
set "ZIP_URL=https://github.com/%REPO_OWNER%/%REPO_NAME%/archive/refs/heads/%BRANCH%.zip"
REM Install to C:\ - use absolute path
set "TARGET_DIR=C:\Power BI Backups"
REM Temp working paths
set "TMP_DIR=%TEMP%\pbi_gov_%RANDOM%%RANDOM%"
set "ZIP_FILE=%TMP_DIR%\repo.zip"
set "UNZIP_DIR=%TMP_DIR%\unzipped"
mkdir "%TMP_DIR%" >nul 2>&1

REM ======= Create target directory =======
if exist "%TARGET_DIR%" (
  echo [INFO] Target directory exists: "%TARGET_DIR%"
  echo [INFO] Existing files will be overwritten.
) else (
  mkdir "%TARGET_DIR%" >nul 2>&1
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

REM ======= Cleanup temp =======
rmdir /s /q "%TMP_DIR%" >nul 2>&1

REM ======= Locate Final Script in repo root =======
set "FINAL_TXT=%TARGET_DIR%\Final PS Script.txt"
set "PBIT_FILE=%TARGET_DIR%\Power BI Governance Model.pbit"

REM ======= Run PowerShell script inline (no file renaming needed) =======
if exist "%FINAL_TXT%" (
  echo [INFO] Running Final Script content inline: "%FINAL_TXT%"
  echo [INFO] Please wait for the PowerShell script to complete...
  echo [DEBUG] Current directory: %CD%
  echo [DEBUG] PowerShell working directory will be: "%TARGET_DIR%"
  cd /d "%TARGET_DIR%"
  
  REM Execute PowerShell script content inline
  powershell -ExecutionPolicy Bypass -NoProfile -Command "& { Set-Location '%TARGET_DIR%'; $content = Get-Content '%FINAL_TXT%' -Raw; Invoke-Expression $content }"
  
  echo [INFO] PowerShell script finished.
) else (
  echo [ERROR] Final PS Script not found after installation.
  echo [DEBUG] Looking for: "%FINAL_TXT%"
  echo [DEBUG] Target directory: "%TARGET_DIR%"
  pause
)

REM ======= Open Power BI Template =======
if exist "%PBIT_FILE%" (
  echo [INFO] Opening Power BI template: "%PBIT_FILE%"
  start "" "%PBIT_FILE%"
) else (
  echo [WARNING] Power BI template file not found: "%PBIT_FILE%"
)

echo [INFO] Installation complete.
exit /b 0
