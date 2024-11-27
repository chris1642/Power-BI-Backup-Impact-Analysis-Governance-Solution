## This all-in-one solution is designed to be ran by anyone who uses Power BI and will work on any computer, no matter the permissions. 
- Everything within the script is limited to your access within the Power BI environment ('My Workspace') is not included. 
- All computer requirements are at the user level and do not require admin privileges.

- <img width="1255" alt="image" src="https://github.com/user-attachments/assets/515ce3e5-ec56-467a-a421-9da05889eaa5">


## Getting Started

### Instructions:
1. Install latest Tabular Editor 2 (https://github.com/TabularEditor/TabularEditor/releases)
2. Create a new folder on your C: drive called 'Power BI Backups' (C:/Power BI Backups)
3. Place Config folder and the contents from Repo into C:/Power BI Backups
4. Run 'Final PS Script' within PowerShell (either via copy/paste or renaming format to .ps1 and executing)
5. Once complete, open 'Power BI Governance Model.pbit' and the model will refresh with your data. All relationships, Visuals, and Measures are set up. Save as PBIX.


** If any modules are required, PowerShell will request to install (user level, no admin access required) **


## Features

### 1. Workspace and Metadata Extraction
- Retrieves information about Power BI workspaces, datasets, data sources, reports, report pages, and apps.
- Exports the extracted metadata into a structured Excel workbook with separate worksheets for each entity.

### 2. Model Backup and Metadata Extract
- Saves exported models in a structured folder hierarchy based on workspace and dataset names.
- Leverages Tabular Editor 2 and C# to extract the metadata and output within an Excel File.
<img width="695" alt="image" src="https://github.com/user-attachments/assets/c3e021b8-6dfe-40c9-bfa5-b9d4471a8fa3">


### 3. Report Backup and Metadata Extract
- Backs up reports from Power BI workspaces, cleaning report names and determining file types (`.pbix` or `.rdl`) for export.
- Leverages Tabular Editor 2 and C# to extract the Visual Object Layer metadata and output within an Excel File (credit to @m-kovalsky for initial work on this)

- <img width="554" alt="image" src="https://github.com/user-attachments/assets/cf88aac7-6f32-445a-96c7-6bc36fcab9aa">


### 4. Dataflow Backup and Metadata Extract
- Extracts dataflows from Power BI workspaces, formatting and organizing their contents, including query details.
- Leverages PowerShell to parse and extract the metadata and output within an Excel File.

- <img width="542" alt="image" src="https://github.com/user-attachments/assets/67e83016-4bc7-4cf5-8d94-1a9779aad6d8">

  
### 5. Power BI Governance Model
- Combines extracts into a Semantic Model to allow easy exploring, impact analysis, and governance of all Power BI Reports, Models, and Dataflows across all Workspaces
- Public example (limited due to no filter pane): https://app.powerbi.com/view?r=eyJrIjoiYzkzNWZlYWItMDc4OS00YTE2LTg0YTYtZTc3MDdlYzUwMzUxIiwidCI6ImUyY2Y4N2QyLTYxMjktNGExYS1iZTczLTEzOGQyY2Y5OGJlMiJ9)

..
..

<img width="1235" alt="image" src="https://github.com/user-attachments/assets/33101f84-b567-4a45-9729-09303eeb50fb">
<img width="1259" alt="image" src="https://github.com/user-attachments/assets/87d23e7e-5f9b-4883-8c58-f102033be5e0">
<img width="1221" alt="image" src="https://github.com/user-attachments/assets/e120c1bb-b52a-4197-aeb3-2a6ddbb67a9f">
<img width="1241" alt="image" src="https://github.com/user-attachments/assets/9d814034-494d-478b-b231-f759d7eebeab">
![Uploading image.png…]()
