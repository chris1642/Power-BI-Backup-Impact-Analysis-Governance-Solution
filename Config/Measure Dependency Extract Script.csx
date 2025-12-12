using System.IO;

// Define the base path for the backups
string baseFolderPath = Directory.GetCurrentDirectory();
var addedPath = System.IO.Path.Combine(baseFolderPath, "Model Backups");

// Dynamically find the latest-dated folder
string[] folders = System.IO.Directory.GetDirectories(addedPath);
string latestFolder = null;
DateTime latestDate = DateTime.MinValue;

foreach (string folder in folders)
{
    string folderName = System.IO.Path.GetFileName(folder);
    DateTime folderDate;

    if (DateTime.TryParseExact(folderName, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out folderDate))
    {
        if (folderDate > latestDate)
        {
            latestDate = folderDate;
            latestFolder = folder;
        }
    }
}

// Use the latest-dated folder, or fallback to today's date if no valid folder is found
var currentDateStr = latestFolder != null ? latestDate.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");

// Create the folder path for the backup
var dateFolderPath = latestFolder ?? System.IO.Path.Combine(addedPath, currentDateStr);
if (!System.IO.Directory.Exists(dateFolderPath))
{
    System.IO.Directory.CreateDirectory(dateFolderPath);
}

// Retrieve the model name
var modelName = Model.Database.Name;
var modelID = Model.Database.ID;

// Initialize the StringBuilder for the CSV content
var sb = new System.Text.StringBuilder();
sb.AppendLine("ObjectName,ObjectType,DependsOn,DependsOnType,ModelAsOfDate,ModelName,ModelID");

// ===============================
//   MEASURES
// ===============================
foreach (var table in Model.Tables)
{
    foreach (var measure in table.Measures)
    {
        var dependencies = measure.DependsOn;

        foreach (var dependency in dependencies)
        {
            sb.AppendLine(String.Format("\"{0}\",\"Measure\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                                         measure.Name,
                                         dependency.Key.DaxObjectFullName,
                                         dependency.Key.ObjectType,
                                         currentDateStr,
                                         modelName,
                                         modelID));
        }
    }
}

// ===============================
//   CALCULATED COLUMNS
// ===============================
foreach (var table in Model.Tables)
{
    foreach (var calcCol in table.CalculatedColumns)
    {
        var dependencies = calcCol.DependsOn;

        foreach (var dependency in dependencies)
        {
            sb.AppendLine(String.Format("\"{0}\",\"CalculatedColumn\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                                         calcCol.Name,
                                         dependency.Key.DaxObjectFullName,
                                         dependency.Key.ObjectType,
                                         currentDateStr,
                                         modelName,
                                         modelID));
        }
    }
}

// Write the file
var filePath = System.IO.Path.Combine(dateFolderPath, modelName + "_MD.csv");
System.IO.File.WriteAllText(filePath, sb.ToString());
