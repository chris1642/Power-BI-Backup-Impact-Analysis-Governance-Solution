# Application Icon

To add a custom icon to the Power BI Governance application:

## Quick Method: Use an existing icon

1. Find or create a `.ico` file with your desired icon
2. Name it `app.ico`
3. Place it in the `PowerBIGovernanceApp` folder
4. The project is already configured to use it (see `ApplicationIcon` in the `.csproj` file)

## Creating an Icon

You can create an icon using:
- **Online tools**: 
  - https://www.icoconverter.com/
  - https://convertico.com/
- **Desktop software**:
  - GIMP (free)
  - Adobe Photoshop
  - Icon editors like IcoFX

## Icon Specifications

For best results, create a multi-resolution icon with these sizes:
- 16x16
- 32x32
- 48x48
- 64x64
- 128x128
- 256x256

## Recommended Design

For a Power BI Governance application, consider:
- Use Power BI brand colors (yellow #F2C811, blue #3174CF)
- Include data/governance imagery (graphs, charts, shield/security icon)
- Keep it simple and recognizable at small sizes
- Ensure good contrast for visibility

## Current Status

The application currently compiles without a custom icon. Windows will use a default .NET application icon until you add `app.ico` to the `PowerBIGovernanceApp` folder.

## Note

The icon file should NOT be committed to the repository if it contains proprietary branding. Consider adding `app.ico` to `.gitignore` if needed.
