
## This all-in-one solution is designed to be ran by ANYONE. 
- Everything within the script is limited to your access within the Power BI environment.
- All computer requirements are at the user level and do not require admin privileges.
- There are ZERO pre-reqs. A once-click solution is offered that stays updated with the latest features.
  
*Have specific Reports and/or Models downloaded you want to analyze? Don't have direct access to the Workspace but have the PBIX? Check out the local version here: https://github.com/chris1642/Local-Power-BI-Impact-Analysis-Governance*


# Power BI Governance & Impact Analysis Solution

## What It Does
This provides a quick and automated way to identify where and how specific fields, measures, and tables are used across Power BI reports in all workspaces by analyzing the visual object layer. It also backs up and breaks down the details of your models, reports, and dataflows for easy review, giving you an all-in-one **Power BI Governance** solution.

### Key Features:
- **Impact Analysis**: Fully understand the downstream impact of data model changes, ensuring you don’t accidentally break visuals or dashboards—especially when reports connected to a model span multiple workspaces.
- **Used and Unused Objects**: Identify which tables, columns, and measures are actively used and where. Equally as important, see what isn't used and can be safely removed from your model to save space and complexity.
- **Comprehensive Environment Overview**: Gain a clear, detailed view of your entire Power BI environment, including complete breakdowns of your models, reports, and dataflows and their dependencies. 
- **Backup Solution**: Automatically backs up every model, report, and dataflow for safekeeping.
- **User-Friendly Output**: Results are presented in a Power BI model, making them easy to explore, analyze, and share with your team.

     .

     .

## 🚀 Quick Start Instructions  

You’ve got **two ways** to get started:  

---

### 🟢 Option 1 — One-Click Auto Run (Recommended)  
Always up-to-date and the easiest way to get started.  

➡️ [**Download & Run One-Click Auto Run**](https://github.com/chris1642/Power-BI-Backup-Impact-Analysis-Governance-Solution/releases/download/v1.0/PBIGovernance-UpdateAndRun.bat)  

This automatically:  
1. Pulls the latest repo from GitHub
2. Places it into `C:\Power BI Backups`
3. Runs the **Final PS Script**  
4. Opens the **Power BI Governance Model** at the end  

> 💡 **Tip:** Once downloaded, simply re-run this locally anytime to keep your **backups** and **governance details up-to-date** *and* take advantage of the **newest features**.  

> ⚠️ If security policies block the batch file, follow the manual steps below instead.


📂 **All backups and the final Power BI Governance Model will be saved to:** `C:\Power BI Backups`


---

### 🟡 Option 2 — Manual Setup  

#### ✅ Step 1: Create Folder  
Make a folder at:  `C:\Power BI Backups`  

#### ✅ Step 2: Add Files  
Download all repo files and place them into the newly-created `C:\Power BI Backups` folder.  

#### ✅ Step 3: Run Script  
Open PowerShell and run the Final PS Script. You can:  
- Copy/paste the full script, or  
- Rename `Final PS Script.txt` → `Final PS Script.ps1` and run directly  

#### ✅ Step 4: Open the Power BI File  
Open:  
`Power BI Governance Model.pbit`  
→ Let it refresh, then save as `.pbix`  

---

🎉 That’s it — enjoy! 🎉








---

### ℹ️ Additional Notes

> ⚙️ *PowerShell may prompt to install required modules.*  
> No admin access is needed — they install at the user level.

> 🧰 *This setup uses the portable version of Tabular Editor 2 (v2.26.0).*  
> You don't need it preinstalled. It runs locally from the folder with no differences.  
> https://github.com/TabularEditor/TabularEditor _(MIT License)_

> 🧠 *Model backups use XMLA (for PPU, Premium, Fabric).*  
> For Pro workspaces, `pbi-tools` extracts the BIM from the PBIX.  
> Includes `pbi-tools v1.2`: https://github.com/pbi-tools/pbi-tools _(AGPL 3.0 License)_

> 🚨 *Using Tabular Editor 3?*  
> Tabular Editor 2 is still included and required for this because TE3 doesn't support command line execution.
> 
> 🧩 *Model refresh error in Power BI Desktop?*  
> If you see:  
> _**"Query XXXXXX references other queries or steps..."**_
> 
> Update your Power BI Desktop privacy settings:  
> **File → Options and settings → Options → Privacy**  
> Then select either:  
> - “Combine data according to each file's Privacy Level settings”  
>   **or**  
> - “Always ignore Privacy Level settings”


---
## Features

### ✨ Recently Added Features

- **Workspace Selector** → Only want to run this against 1, 2, 10 workspaces? Now
a popup will allow you to choose which workspaces you run this against. Select All will still run against eveyrthing and a built-in timer ensures no selection will run against everything.
- **Unused Model Objects** → Identify model fields/measures not used in any visuals, measures, or relationships.  
- **Broken Visuals (with Page Links)** → See all broken visuals/filters and jump directly to the impacted report page.  
- **Report-Level Measures Inventory** → Surface report-only measures with full DAX and usage details.
  
---

### 1. Workspace and Power BI Environment Metadata Extraction
- Leverages Power BI REST API to gather information about Power BI workspaces, datasets, reports, report pages, and apps.
- Exports the extracted metadata into a structured Excel workbook with separate worksheets for each entity.
- You must have at least read access within workspaces. 'My Workspace' also included.
- <img width="1255" alt="image" src="https://github.com/user-attachments/assets/515ce3e5-ec56-467a-a421-9da05889eaa5">


### 2. Model Backup and Metadata Extract
- Saves exported models in a structured folder hierarchy based on workspace and dataset names.
- Leverages Tabular Editor 2 and C# to extract the metadata and output within an Excel File.
- All backups are saved with the following format: Workspace Name ~ Model Name.
- You must have edit rights on the related model. Works with all Pro, Premium-Per-User, Premium, and Fabric Capacity workspaces. 'My Workspace' also included. Both XMLA and non-XMLA models.
<img width="695" alt="image" src="https://github.com/user-attachments/assets/c3e021b8-6dfe-40c9-bfa5-b9d4471a8fa3">


### 3. Report Backup and Metadata Extract
- Backs up Power BI and Paginated Reports from Power BI workspaces, cleaning report names and determining file types (`.pbix` or `.rdl`) for export.
- Leverages Tabular Editor 2 and C# to extract the Visual Object Layer metadata and output within an Excel File (credit to @m-kovalsky for initial work on this)
- Paginated Reports are only backed up (no metadata extraction).
- All backups are saved with the following format: Workspace Name ~ Report Name.
- You must have edit rights on the related report. Works with all Pro, Premium-Per-User, Premium, and Fabric Capacity workspaces. 'My Workspace' also included.
- <img width="554" alt="image" src="https://github.com/user-attachments/assets/cf88aac7-6f32-445a-96c7-6bc36fcab9aa">


### 4. Dataflow Backup and Metadata Extract
- Extracts dataflows from Power BI workspaces, formatting and organizing their contents, including query details.
- Leverages PowerShell to parse and extract the metadata and output within an Excel File.
- All backups are saved with the following format: Workspace Name ~ Dataflow Name.
- Must have edit rights on the related dataflow. 'Ownership' of the Dataflow is not required. Works with all Pro, Premium Capacity, Fabric Capacity workspaces. 'My Workspace' also included.
- <img width="542" alt="image" src="https://github.com/user-attachments/assets/67e83016-4bc7-4cf5-8d94-1a9779aad6d8">

### 5. Model Connection Details Metadata Extract
- Leverages Power BI REST API to gather all model connection details.
- Exports the extracted metadata into the same structured excel workbook as the Power BI Environment Information Extract
- You must have read permissions on the related model.

### 6. Model Refresh History Metadata Extract
- Leverages Power BI REST API to gather all model refresh history (limited to the same history shown in the Service).
- Exports the extracted metadata into the same structured excel workbook as the Power BI Environment Detail Extract
- You must have read permissions on the related model.

### 7. Dataflow Connection Details Metadata Extract
- Leverages Power BI REST API to gather all Dataflow connection details.
- Exports the extracted metadata into the same structured excel workbook as the Power BI Environment Detail Extract
- You must have read permissions on the related Dataflow.

### 8. Dataflow Refresh History Metadata Extract
- Leverages Power BI REST API to gather all Dataflow refresh history (limited to the same history shown in the Service).
- Exports the extracted metadata into the same structured excel workbook as the Power BI Environment Detail Extract
- You must have read permissions on the related Dataflow.
  
### 9. Power BI Governance Model
- Combines extracts into a Semantic Model to allow easy exploring, impact analysis, and governance of all Power BI Reports, Models, and Dataflows across all Workspaces
- Works for anyone who runs the script and has at least 1 model and report. Dataflow not required.
- Public example (limited due to no filter pane): https://app.powerbi.com/view?r=eyJrIjoiNmMxYWQ2ZTItZDM4ZS00MGM1LTlhMDQtN2I1OTMwMzI0OTg2IiwidCI6ImUyY2Y4N2QyLTYxMjktNGExYS1iZTczLTEzOGQyY2Y5OGJlMiJ9

## Special Notes
- The script has a built-in timer to ensure the API bearer token does not expire. It is defaulted to require logging in every 55 minutes. This is only applicable if you have a large number of reports and models (150+)
- This defaults to looping across all workspaces. If you only want to run this for a specific workspace, you can enter a workspace ID within the quotation marks in $reportSpecificWorkspaceId and/or $modelSpecificWorkspaceId (these are in the first 20 lines of the script)
- For the best user experience, the final Power BI Govervance Model output is **from the perspective of the Report**. This means that when looking at a Workspace where Reports have the Model sitting in a different Workspace (i.e. multiple reports connected to a model in a different workspace), the Model detail will still be viewable. This ensures you get a comprehensive view of any report. This does not work both ways - when viewing a Workspace with only Models and no Reports, it will only show the Model detail since there are no Reports within that Workspace. If you do not want this perspective and prefer that Model detail only show in the Workspaces they are in, then set the All-Pages filter "Model in Workspace Flag" to TRUE.
- For backing up Reports & extracting the metadata, this mirrors what you can do at powerbi.com. This means that if you cannot download the report online, then the script will also not be able to download it. For Models, this works differently and if it's within a Premium, PPU, or Fabric capacity, even XMLA-only models can be backed up and extracted by leveraging the XMLA endpoint connection.

## Screenshots of Final Output
..
..

<img width="1235" alt="image" src="https://github.com/user-attachments/assets/805d3145-8290-4d84-8da2-bb27529bb050">
<img width="1259" alt="image" src="https://github.com/user-attachments/assets/54212360-8d0f-44c5-9337-db2cdd0fb5ee">
<img width="1259" alt="image" src="https://github.com/user-attachments/assets/9280e350-8714-40e5-8e09-d1de07faf5f5">
<img width="1221" alt="image" src="https://github.com/user-attachments/assets/e120c1bb-b52a-4197-aeb3-2a6ddbb67a9f">
<img width="1221" alt="image" src="https://github.com/user-attachments/assets/c9f5331d-8976-4f66-be76-5628e38e8d0f">
<img width="1241" alt="image" src="https://github.com/user-attachments/assets/9d814034-494d-478b-b231-f759d7eebeab">
