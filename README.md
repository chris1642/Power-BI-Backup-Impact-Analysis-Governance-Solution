
## This all-in-one solution is designed to be ran by ANYONE. 
- Everything within the script is limited to your access within the Power BI environment.
- All computer requirements are at the user level and do not require admin privileges.
- There are ZERO pre-reqs. It's as simple as downloading this repo into C:\Power BI Backups, opening Powershell, and running the included 'Final PS Script'

**Have specific Reports & models you want to run this against? Don't have direct access to the workspace but have the PBIX? Check out the local version here: https://github.com/chris1642/Local-Power-BI-Impact-Analysis-Governance**

# Power BI Governance & Impact Analysis Solution

## What It Does
This provides a quick and automated way to identify where and how specific fields, measures, and tables are used across Power BI reports in all workspaces by analyzing the visual object layer. It also backs up and breaks down the details of your models, reports, and dataflows for easy review, giving you an all-in-one **Power BI Governance** solution.

### Key Features:
- **Impact Analysis**: Fully understand the downstream impact of data model changes, ensuring you don’t accidentally break visuals or dashboards—especially when reports connected to a model span multiple workspaces.
- **Comprehensive Environment Overview**: Gain a clear, detailed view of your entire Power BI environment, including complete breakdowns of your models, reports, and dataflows and their dependencies. 
- **Backup Solution**: Automatically backs up every model, report, and dataflow for safekeeping.
- **User-Friendly Output**: Results are presented in a Power BI model, making them easy to explore, analyze, and share with your team.

     .

     .

## Instructions:

1. Create a new folder on your C: drive called 'Power BI Backups' (C:\Power BI Backups).
2. Download contents of this repo and place in C:\Power BI Backups folder. The contents of the Config folder should be at C:\Power BI Backups\Config
5. Open PowerShell and run 'Final PS Script' (either via copy/paste or renaming the format from .txt to .ps1 and executing).
6. Once complete, open 'Power BI Governance Model.pbit' and the model will refresh with your data. All relationships, Visuals, and Measures are set up. Save as PBIX.


     .

     .

- ** If any modules are required within PowerShell, PowerShell will request to install (user level, no admin access required) **
- ** This includes the portable version of Tabular Editor 2 v2.25.0 (https://github.com/TabularEditor/TabularEditor - MIT License). This no longer requires having it preinstalled, as it will run from the portable version with no differences. ** 
- ** For backing up Models, the solution will use the XMLA endpoint via Tabular Editor 2's Command Line Interface for PPU, Premium, and Fabric workspaces. Within Pro workspaces, pbi-tools is leveraged to extract the Model into a BIM file via the PBIX. This solution includes pbi-tools v1.2 (https://github.com/pbi-tools/pbi-tools - AGPL 3.0 license). **
- ** For users of the commercial/enterprise Tabular Editor 3, Tabular Editor 2 is still required. TE3 does not have a command line interface feature **
- ** If you see a refresh error in the Power BI Governance Model Template along the lines of "Query XXXXXX references other queries or steps, so it may not directly access a data source. Please rebuild this data combination", this is related to the privacy settings within Power BI Desktop. To resolve, go to: File -> Options and settings -> Options -> Privacy (under Global) -> change to EITHER "Combine data according to each file's Privacy Level settings" or "Always Ignore Privacy Level Settings". The template model file is set to ignore privacy settings for the excel files - but requires that the global privacy settings are either per file or always ignored. **


## Features

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
