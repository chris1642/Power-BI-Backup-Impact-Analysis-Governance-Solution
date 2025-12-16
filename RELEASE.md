# Creating a Release for the GUI Application

After building the application, follow these steps to create a GitHub release:

## Step 1: Build the Application

Run the build script to create the executable:
```bash
.\Build-Application.bat
# or
.\Build-Application.ps1
```

This creates `PowerBIGovernance.exe` in the root directory.

## Step 2: Create a GitHub Release

1. Go to the GitHub repository
2. Click on "Releases" in the right sidebar
3. Click "Draft a new release"
4. Choose a tag version (e.g., `v2.0.0`)
5. Set the release title (e.g., "Power BI Governance v2.0 - GUI Application")
6. Add release notes describing the new GUI feature
7. Upload `PowerBIGovernance.exe` as an asset
8. Publish the release

## Step 3: Update Download Link

The README already includes a link to:
```
https://github.com/chris1642/Power-BI-Backup-Impact-Analysis-Governance-Solution/releases/latest/download/PowerBIGovernance.exe
```

This link will automatically point to the latest release's executable.

## Release Notes Template

```markdown
## Power BI Governance Solution v2.0 - GUI Application

### 🎉 Major Update: Modern GUI Application!

We've created a beautiful, user-friendly GUI application that makes running the Power BI Governance Solution easier than ever!

**What's New:**
- ✨ Modern, professional Windows application with gradient design
- 📊 Real-time progress tracking and live log viewer
- 🌍 Easy environment selection for all Power BI clouds
- 📦 Single executable - no installation required!
- ✅ One-click button to open your results

**Download:**
- [PowerBIGovernance.exe](link-to-exe) - Single-file GUI application

**How to Use:**
1. Download PowerBIGovernance.exe
2. Double-click to run (no installation needed!)
3. Select your Power BI environment
4. Click "START GOVERNANCE PROCESS"
5. View results when complete!

**Alternative Options:**
- One-Click Batch Script (existing)
- Manual Setup (existing)

All three options work great - choose what works best for you!
```

## File Size Note

The executable will be approximately 80-100MB because it's self-contained and includes the entire .NET runtime. This is intentional and ensures it works on any Windows machine without requiring .NET to be installed.
