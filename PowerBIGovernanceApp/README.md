# Power BI Governance Application

This folder contains the C# Windows Forms application that provides a modern, user-friendly GUI for the Power BI Governance Solution.

## Building the Application

### Prerequisites
- .NET 8.0 SDK or later
- Windows Operating System

### Build Instructions

#### Option 1: Using the Build Script (Recommended)
Simply run one of the build scripts in the parent directory:
- **Build-Application.bat** (for Command Prompt)
- **Build-Application.ps1** (for PowerShell)

The script will:
1. Clean previous builds
2. Build a self-contained, single-file executable
3. Copy the executable to the root directory as `PowerBIGovernance.exe`

#### Option 2: Manual Build
```bash
cd PowerBIGovernanceApp
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The executable will be located at:
```
PowerBIGovernanceApp\bin\Release\net8.0-windows\win-x64\publish\PowerBIGovernance.exe
```

## Features

- **Modern UI**: Clean, professional interface with gradient headers and rounded panels
- **Environment Selection**: Choose from various Power BI cloud environments
- **Real-time Logging**: View script progress in real-time
- **Progress Tracking**: Visual progress bar during execution
- **Single File**: All dependencies embedded - no installation required
- **Self-contained**: Includes .NET runtime - works on any Windows machine

## Usage

1. Run `PowerBIGovernance.exe`
2. Select your Power BI environment
3. Click "START GOVERNANCE PROCESS"
4. Wait for completion
5. Click "Open Power BI Governance Model" to view results

All backups and outputs are saved to: `C:\Power BI Backups`
