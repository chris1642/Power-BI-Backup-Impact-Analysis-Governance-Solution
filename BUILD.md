# Building the GUI Application

This guide explains how to build the Power BI Governance GUI application from source.

## Prerequisites

- Windows 10/11
- .NET 8.0 SDK ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- Git (optional, for cloning the repository)

## Quick Build

### Using the Build Scripts (Easiest)

1. Clone or download this repository
2. Navigate to the repository root folder
3. Run one of the build scripts:
   - **Windows Command Prompt**: Double-click `Build-Application.bat`
   - **PowerShell**: Right-click `Build-Application.ps1` and select "Run with PowerShell"

The script will:
- Clean any previous builds
- Build a self-contained, single-file executable
- Copy the executable to the root directory as `PowerBIGovernance.exe`

### Manual Build

If you prefer to build manually:

```bash
cd PowerBIGovernanceApp
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=false
```

The executable will be located at:
```
PowerBIGovernanceApp\bin\Release\net8.0-windows\win-x64\publish\PowerBIGovernance.exe
```

## Publishing for Distribution

The build process creates a fully self-contained executable that includes:
- The .NET 8.0 runtime
- All dependencies
- Embedded resources (PowerShell script, Config files, Power BI template)

The single executable can be distributed without any installation or dependencies!

## Build Configuration

The application is configured to:
- Target Windows x64 architecture
- Include all native libraries
- Embed all resources into the executable
- Create a single-file publish

See `PowerBIGovernanceApp/PowerBIGovernanceApp.csproj` for full configuration details.

## Troubleshooting

### "dotnet command not found"
Install the .NET 8.0 SDK from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build errors about missing resources
Ensure all Config files and resources are present in the repository before building.

### The executable is very large
This is normal! The self-contained build includes the entire .NET runtime (~80-100MB). This ensures the app works on any Windows machine without requiring .NET to be installed.

## Development

To run the application in development mode:

```bash
cd PowerBIGovernanceApp
dotnet run
```

This is faster for testing but requires .NET SDK to be installed.
