#r "System.IO"
#r "System.IO.Compression.FileSystem"

using System.IO;
using System.IO.Compression;
using System.Text;
using System.Globalization;

// === CONFIGURATION ===
string baseFolderPath = Directory.GetCurrentDirectory();
string addedPath = Path.Combine(baseFolderPath, "Report Backups");

// === FIND LATEST-DATED FOLDER ===
string[] folders = Directory.GetDirectories(addedPath);
string latestFolder = null;
DateTime latestDate = DateTime.MinValue;

foreach (string folder in folders)
{
    string folderName = Path.GetFileName(folder);
    DateTime folderDate;
    if (DateTime.TryParseExact(folderName, "yyyy-MM-dd", null, DateTimeStyles.None, out folderDate))
    {
        if (folderDate > latestDate)
        {
            latestDate = folderDate;
            latestFolder = folder;
        }
    }
}

string pbiFolderName = latestFolder ?? Path.Combine(addedPath, DateTime.Now.ToString("yyyy-MM-dd"));
Directory.CreateDirectory(pbiFolderName);

// === FIND PBIX/PBIT FILES ===
var foldersToDelete = new List<string>();
var fileList = new List<string>();
foreach (var f in Directory.GetFiles(pbiFolderName, "*.pbi*"))
    fileList.Add(f);

// === STRING UTILS ===
Func<string, string, string> ExtractValue = (text, key) =>
{
    int i = text.IndexOf(key);
    if (i == -1) return "";
    i = text.IndexOf(":", i);
    if (i == -1) return "";
    i++;
    while (i < text.Length && (text[i] == ' ' || text[i] == '"')) i++;
    int j = i;
    while (j < text.Length && text[j] != '"' && text[j] != ',' && text[j] != '\r' && text[j] != '\n') j++;
    return text.Substring(i, j - i).Trim('"', ',', ' ');
};

// === ENHANCED EXTRACTION FUNCTIONS ===
Func<Newtonsoft.Json.Linq.JToken, string[]> GetAllPossiblePaths = (node) =>
{
    var paths = new List<string>();
    
    // Legacy paths
    paths.AddRange(new[] {
        "visual.query.queryState.Values.projections",
        "visual.query.queryState.Rows.projections", 
        "visual.query.queryState.Columns.projections",
        "visual.query.queryState.Category.projections",
        "visual.query.queryState.Series.projections",
        "visual.query.queryState.Y.projections",
        "visual.query.queryState.X.projections",
        "visual.query.queryState.Size.projections",
        "visual.query.queryState.Play.projections",
        "visual.query.queryState.Legend.projections",
        "visual.query.queryState.Details.projections",
        "visual.query.queryState.Tooltips.projections",
        // FIX #1: include Data.projections (legacy)
        "visual.query.queryState.Data.projections"
    });
    
    // Modern paths - more comprehensive
    paths.AddRange(new[] {
        "visual.query.dataTransforms[0].queryMetadata.Select",
        "visual.query.dataTransforms[0].queryMetadata.GroupBy", 
        "visual.query.dataTransforms[0].projections",
        "visual.prototypeQuery.queryState.Values.projections",
        "visual.prototypeQuery.queryState.Rows.projections",
        "visual.prototypeQuery.queryState.Columns.projections",
        "visual.query.Binding.Primary.Groupings",
        "visual.query.Binding.Secondary.Groupings",
        "visual.query.Binding.DataReduction.Primary",
        "visual.query.Commands[0].queryState.Values.projections",
        "visual.query.Commands[0].queryState.Rows.projections",
        "visual.query.Commands[0].queryState.Columns.projections",
        // FIX #1: include Data.projections (modern)
        "visual.prototypeQuery.queryState.Data.projections",
        "visual.query.Commands[0].queryState.Data.projections",
        // Additional comprehensive paths
        "visual.query.queryState.Axis.projections",
        "visual.query.queryState.Color.projections",
        "visual.query.queryState.Shape.projections",
        "visual.query.queryState.Gradient.projections",
        "visual.query.queryState.Image.projections",
        "visual.prototypeQuery.queryState.Category.projections",
        "visual.prototypeQuery.queryState.Series.projections",
        "visual.prototypeQuery.queryState.Y.projections",
        "visual.prototypeQuery.queryState.X.projections",
        "visual.query.Commands[0].queryState.Category.projections",
        "visual.query.Commands[0].queryState.Series.projections",
        "config.singleVisual.prototypeQuery.Select",
        "config.singleVisual.query.queryState.Values.projections",
        "config.singleVisual.query.queryState.Rows.projections",
        // Additional paths for custom visuals
        "visual.prototypeQuery.Select",
        "visual.query.Select",
        "query.queryState.Values.projections",
        "query.queryState.Rows.projections",
        "query.queryState.Columns.projections",
        "prototypeQuery.Select",
        "prototypeQuery.queryState.Values.projections",
        // Additional path for textFilter and similar custom visuals
        "visual.query.queryState.field.projections",
        "query.queryState.field.projections"
    });
    
    return paths.ToArray();
};

Func<Newtonsoft.Json.Linq.JToken, List<Newtonsoft.Json.Linq.JToken>> ExtractAllProjections = (visualNode) =>
{
    var allProjections = new List<Newtonsoft.Json.Linq.JToken>();
    var paths = GetAllPossiblePaths(visualNode);
    
    foreach (var path in paths)
    {
        try
        {
            var projections = visualNode.SelectToken("$." + path);
            if (projections != null && projections.Type == Newtonsoft.Json.Linq.JTokenType.Array)
            {
                foreach (var proj in projections)
                {
                    allProjections.Add(proj);
                }
            }
        }
        catch { }
    }
    
    return allProjections;
};

Func<Newtonsoft.Json.Linq.JToken, List<Newtonsoft.Json.Linq.JToken>> ExtractAllFilters = (visualNode) =>
{
    var allFilters = new List<Newtonsoft.Json.Linq.JToken>();
    var filterPaths = new[] {
        "filters",
        "filterConfig.filters", 
        "visual.vcFilters",
        "visual.filters",
        "visual.query.where",
        "visual.query.having",
        "visual.prototypeQuery.where",
        "visual.prototypeQuery.having",
        "visual.query.Commands[0].where",
        "visual.query.Commands[0].having",
        "config.singleVisualGroup.filters",
        "config.singleVisual.vcFilters",
        "config.singleVisual.filters",
        "config.filterConfig.filters",
        "visual.query.filterClause",
        "visual.prototypeQuery.filterClause",
        // Additional paths for custom visuals
        "query.where",
        "query.having",
        "prototypeQuery.where",
        "prototypeQuery.having",
        "vcFilters",
        "visual.query.queryState.field.where",
        "visual.query.queryState.field.having",
        "query.queryState.field.where",
        "query.queryState.field.having"
    };
    
    foreach (var path in filterPaths)
    {
        try
        {
            var filters = visualNode.SelectToken("$." + path);
            if (filters != null && filters.Type == Newtonsoft.Json.Linq.JTokenType.Array)
            {
                foreach (var filter in filters)
                {
                    allFilters.Add(filter);
                }
            }
        }
        catch { }
    }
    
    return allFilters;
};

Func<Newtonsoft.Json.Linq.JToken, Newtonsoft.Json.Linq.JToken> ExtractFieldFromCondition = (condition) =>
{
    try
    {
        // Try different nested structures
        var paths = new[] {
            "Condition.Comparison.Left",
            "Condition.Not.Expression.Comparison.Left",
            "Condition.And.Left.Comparison.Left",
            "Condition.Or.Left.Comparison.Left"
        };
        
        foreach (var path in paths)
        {
            var field = condition.SelectToken(path);
            if (field != null) return field;
        }
    }
    catch { }
    return null;
};

string newline = Environment.NewLine;

var sb_Connections = new System.Text.StringBuilder();
sb_Connections.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "ServerName" + '\t' + "Type" + '\t' + "ReportDate" + newline);

var sb_CustomVisuals = new System.Text.StringBuilder();
sb_CustomVisuals.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "Name" + '\t' + "ReportDate" + newline);

var sb_ReportFilters = new System.Text.StringBuilder();
sb_ReportFilters.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "DisplayName" + '\t' + "TableName" + '\t' + "ObjectName" + '\t' + "ObjectType" + '\t' + "FilterType" + '\t' + "HiddenFilter" + '\t' + "LockedFilter" + '\t' + "AppliedFilterVersion" + '\t' + "ReportDate" + newline);

var sb_VisualObjects = new System.Text.StringBuilder();
sb_VisualObjects.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "PageName" + '\t' + "PageId" + '\t' + "VisualId" + '\t' + "VisualType" + '\t' + "AppliedFilterVersion" + '\t' + "CustomVisualFlag" + '\t' + "TableName" + '\t' + "ObjectName" + '\t' + "ObjectType" + '\t' + "Source" + '\t' + "DisplayName" + '\t' + "ReportDate" + newline);

var sb_Bookmarks = new System.Text.StringBuilder();
sb_Bookmarks.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "Name" + '\t' + "Id" + '\t' + "PageName" + '\t' + "PageId" + '\t' + "VisualId" + '\t' + "VisualHiddenFlag" + '\t' + "ReportDate" + newline);

var sb_PageFilters = new System.Text.StringBuilder();
sb_PageFilters.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "PageId" + '\t' + "PageName" + '\t' + "DisplayName" + '\t' + "TableName" + '\t' + "ObjectName" + '\t' + "ObjectType" + '\t' + "FilterType" + '\t' + "HiddenFilter" + '\t' + "LockedFilter" + '\t' + "AppliedFilterVersion" + '\t' + "ReportDate" + newline);

var sb_Pages = new System.Text.StringBuilder();
sb_Pages.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "Id" + '\t' + "Name" + '\t' + "Number" + '\t' + "Width" + '\t' + "Height" + '\t' + "HiddenFlag" + '\t' + "VisualCount" + '\t' + "BackgroundImage" + '\t' + "WallpaperImage" + '\t' + "Type" + '\t' + "ReportDate" + newline);

var sb_Visuals = new System.Text.StringBuilder();
sb_Visuals.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "PageName" + '\t' + "PageId" + '\t' + "Id" + '\t' + "Name" + '\t' + "Type" + '\t' + "CustomVisualFlag" + '\t' + "HiddenFlag" + '\t' + "X" + '\t' + "Y" + '\t' + "Z" + '\t' + "Width" + '\t' + "Height" + '\t' + "ObjectCount" + '\t' + "ShowItemsNoDataFlag" + '\t' + "SlicerType" + '\t' + "ParentGroup" + '\t' + "ReportDate" + newline);

var sb_VisualFilters = new System.Text.StringBuilder();
sb_VisualFilters.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "PageName" + '\t' + "PageId" + '\t' + "VisualId" + '\t' + "TableName" + '\t' + "ObjectName" + '\t' + "ObjectType" + '\t' + "FilterType" + '\t' + "HiddenFilter" + '\t' + "LockedFilter" + '\t' + "AppliedFilterVersion" + '\t' + "DisplayName" + '\t' + "ReportDate" + newline);

var sb_VisualInteractions = new System.Text.StringBuilder();
sb_VisualInteractions.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "PageName" + '\t' + "PageId" + '\t' + "SourceVisualID" + '\t' + "TargetVisualID" + '\t' + "TypeID" + '\t' + "Type" + '\t' + "ReportDate" + newline);

// === NEW: ReportLevelMeasures StringBuilder ===
var sb_ReportLevelMeasures = new System.Text.StringBuilder();
sb_ReportLevelMeasures.Append("ReportName" + '\t' + "ReportID" + '\t' + "ModelID" + '\t' + "TableName" + '\t' + "ObjectName" + '\t' + "ObjectType" + '\t' + "Expression" + '\t' + "HiddenFlag" + '\t' + "FormatString" + '\t' + "ReportDate" + newline);

// === PROCESS EACH FILE ===
foreach (var rpt in fileList)
{
    var CustomVisuals = new List<CustomVisual>();
    var Bookmarks = new List<Bookmark>();
    var ReportFilters = new List<ReportFilter>();
    var Visuals = new List<Visual>();
    var VisualObjects = new List<VisualObject>();
    var VisualFilters = new List<VisualFilter>();
    var PageFilters = new List<PageFilter>();
    var Pages = new List<Page>();
    var Connections = new List<Connection>();
    var VisualInteractions = new List<VisualInteraction>();
    var ReportLevelMeasures = new List<ReportLevelMeasure>(); // NEW
    
    string fileExt = Path.GetExtension(rpt);
    if (!(fileExt == ".pbix" || fileExt == ".pbit")) continue;

    string reportName = Path.GetFileNameWithoutExtension(rpt);
    string reportDate = latestDate != DateTime.MinValue ? latestDate.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
    string folderName = Path.GetDirectoryName(rpt) + @"\";
    string zipPath = folderName + reportName + ".zip";
    string unzipPath = folderName + reportName;

    bool extractionSucceeded = false;
    try
    {
        File.Copy(rpt, zipPath, true);
        ZipFile.ExtractToDirectory(zipPath, unzipPath);
        extractionSucceeded = true;
        if (extractionSucceeded && Directory.Exists(unzipPath))
            foldersToDelete.Add(unzipPath);
    }
    catch { }
    finally
    {
        if (File.Exists(zipPath)) File.Delete(zipPath);
    }

    if (!extractionSucceeded) continue;

    string modelId = "";
    string reportId = "";

// === CONNECTIONS SECTION (PBIR only) ===
string definitionRoot = Path.Combine(unzipPath, "Report", "definition");
if (Directory.Exists(definitionRoot)) // <-- gate on PBIR structure
{
    string connPath = Path.Combine(unzipPath, "Connections");
    if (File.Exists(connPath))
    {
        try
        {
            string jsonConnPath = connPath + ".json";
            File.Move(connPath, jsonConnPath);

            string rawJson = File.ReadAllText(jsonConnPath, Encoding.UTF8);
            var connJson = Newtonsoft.Json.Linq.JObject.Parse(rawJson);

            var conns = connJson["Connections"];
            if (conns != null && conns.Type == Newtonsoft.Json.Linq.JTokenType.Array)
            {
                foreach (var o in conns)
                {
                    string connType = o["ConnectionType"] != null ? o["ConnectionType"].ToString() : "";
                    string connString = o["ConnectionString"] != null ? o["ConnectionString"].ToString() : "";
                    string serverName = "";
                    string modelID = "";
                    string reportID = "";

                    if (!string.IsNullOrEmpty(connString) && connString.Contains("Data Source="))
                    {
                        int start = connString.IndexOf("Data Source=") + "Data Source=".Length;
                        int end = connString.IndexOf(";", start);
                        if (end > start) serverName = connString.Substring(start, end - start);
                    }

                    var remote = connJson["RemoteArtifacts"];
                    if (remote != null && remote.HasValues)
                    {
                        modelID = remote[0]["DatasetId"] != null ? remote[0]["DatasetId"].ToString() : "";
                        reportID = remote[0]["ReportId"] != null ? remote[0]["ReportId"].ToString() : "";
                    }
                    else
                    {
                        modelID = o["PbiModelDatabaseName"] != null ? o["PbiModelDatabaseName"].ToString() : "";
                    }

                    if (string.IsNullOrEmpty(modelId) && !string.IsNullOrEmpty(modelID))
                        modelId = modelID;

                    if (string.IsNullOrEmpty(reportId) && !string.IsNullOrEmpty(reportID))
                        reportId = reportID;

                    Connections.Add(new Connection
                    {
                        ServerName = serverName,
                        ReportID = reportID,
                        ModelID = modelID,
                        Type = connType,
                        ReportDate = reportDate
                    });
                }
            }
            else
            {
                var remote = connJson["RemoteArtifacts"];
                if (remote != null && remote.HasValues)
                {
                    string modelID = remote[0]["DatasetId"] != null ? remote[0]["DatasetId"].ToString() : "";
                    string reportID = remote[0]["ReportId"] != null ? remote[0]["ReportId"].ToString() : "";
                    string connType = "localPowerQuery";

                    if (string.IsNullOrEmpty(modelId) && !string.IsNullOrEmpty(modelID))
                        modelId = modelID;

                    if (string.IsNullOrEmpty(reportId) && !string.IsNullOrEmpty(reportID))
                        reportId = reportID;

                    Connections.Add(new Connection
                    {
                        ServerName = "",
                        ReportID = reportID,
                        ModelID = modelID,
                        Type = connType,
                        ReportDate = reportDate
                    });
                }
            }

            File.Delete(jsonConnPath);
        }
        catch { }
    }
}


    string pagesRoot = Path.Combine(unzipPath, "Report", "definition", "pages");
    if (Directory.Exists(pagesRoot))
    {
        // === REPORT FILTERS SECTION ===
        string reportJsonPath = Path.Combine(unzipPath, "Report", "definition", "report.json");
        if (File.Exists(reportJsonPath))
        {
            try
            {
                string content = File.ReadAllText(reportJsonPath);
                string formattedJson = "";
                dynamic json = null;

                try
                {
                    formattedJson = Newtonsoft.Json.Linq.JToken.Parse(content).ToString();
                    json = Newtonsoft.Json.Linq.JObject.Parse(formattedJson);
                }
                catch { }

                if (json != null && json["filterConfig"] != null && json["filterConfig"]["filters"] != null)
                {
                    foreach (var filter in json["filterConfig"]["filters"])
                    {
                        string objectType = "";
                        string tableName = "";
                        string objectName = "";
                        string filterType = "";
                        string version = "";
                        // FIX #2: capture hidden/locked/displayName for report-level filters
                        string hidden = "";
                        string locked = "";
                        string displayName = "";

                        try
                        {
                            if (filter["field"]["Column"] != null)
                            {
                                objectType = "Column";
                                tableName = filter["field"]["Column"]["Expression"]["SourceRef"]["Entity"].ToString();
                                objectName = filter["field"]["Column"]["Property"].ToString();
                            }
                            else if (filter["field"]["HierarchyLevel"] != null)
                            {
                                objectType = "Hierarchy";
                                tableName = filter["field"]["HierarchyLevel"]["Expression"]["Hierarchy"]["Expression"]["SourceRef"]["Entity"].ToString();
                                objectName = filter["field"]["HierarchyLevel"]["Level"].ToString();
                            }

                            if (filter["type"] != null)
                                filterType = filter["type"].ToString();

                            if (filter["filter"] != null && filter["filter"]["Version"] != null)
                                version = filter["filter"]["Version"].ToString();

                            var hiddenToken = filter["isHiddenInViewMode"] ?? filter["isHidden"] ?? filter["hidden"] ?? filter["Hidden"];
                            if (hiddenToken != null) hidden = hiddenToken.ToString();

                            var lockedToken = filter["isLockedInViewMode"] ?? filter["isLocked"] ?? filter["locked"] ?? filter["Locked"];
                            if (lockedToken != null) locked = lockedToken.ToString();

                            var displayNameToken = filter["displayName"] ?? filter["DisplayName"] ?? filter["name"] ?? filter["Name"];
                            if (displayNameToken != null) displayName = displayNameToken.ToString();
                        }
                        catch { }

                        ReportFilters.Add(new ReportFilter
                        {
                            TableName = tableName,
                            ObjectName = objectName,
                            ObjectType = objectType,
                            FilterType = filterType,
                            HiddenFilter = hidden,       // FIX #2
                            LockedFilter = locked,       // FIX #2
                            AppliedFilterVersion = version,
                            displayName = displayName,   // keep header alignment
                            ReportID = reportId,
                            ModelID = modelId,
                            ReportDate = reportDate
                        });
                    }
                }

                // === NEW: REPORT LEVEL MEASURES SECTION ===
                try
                {
                    string configRaw = json["config"] != null ? json["config"].ToString() : "";
                    if (!string.IsNullOrEmpty(configRaw))
                    {
                        var configToken = Newtonsoft.Json.Linq.JToken.Parse(configRaw);
                        Action<Newtonsoft.Json.Linq.JToken> processEntities = delegate(Newtonsoft.Json.Linq.JToken entitiesToken)
                        {
                            if (entitiesToken == null) return;
                            foreach (var ent in entitiesToken.Children())
                            {
                                string tableName = (string)ent["name"];
                                if (string.IsNullOrEmpty(tableName))
                                    tableName = (string)ent["Name"];
                                var measures = ent["measures"];
                                if (measures == null) measures = ent["Measures"];
                                if (measures == null) continue;
                                foreach (var m in measures.Children())
                                {
                                    try
                                    {
                                        string objectName = (string)m["name"];
                                        if (string.IsNullOrEmpty(objectName))
                                            objectName = (string)m["Name"];
                                        string expr = (string)m["expression"];
                                        if (string.IsNullOrEmpty(expr))
                                            expr = (string)m["Expression"];
                                        bool hidden = false;
                                        try { hidden = (bool)m["hidden"]; } catch { }
                                        string formatStr = "";
                                        try { formatStr = (string)m["formatInformation"]["formatString"]; } catch { }
                                        if (string.IsNullOrEmpty(formatStr))
                                        {
                                            try { formatStr = (string)m["formatString"]; } catch { }
                                        }
                                        
                                        // Clean up text to prevent tab/newline issues
                                        if (!string.IsNullOrEmpty(expr))
                                        {
                                            expr = expr.Replace("\t", " ").Replace("\r\n", " ").Replace("\n", " ");
                                        }
                                        if (!string.IsNullOrEmpty(formatStr))
                                        {
                                            formatStr = formatStr.Replace("\t", " ").Replace("\r\n", " ").Replace("\n", " ");
                                        }
                                        
                                        string objectType = "Measure";
                                        
                                        ReportLevelMeasures.Add(new ReportLevelMeasure
                                        {
                                            TableName = tableName ?? "",
                                            ObjectName = objectName ?? "",
                                            ObjectType = objectType,
                                            Expression = expr ?? "",
                                            HiddenFlag = hidden.ToString().ToLower(),
                                            FormatString = formatStr ?? "",
                                            ReportName = reportName,
                                            ReportID = reportId,
                                            ModelID = modelId,
                                            ReportDate = reportDate
                                        });
                                    }
                                    catch
                                    {
                                        // Skip malformed measure
                                    }
                                }
                            }
                        };
                        
                        // Path 1: modelExtensions -> entities -> measures
                        var modelExtensions = configToken["modelExtensions"];
                        if (modelExtensions == null) modelExtensions = configToken["ModelExtensions"];
                        if (modelExtensions != null)
                        {
                            foreach (var me in modelExtensions.Children())
                            {
                                var entities = me["entities"];
                                if (entities == null) entities = me["Entities"];
                                processEntities(entities);
                            }
                        }
                        
                        // Path 2: Extension -> Entities -> Measures
                        var extension = configToken["Extension"];
                        if (extension != null)
                        {
                            var extensionEntities = extension["Entities"];
                            processEntities(extensionEntities);
                        }
                    }
                }
                catch
                {
                    // Ignore if config not present or parse fails
                }

                // === NEW: REPORT EXTENSIONS MEASURES SECTION ===
                try
                {
                    string reportExtensionsPath = Path.Combine(unzipPath, "Report", "definition", "reportExtensions.json");
                    if (File.Exists(reportExtensionsPath))
                    {
                        string extensionsContent = File.ReadAllText(reportExtensionsPath, Encoding.UTF8);
                        var extensionsJson = Newtonsoft.Json.Linq.JObject.Parse(extensionsContent);
                        
                        var entities = extensionsJson["entities"];
                        if (entities != null && entities.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                        {
                            foreach (var entity in entities)
                            {
                                string tableName = entity["name"] != null ? entity["name"].ToString() : "";
                                var measures = entity["measures"];
                                
                                if (measures != null && measures.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                                {
                                    foreach (var measure in measures)
                                    {
                                        try
                                        {
                                            string objectName = measure["name"] != null ? measure["name"].ToString() : "";
                                            string expr = measure["expression"] != null ? measure["expression"].ToString() : "";
                                            string formatStr = measure["formatString"] != null ? measure["formatString"].ToString() : "";
                                            string hidden = measure["hidden"] != null ? measure["hidden"].ToString().ToLower() : "false";
                                            
                                            // Clean up text to prevent tab/newline issues
                                            if (!string.IsNullOrEmpty(expr))
                                            {
                                                expr = expr.Replace("\t", " ").Replace("\r\n", " ").Replace("\n", " ");
                                            }
                                            if (!string.IsNullOrEmpty(formatStr))
                                            {
                                                formatStr = formatStr.Replace("\t", " ").Replace("\r\n", " ").Replace("\n", " ");
                                            }
                                            
                                            string objectType = "Measure";
                                            
                                            ReportLevelMeasures.Add(new ReportLevelMeasure
                                            {
                                                TableName = tableName ?? "",
                                                ObjectName = objectName ?? "",
                                                ObjectType = objectType,
                                                Expression = expr ?? "",
                                                HiddenFlag = hidden,
                                                FormatString = formatStr ?? "",
                                                ReportName = reportName,
                                                ReportID = reportId,
                                                ModelID = modelId,
                                                ReportDate = reportDate
                                            });
                                        }
                                        catch
                                        {
                                            // Skip malformed measure
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore if reportExtensions.json not present or parse fails
                }
            }
            catch { }
        }

        // === BOOKMARKS SECTION ===
        string bookmarksPath = Path.Combine(unzipPath, "Report", "definition", "bookmarks", "bookmarks.json");
        if (File.Exists(bookmarksPath))
        {
            try
            {
                string content = File.ReadAllText(bookmarksPath);
                string[] bookmarks = content.Split(new string[] { "\"name\"" }, StringSplitOptions.None);
                foreach (string b in bookmarks)
                {
                    if (b.Contains("\"displayName\"") && b.Contains("\"id\""))
                    {
                        string name = ExtractValue(b, "\"name\"");
                        string id = ExtractValue(b, "\"id\"");
                        string group = ExtractValue(b, "\"group\"");
                        string displayName = ExtractValue(b, "\"displayName\"");

                        Bookmarks.Add(new Bookmark
                        {
                            Name = name,
                            Id = id,
                            PageName = group,
                            PageId = "",
                            VisualId = "",
                            VisualHiddenFlag = false,
                            ReportID = reportId,
                            ModelID = modelId,
                            ReportDate = reportDate
                        });
                    }
                }
            }
            catch { }
        }

        foreach (var pageFolder in Directory.GetDirectories(pagesRoot))
        {
            string pageId = Path.GetFileName(pageFolder);
            string pageJsonPath = Path.Combine(pageFolder, "page.json");
            if (!File.Exists(pageJsonPath)) continue;
            string pageName = "";

            try
            {
                string unformattedPageJson = File.ReadAllText(pageJsonPath, Encoding.UTF8);
                string formattedPageJson = Newtonsoft.Json.Linq.JToken.Parse(unformattedPageJson).ToString();
                dynamic pageJson = Newtonsoft.Json.Linq.JObject.Parse(formattedPageJson);

                pageName = pageJson["displayName"] != null ? pageJson["displayName"].ToString() : "";
                string width = pageJson["width"] != null ? pageJson["width"].ToString() : "0";
                string height = pageJson["height"] != null ? pageJson["height"].ToString() : "0";

                Pages.Add(new Page
                {
                    Id = pageId,
                    Name = pageName,
                    ReportID = reportId,
                    ModelID = modelId,
                    Number = 0,
                    Width = string.IsNullOrEmpty(width) ? 0 : int.Parse(width),
                    Height = string.IsNullOrEmpty(height) ? 0 : int.Parse(height),
                    HiddenFlag = false,
                    VisualCount = 0,
                    BackgroundImage = "",
                    WallpaperImage = "",
                    Type = "",
                    ReportDate = reportDate
                });

                // === PAGE FILTERS - COMPREHENSIVE ENHANCEMENT ===
                var pageFilterPaths = new string[]
                {
                    "config.filterConfig.filters",
                    "filterConfig.filters",
                    "filters",
                    "config.filters",
                    "config.layouts[0].filterConfig.filters",
                    "layouts[0].filterConfig.filters",
                    "config.singlePage.filterConfig.filters",
                    "singlePage.filterConfig.filters",
                    "config.visualContainers.filterConfig.filters",
                    "visualContainers.filterConfig.filters",
                    "pageFilters",
                    "config.pageFilters",
                    "config.singlePageGroup.filterConfig.filters",
                    "config.reportLevelFilters",
                    "reportLevelFilters"
                };

                var allPageFilters = new List<Newtonsoft.Json.Linq.JToken>();

                foreach (var path in pageFilterPaths)
                {
                    try
                    {
                        var filterToken = pageJson.SelectToken("$." + path);
                        if (filterToken != null && filterToken.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                        {
                            foreach (var filter in filterToken)
                            {
                                allPageFilters.Add(filter);
                            }
                        }
                    }
                    catch { }
                }

                foreach (var filter in allPageFilters)
                {
                    string objectType = "";
                    string tableName = "";
                    string objectName = "";
                    string filterType = "";
                    string version = "";
                    string hidden = "";
                    string locked = "";
                    string displayName = "";

                    try
                    {
                        var field = filter["field"];
                        if (field != null)
                        {
                            var column = field["Column"];
                            var hierarchy = field["HierarchyLevel"];
                            var measure = field["Measure"];
                            var aggregation = field["Aggregation"];
                            var dateHierarchy = field["DateHierarchy"];
                            var selectColumn = field["SelectColumn"];
                            var groupBy = field["GroupBy"];

                            if (column != null)
                            {
                                objectType = "Column";
                                var expression = column["Expression"];
                                var sourceRef = expression != null ? expression["SourceRef"] : null;
                                var entity = sourceRef != null ? sourceRef["Entity"] : null;
                                tableName = entity != null ? entity.ToString() : "";
                                var property = column["Property"];
                                objectName = property != null ? property.ToString() : "";
                            }
                            else if (hierarchy != null)
                            {
                                objectType = "Hierarchy";
                                var expr = hierarchy["Expression"];
                                var hierarchyNode = expr != null ? expr["Hierarchy"] : null;
                                var innerExpr = hierarchyNode != null ? hierarchyNode["Expression"] : null;
                                var sourceRef = innerExpr != null ? innerExpr["SourceRef"] : null;
                                var entity = sourceRef != null ? sourceRef["Entity"] : null;
                                tableName = entity != null ? entity.ToString() : "";
                                var level = hierarchy["Level"];
                                objectName = level != null ? level.ToString() : "";
                            }
                            else if (measure != null)
                            {
                                objectType = "Measure";
                                var expression = measure["Expression"];
                                var sourceRef = expression != null ? expression["SourceRef"] : null;
                                var entity = sourceRef != null ? sourceRef["Entity"] : null;
                                tableName = entity != null ? entity.ToString() : "";
                                var property = measure["Property"];
                                objectName = property != null ? property.ToString() : "";
                            }
                            else if (aggregation != null)
                            {
                                objectType = "Column";
                                var expr = aggregation["Expression"];
                                if (expr != null && expr["Column"] != null)
                                {
                                    var sourceRef = expr["Column"]["Expression"]["SourceRef"];
                                    tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                    objectName = expr["Column"]["Property"] != null ? expr["Column"]["Property"].ToString() : "";
                                }
                            }
                            else if (dateHierarchy != null)
                            {
                                objectType = "Column";
                                var expr = dateHierarchy["Expression"];
                                var sourceRef = expr != null && expr["SourceRef"] != null ? expr["SourceRef"] : null;
                                tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                objectName = dateHierarchy["Level"] != null ? dateHierarchy["Level"].ToString() : "";
                            }
                            else if (selectColumn != null)
                            {
                                objectType = "Column";
                                var expr = selectColumn["Expression"];
                                var sourceRef = expr != null ? expr["SourceRef"] : null;
                                tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                objectName = selectColumn["Property"] != null ? selectColumn["Property"].ToString() : "";
                            }
                            else if (groupBy != null)
                            {
                                objectType = "Column";
                                var expr = groupBy["Expression"];
                                var sourceRef = expr != null ? expr["SourceRef"] : null;
                                tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                objectName = groupBy["Property"] != null ? groupBy["Property"].ToString() : "";
                            }
                        }

                        var typeToken = filter["type"] ?? filter["filterType"] ?? filter["Filter"] ?? filter["FilterType"];
                        filterType = typeToken != null ? typeToken.ToString() : "";

                        var filterObj = filter["filter"] ?? filter["Filter"] ?? filter["filterCondition"];
                        var versionToken = filterObj != null ? (filterObj["Version"] ?? filterObj["version"]) : null;
                        version = versionToken != null ? versionToken.ToString() : "";

                        var hiddenToken = filter["isHiddenInViewMode"] ?? filter["isHidden"] ?? filter["hidden"] ?? filter["Hidden"];
                        hidden = hiddenToken != null ? hiddenToken.ToString() : "";

                        var lockedToken = filter["isLockedInViewMode"] ?? filter["isLocked"] ?? filter["locked"] ?? filter["Locked"];
                        locked = lockedToken != null ? lockedToken.ToString() : "";

                        var displayNameToken = filter["displayName"] ?? filter["DisplayName"] ?? filter["name"] ?? filter["Name"];
                        displayName = displayNameToken != null ? displayNameToken.ToString() : "";
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(tableName) || !string.IsNullOrEmpty(objectName) || !string.IsNullOrEmpty(filterType))
                    {
                        PageFilters.Add(new PageFilter
                        {
                            PageId = pageId,
                            PageName = pageName,
                            ReportID = reportId,
                            ModelID = modelId,
                            ReportDate = reportDate,
                            displayName = displayName,
                            TableName = tableName,
                            ObjectName = objectName,
                            ObjectType = objectType,
                            FilterType = filterType,
                            HiddenFilter = hidden,
                            LockedFilter = locked,
                            AppliedFilterVersion = version
                        });
                    }
                }
            }
            catch
            {
                continue;
            }

            // === VISUALS - ENHANCED ===
            string visualsPath = Path.Combine(pageFolder, "visuals");
            if (!Directory.Exists(visualsPath)) continue;

            foreach (var visualFolder in Directory.GetDirectories(visualsPath))
            {
                string visualJsonPath = Path.Combine(visualFolder, "visual.json");
                if (!File.Exists(visualJsonPath)) continue;

                try
                {
                    string unformattedVisualJson = File.ReadAllText(visualJsonPath, Encoding.UTF8);
                    var node = Newtonsoft.Json.Linq.JObject.Parse(unformattedVisualJson);

                    string visualId = node["name"] != null ? node["name"].ToString() : Path.GetFileName(visualFolder);
                    string visualType = "";
                    string name = "";
                    string x = "", y = "", z = "", width = "", height = "";

                    // === Get visual type - ENHANCED ===
                    try
                    {
                        var visualNodeType = node["visual"];
                        if (visualNodeType != null && visualNodeType["visualType"] != null)
                        {
                            visualType = visualNodeType["visualType"].ToString();
                        }
                        else
                        {
                            var altPaths = new[] {
                                "config.singleVisual.visualType",
                                "singleVisual.visualType",
                                "visual.singleVisual.visualType"
                            };
                            
                            foreach (var path in altPaths)
                            {
                                var typeToken = node.SelectToken("$." + path);
                                if (typeToken != null)
                                {
                                    visualType = typeToken.ToString();
                                    break;
                                }
                            }
                        }
                    }
                    catch { }

                    // === Get title text ===
                    try
                    {
                        var general = node.SelectToken("visual.objects.general[0].properties.paragraphs[0].textRuns[0].value");
                        if (general != null) name = general.ToString();
                        
                        if (string.IsNullOrEmpty(name))
                        {
                            var altTitlePaths = new[] {
                                "visual.objects.title[0].properties.text.value",
                                "visual.config.objects.general[0].properties.title",
                                "config.singleVisual.objects.general[0].properties.title"
                            };
                            
                            foreach (var path in altTitlePaths)
                            {
                                var titleToken = node.SelectToken("$." + path);
                                if (titleToken != null)
                                {
                                    name = titleToken.ToString();
                                    break;
                                }
                            }
                        }
                    }
                    catch { }

                    // === Get position ===
                    try
                    {
                        var pos = node["position"];
                        if (pos != null)
                        {
                            x = pos["x"] != null ? pos["x"].ToString() : "";
                            y = pos["y"] != null ? pos["y"].ToString() : "";
                            z = pos["z"] != null ? pos["z"].ToString() : "";
                            width = pos["width"] != null ? pos["width"].ToString() : "";
                            height = pos["height"] != null ? pos["height"].ToString() : "";
                        }
                    }
                    catch { }

                    // === Get additional visual properties ===
                    bool showItemsNoData = false;
                    string slicerType = "N/A";
                    string parentGroup = "";
                    bool hiddenFlag = false;
                    int visualObjectCount = 0;
                    
                    try
                    {
                        var parentGroupToken = node["parentGroupName"];
                        if (parentGroupToken != null)
                        {
                            parentGroup = parentGroupToken.ToString();
                        }
                    }
                    catch { }
                    
                    try
                    {
                        var showAllRoles = node.SelectToken("visual.showAllRoles");
                        if (showAllRoles != null && showAllRoles.Type == Newtonsoft.Json.Linq.JTokenType.Array && showAllRoles.HasValues)
                        {
                            string firstRole = showAllRoles[0].ToString();
                            if (firstRole == "Values" || firstRole == "Rows" || firstRole == "Columns")
                            {
                                showItemsNoData = true;
                            }
                        }
                    }
                    catch { }
                    
                    try
                    {
                        if (visualType == "slicer")
                        {
                            var slicerMode = node.SelectToken("visual.objects.data[0].properties.mode.expr.Literal.Value");
                            if (slicerMode != null)
                            {
                                string modeValue = slicerMode.ToString();
                                if (modeValue == "'Basic'")
                                {
                                    slicerType = "List";
                                }
                                else if (modeValue == "'Dropdown'")
                                {
                                    slicerType = "Dropdown";
                                }
                            }
                            else
                            {
                                slicerType = "List";
                            }
                        }
                    }
                    catch { }
                    
                    try
                    {
                        var displayMode = node.SelectToken("visual.display.mode");
                        if (displayMode != null && displayMode.ToString() == "hidden")
                        {
                            hiddenFlag = true;
                        }
                    }
                    catch { }

                    Visuals.Add(new Visual
                    {
                        Id = visualId,
                        Name = name,
                        Type = visualType,
                        X = string.IsNullOrEmpty(x) ? 0 : (int)double.Parse(x),
                        Y = string.IsNullOrEmpty(y) ? 0 : (int)double.Parse(y),
                        Z = string.IsNullOrEmpty(z) ? 0 : (int)double.Parse(z),
                        Width = string.IsNullOrEmpty(width) ? 0 : (int)double.Parse(width),
                        Height = string.IsNullOrEmpty(height) ? 0 : (int)double.Parse(height),
                        HiddenFlag = hiddenFlag,
                        PageId = pageId,
                        PageName = pageName,
                        ReportID = reportId,
                        ModelID = modelId,
                        CustomVisualFlag = visualType.ToLower().StartsWith("custom"),
                        ObjectCount = visualObjectCount,
                        ShowItemsNoDataFlag = showItemsNoData,
                        SlicerType = slicerType,
                        ParentGroup = string.IsNullOrEmpty(parentGroup) ? null : parentGroup,
                        ReportDate = reportDate
                    });

                    // === VISUAL INTERACTIONS ===
                    var visualNode = node["visual"];
                    var interactions = visualNode != null ? visualNode["interactions"] : null;

                    if (interactions != null && interactions.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                    {
                        foreach (var interaction in interactions)
                        {
                            string targetVisualId = "";
                            string interactionType = "";
                            int interactionTypeId = 0;

                            if (interaction["target"] != null)
                            {
                                targetVisualId = interaction["target"].ToString();
                            }

                            if (interaction["interactionState"] != null)
                            {
                                string typeCode = interaction["interactionState"].ToString();

                                if (typeCode == "0")
                                {
                                    interactionType = "None";
                                    interactionTypeId = 0;
                                }
                                else if (typeCode == "1")
                                {
                                    interactionType = "Filter";
                                    interactionTypeId = 1;
                                }
                                else if (typeCode == "2")
                                {
                                    interactionType = "Highlight";
                                    interactionTypeId = 2;
                                }
                                else
                                {
                                    interactionType = "Unknown";
                                    interactionTypeId = -1;
                                }
                            }

                            if (!string.IsNullOrEmpty(targetVisualId))
                            {
                                VisualInteractions.Add(new VisualInteraction {
                                    PageId = pageId,
                                    PageName = pageName,
                                    ReportID = reportId,
                                    ModelID = modelId,
                                    SourceVisualID = visualId,
                                    TargetVisualID = targetVisualId,
                                    Type = interactionType,
                                    TypeID = interactionTypeId,
                                    ReportDate = reportDate
                                });
                            }
                        }
                    }

                    // === CUSTOM VISUALS ===
                    var visualNodeForCustom = node["visual"];
                    if (visualNodeForCustom != null && visualNodeForCustom["customVisual"] != null)
                    {
                        CustomVisuals.Add(new CustomVisual {
                            Name = visualId,
                            ReportID = reportId,
                            ModelID = modelId,
                            ReportDate = reportDate
                        });
                    }

                    // === VISUAL OBJECTS - ENHANCED FOR BETTER COVERAGE ===
                    try
                    {
                        var visual = node["visual"];
                        string visualTypeLower = visualType.ToLower();
                        string customFlag = visualTypeLower.StartsWith("custom") ? "true" : "false";

                        var allProjections = ExtractAllProjections(node);
                        
                        foreach (var proj in allProjections)
                        {
                            string projectionModelId = proj["modelId"] != null ? proj["modelId"].ToString() : "";
                            string source = proj["queryRef"] != null ? proj["queryRef"].ToString() : "";
                            string displayName = proj["displayName"] != null ? proj["displayName"].ToString() : "";
                            string appliedFilterVersion = (proj["filter"] != null && proj["filter"]["Version"] != null)
                                ? proj["filter"]["Version"].ToString()
                                : "";

                            string tableName = "";
                            string objectName = "";
                            string objectType = "";

                            var field = proj["field"];
                            if (field != null)
                            {
                                if (field["Column"] != null)
                                {
                                    var expr = field["Column"]["Expression"];
                                    var entity = expr != null && expr["SourceRef"] != null ? expr["SourceRef"]["Entity"] : null;
                                    tableName = entity != null ? entity.ToString() : "";
                                    objectName = field["Column"]["Property"] != null ? field["Column"]["Property"].ToString() : "";
                                    objectType = "Column";
                                }
                                else if (field["Measure"] != null)
                                {
                                    var expr = field["Measure"]["Expression"];
                                    var entity = expr != null && expr["SourceRef"] != null ? expr["SourceRef"]["Entity"] : null;
                                    tableName = entity != null ? entity.ToString() : "";
                                    objectName = field["Measure"]["Property"] != null ? field["Measure"]["Property"].ToString() : "";
                                    objectType = "Measure";
                                }
                                else if (field["HierarchyLevel"] != null)
                                {
                                    var expr = field["HierarchyLevel"]["Expression"];
                                    var hierarchy = expr != null ? expr["Hierarchy"] : null;
                                    var innerExpr = hierarchy != null ? hierarchy["Expression"] : null;
                                    var entity = innerExpr != null && innerExpr["SourceRef"] != null ? innerExpr["SourceRef"]["Entity"] : null;
                                    tableName = entity != null ? entity.ToString() : "";
                                    
                                    string levelName = field["HierarchyLevel"]["Level"] != null ? field["HierarchyLevel"]["Level"].ToString() : "";
                                    string hierName = hierarchy != null && hierarchy["Hierarchy"] != null ? hierarchy["Hierarchy"].ToString() : "";
                                    objectName = !string.IsNullOrEmpty(hierName) && !string.IsNullOrEmpty(levelName) ? hierName + "." + levelName : levelName;
                                    objectType = "Hierarchy";
                                }
                                else if (field["Aggregation"] != null)
                                {
                                    var expr = field["Aggregation"]["Expression"];
                                    if (expr != null && expr["Column"] != null)
                                    {
                                        var sourceRef = expr["Column"]["Expression"]["SourceRef"];
                                        tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                        objectName = expr["Column"]["Property"] != null ? expr["Column"]["Property"].ToString() : "";
                                        objectType = "Column";
                                    }
                                    else if (expr != null && expr["Measure"] != null)
                                    {
                                        var sourceRef = expr["Measure"]["Expression"]["SourceRef"];
                                        tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                        objectName = expr["Measure"]["Property"] != null ? expr["Measure"]["Property"].ToString() : "";
                                        objectType = "Measure";
                                    }
                                }
                                else if (field["DateHierarchy"] != null)
                                {
                                    var expr = field["DateHierarchy"]["Expression"];
                                    var sourceRef = expr != null && expr["SourceRef"] != null ? expr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                    objectName = field["DateHierarchy"]["Level"] != null ? field["DateHierarchy"]["Level"].ToString() : "";
                                    objectType = "Column";
                                }
                                else if (field["SelectColumn"] != null)
                                {
                                    var expr = field["SelectColumn"]["Expression"];
                                    var sourceRef = expr != null ? expr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                    objectName = field["SelectColumn"]["Property"] != null ? field["SelectColumn"]["Property"].ToString() : "";
                                    objectType = "Column";
                                }
                                else if (field["GroupBy"] != null)
                                {
                                    var expr = field["GroupBy"]["Expression"];
                                    var sourceRef = expr != null ? expr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                    objectName = field["GroupBy"]["Property"] != null ? field["GroupBy"]["Property"].ToString() : "";
                                    objectType = "Column";
                                }
                            }

                            // Always add the object, even if tableName or objectName is empty
                            visualObjectCount++;
                            VisualObjects.Add(new VisualObject {
                                PageId = pageId,
                                PageName = pageName,
                                ReportID = reportId,
                                ModelID = projectionModelId,
                                VisualId = visualId,
                                VisualType = visualType,
                                AppliedFilterVersion = appliedFilterVersion,
                                CustomVisualFlag = customFlag == "true",
                                TableName = tableName,
                                ObjectName = objectName,
                                ObjectType = objectType,
                                Source = source,
                                displayName = displayName,
                                ReportDate = reportDate
                            });
                        }
                    }
                    catch { }

                    // === VISUAL OBJECTS FROM CONDITIONAL FORMATTING AND OBJECTS ===
                    try
                    {
                        var visualObjects = node.SelectToken("visual.objects");
                        if (visualObjects != null)
                        {
                            // Helper function to extract field info from expression
                            Action<Newtonsoft.Json.Linq.JToken, string> processExpression = null;
                            processExpression = delegate(Newtonsoft.Json.Linq.JToken expr, string sourceType)
                            {
                                if (expr == null) return;
                                
                                try
                                {
                                    string tableName = "";
                                    string objectName = "";
                                    string objectType = "";
                                    
                                    // Try different expression types
                                    if (expr["Measure"] != null)
                                    {
                                        var measureExpr = expr["Measure"]["Expression"];
                                        var sourceRef = measureExpr != null ? measureExpr["SourceRef"] : null;
                                        tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                        objectName = expr["Measure"]["Property"] != null ? expr["Measure"]["Property"].ToString() : "";
                                        objectType = "Measure";
                                    }
                                    else if (expr["Column"] != null)
                                    {
                                        var columnExpr = expr["Column"]["Expression"];
                                        var sourceRef = columnExpr != null ? columnExpr["SourceRef"] : null;
                                        tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                        objectName = expr["Column"]["Property"] != null ? expr["Column"]["Property"].ToString() : "";
                                        objectType = "Column";
                                    }
                                    else if (expr["Aggregation"] != null)
                                    {
                                        var aggExpr = expr["Aggregation"]["Expression"];
                                        if (aggExpr != null && aggExpr["Column"] != null)
                                        {
                                            var sourceRef = aggExpr["Column"]["Expression"] != null ? aggExpr["Column"]["Expression"]["SourceRef"] : null;
                                            tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                            objectName = aggExpr["Column"]["Property"] != null ? aggExpr["Column"]["Property"].ToString() : "";
                                            objectType = "Column";
                                        }
                                        else if (aggExpr != null && aggExpr["Measure"] != null)
                                        {
                                            var sourceRef = aggExpr["Measure"]["Expression"] != null ? aggExpr["Measure"]["Expression"]["SourceRef"] : null;
                                            tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                            objectName = aggExpr["Measure"]["Property"] != null ? aggExpr["Measure"]["Property"].ToString() : "";
                                            objectType = "Measure";
                                        }
                                    }
                                    else if (expr["FillRule"] != null && expr["FillRule"]["Input"] != null)
                                    {
                                        processExpression(expr["FillRule"]["Input"], sourceType);
                                        return;
                                    }
                                    else if (expr["Conditional"] != null && expr["Conditional"]["Cases"] != null)
                                    {
                                        var cases = expr["Conditional"]["Cases"];
                                        if (cases.Type == Newtonsoft.Json.Linq.JTokenType.Array && cases.HasValues)
                                        {
                                            var firstCase = cases[0];
                                            if (firstCase != null && firstCase["Condition"] != null)
                                            {
                                                var condition = firstCase["Condition"];
                                                // Try to find Comparison.Left
                                                var comparison = condition["Comparison"];
                                                if (comparison == null && condition["And"] != null)
                                                {
                                                    comparison = condition["And"]["Left"] != null ? condition["And"]["Left"]["Comparison"] : null;
                                                }
                                                if (comparison != null && comparison["Left"] != null)
                                                {
                                                    processExpression(comparison["Left"], sourceType);
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                    
                                    // Always add the object, even if tableName or objectName is empty
                                    VisualObjects.Add(new VisualObject {
                                        PageId = pageId,
                                        PageName = pageName,
                                        ReportID = reportId,
                                        ModelID = modelId,
                                        VisualId = visualId,
                                        VisualType = visualType,
                                        AppliedFilterVersion = "",
                                        CustomVisualFlag = visualType.ToLower().StartsWith("custom"),
                                        TableName = tableName,
                                        ObjectName = objectName,
                                        ObjectType = objectType,
                                        Source = sourceType,
                                        displayName = "",
                                        ReportDate = reportDate
                                    });
                                }
                                catch { }
                            };
                            
                            // Check various object types for conditional formatting
                            var objectTypes = new[] { "labels", "categoryAxis", "valueAxis", "title", "background", "border", "dropShadow", "values", "dataLabels" };
                            foreach (var objType in objectTypes)
                            {
                                try
                                {
                                    var objects = visualObjects[objType];
                                    if (objects != null && objects.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                                    {
                                        foreach (var obj in objects)
                                        {
                                            if (obj["properties"] == null) continue;
                                            
                                            var properties = obj["properties"];
                                            
                                            // Check common property paths for expressions
                                            var propertyPaths = new[] {
                                                "color.solid.color.expr",
                                                "fontColor.solid.color.expr",
                                                "backColor.solid.color.expr",
                                                "background.solid.color.expr",
                                                "labelColor.solid.color.expr",
                                                "titleColor.solid.color.expr",
                                                "text.expr",
                                                "webURL.expr",
                                                "icon.value.expr"
                                            };
                                            
                                            foreach (var propPath in propertyPaths)
                                            {
                                                try
                                                {
                                                    var expr = properties.SelectToken(propPath);
                                                    if (expr != null)
                                                    {
                                                        string sourceLabel = objType;
                                                        if (propPath.Contains("fontColor")) sourceLabel = objType + " (Font Color)";
                                                        else if (propPath.Contains("backColor")) sourceLabel = objType + " (Back Color)";
                                                        else if (propPath.Contains("background")) sourceLabel = objType + " (Background)";
                                                        else if (propPath.Contains("text")) sourceLabel = objType + " (Text)";
                                                        else if (propPath.Contains("webURL")) sourceLabel = objType + " (WebURL)";
                                                        else if (propPath.Contains("icon")) sourceLabel = objType + " (Icon)";
                                                        
                                                        processExpression(expr, sourceLabel);
                                                    }
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }

                    // === VISUAL FILTERS - ENHANCED FOR BETTER COVERAGE ===
                    try
                    {
                        var allFilters = ExtractAllFilters(node);
                        
                        foreach (var filter in allFilters)
                        {
                            string tableName = "";
                            string objectName = "";
                            string objectType = "";
                            string filterType = "";
                            string hidden = "";
                            string locked = "";
                            string version = "";
                            string displayName = "";

                            var field = filter["field"];
                            if (field != null)
                            {
                                var column = field["Column"];
                                var hierarchy = field["HierarchyLevel"];
                                var measure = field["Measure"];

                                if (column != null)
                                {
                                    var expr = column["Expression"];
                                    var sourceRef = expr != null ? expr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null
                                        ? sourceRef["Entity"].ToString()
                                        : "";
                                    objectName = column["Property"] != null ? column["Property"].ToString() : "";
                                    objectType = "Column";
                                }
                                else if (hierarchy != null)
                                {
                                    var expr = hierarchy["Expression"];
                                    var hierarchyNode = expr != null ? expr["Hierarchy"] : null;
                                    var innerExpr = hierarchyNode != null ? hierarchyNode["Expression"] : null;
                                    var sourceRef = innerExpr != null ? innerExpr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null
                                        ? sourceRef["Entity"].ToString()
                                        : "";
                                    
                                    string levelName = hierarchy["Level"] != null ? hierarchy["Level"].ToString() : "";
                                    string hierName = hierarchyNode != null && hierarchyNode["Hierarchy"] != null ? hierarchyNode["Hierarchy"].ToString() : "";
                                    objectName = !string.IsNullOrEmpty(hierName) && !string.IsNullOrEmpty(levelName) ? hierName + "." + levelName : levelName;
                                    objectType = "Hierarchy";
                                }
                                else if (measure != null)
                                {
                                    var expr = measure["Expression"];
                                    var sourceRef = expr != null ? expr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null
                                        ? sourceRef["Entity"].ToString()
                                        : "";
                                    objectName = measure["Property"] != null ? measure["Property"].ToString() : "";
                                    objectType = "Measure";
                                }
                                else if (field["Aggregation"] != null)
                                {
                                    var expr = field["Aggregation"]["Expression"];
                                    if (expr != null && expr["Column"] != null)
                                    {
                                        var sourceRef = expr["Column"]["Expression"]["SourceRef"];
                                        tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                        objectName = expr["Column"]["Property"] != null ? expr["Column"]["Property"].ToString() : "";
                                        objectType = "Column";
                                    }
                                    else if (expr != null && expr["Measure"] != null)
                                    {
                                        var sourceRef = expr["Measure"]["Expression"]["SourceRef"];
                                        tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                        objectName = expr["Measure"]["Property"] != null ? expr["Measure"]["Property"].ToString() : "";
                                        objectType = "Measure";
                                    }
                                }
                                else if (field["DateHierarchy"] != null)
                                {
                                    var expr = field["DateHierarchy"]["Expression"];
                                    var sourceRef = expr != null && expr["SourceRef"] != null ? expr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                    objectName = field["DateHierarchy"]["Level"] != null ? field["DateHierarchy"]["Level"].ToString() : "";
                                    objectType = "Column";
                                }
                                else if (field["SelectColumn"] != null)
                                {
                                    var expr = field["SelectColumn"]["Expression"];
                                    var sourceRef = expr != null ? expr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                    objectName = field["SelectColumn"]["Property"] != null ? field["SelectColumn"]["Property"].ToString() : "";
                                    objectType = "Column";
                                }
                                else if (field["GroupBy"] != null)
                                {
                                    var expr = field["GroupBy"]["Expression"];
                                    var sourceRef = expr != null ? expr["SourceRef"] : null;
                                    tableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                    objectName = field["GroupBy"]["Property"] != null ? field["GroupBy"]["Property"].ToString() : "";
                                    objectType = "Column";
                                }
                            }

                            filterType = filter["type"] != null ? filter["type"].ToString() : "";
                            version = filter["filter"] != null && filter["filter"]["Version"] != null
                                ? filter["filter"]["Version"].ToString()
                                : "";
                            
                            var hiddenToken = filter["isHiddenInViewMode"] ?? filter["isHidden"];
                            hidden = hiddenToken != null ? hiddenToken.ToString() : "";
                            
                            var lockedToken = filter["isLockedInViewMode"] ?? filter["isLocked"];
                            locked = lockedToken != null ? lockedToken.ToString() : "";
                            
                            displayName = filter["displayName"] != null ? filter["displayName"].ToString() : "";

                            VisualFilters.Add(new VisualFilter {
                                PageId = pageId,
                                PageName = pageName,
                                ReportID = reportId,
                                ModelID = modelId,
                                VisualId = visualId,
                                TableName = tableName,
                                ObjectName = objectName,
                                ObjectType = objectType,
                                FilterType = filterType,
                                LockedFilter = locked,
                                HiddenFilter = hidden,
                                AppliedFilterVersion = version,
                                displayName = displayName,
                                ReportDate = reportDate
                            });
                            
                            // Also extract filters from the Where conditions
                            try
                            {
                                var filterObj = filter["filter"];
                                if (filterObj != null && filterObj["Where"] != null)
                                {
                                    var whereConditions = filterObj["Where"];
                                    if (whereConditions.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                                    {
                                        foreach (var condition in whereConditions)
                                        {
                                            // Extract fields from comparison conditions
                                            var comparisonField = ExtractFieldFromCondition(condition);
                                            if (comparisonField != null)
                                            {
                                                string condTableName = "";
                                                string condObjectName = "";
                                                string condObjectType = "";
                                                
                                                if (comparisonField["Column"] != null)
                                                {
                                                    var expr = comparisonField["Column"]["Expression"];
                                                    var sourceRef = expr != null ? expr["SourceRef"] : null;
                                                    condTableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                                    condObjectName = comparisonField["Column"]["Property"] != null ? comparisonField["Column"]["Property"].ToString() : "";
                                                    condObjectType = "Column";
                                                }
                                                else if (comparisonField["Measure"] != null)
                                                {
                                                    var expr = comparisonField["Measure"]["Expression"];
                                                    var sourceRef = expr != null ? expr["SourceRef"] : null;
                                                    condTableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                                    condObjectName = comparisonField["Measure"]["Property"] != null ? comparisonField["Measure"]["Property"].ToString() : "";
                                                    condObjectType = "Measure";
                                                }
                                                else if (comparisonField["Aggregation"] != null)
                                                {
                                                    var expr = comparisonField["Aggregation"]["Expression"];
                                                    if (expr != null && expr["Column"] != null)
                                                    {
                                                        var sourceRef = expr["Column"]["Expression"]["SourceRef"];
                                                        condTableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                                        condObjectName = expr["Column"]["Property"] != null ? expr["Column"]["Property"].ToString() : "";
                                                        condObjectType = "Column";
                                                    }
                                                    else if (expr != null && expr["Measure"] != null)
                                                    {
                                                        var sourceRef = expr["Measure"]["Expression"]["SourceRef"];
                                                        condTableName = sourceRef != null && sourceRef["Entity"] != null ? sourceRef["Entity"].ToString() : "";
                                                        condObjectName = expr["Measure"]["Property"] != null ? expr["Measure"]["Property"].ToString() : "";
                                                        condObjectType = "Measure";
                                                    }
                                                }
                                                
                                                // Add this condition-based filter
                                                VisualFilters.Add(new VisualFilter {
                                                    PageId = pageId,
                                                    PageName = pageName,
                                                    ReportID = reportId,
                                                    ModelID = modelId,
                                                    VisualId = visualId,
                                                    TableName = condTableName,
                                                    ObjectName = condObjectName,
                                                    ObjectType = condObjectType,
                                                    FilterType = filterType,
                                                    LockedFilter = locked,
                                                    HiddenFilter = hidden,
                                                    AppliedFilterVersion = version,
                                                    displayName = displayName,
                                                    ReportDate = reportDate
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }

                }
                catch { continue; }
            }
        }
    }

    // Offset child visuals by their parent group X/Y
    foreach (var x in Visuals.ToList())
    {
        if (!string.IsNullOrEmpty(x.ParentGroup))
        {
            var parent = Visuals.FirstOrDefault(v => v.Id == x.ParentGroup);
            if (parent != null)
            {
                x.X += parent.X;
                x.Y += parent.Y;
            }
        }
    }

    // Append results to StringBuilders
    foreach (var x in CustomVisuals) sb_CustomVisuals.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.Name + '\t' + reportDate + newline);
    foreach (var x in ReportFilters) sb_ReportFilters.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.displayName + '\t' + x.TableName + '\t' + x.ObjectName + '\t' + x.ObjectType + '\t' + x.FilterType + '\t' + x.HiddenFilter + '\t' + x.LockedFilter + '\t' + x.AppliedFilterVersion + '\t' + reportDate + newline);
    foreach (var x in PageFilters) sb_PageFilters.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.PageId + '\t' + x.PageName + '\t' + x.displayName + '\t' + x.TableName + '\t' + x.ObjectName + '\t' + x.ObjectType + '\t' + x.FilterType + '\t' + x.HiddenFilter + '\t' + x.LockedFilter + '\t' + x.AppliedFilterVersion + '\t' + reportDate + newline);
    foreach (var x in VisualFilters) sb_VisualFilters.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.PageName + '\t' + x.PageId + '\t' + x.VisualId + '\t' + x.TableName + '\t' + x.ObjectName + '\t' + x.ObjectType + '\t' + x.FilterType + '\t' + x.HiddenFilter + '\t' + x.LockedFilter + '\t' + x.AppliedFilterVersion + '\t' + x.displayName + '\t' + reportDate + newline);
    foreach (var x in VisualObjects) sb_VisualObjects.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.PageName + '\t' + x.PageId + '\t' + x.VisualId + '\t' + x.VisualType + '\t' + x.AppliedFilterVersion + '\t' + x.CustomVisualFlag + '\t' + x.TableName + '\t' + x.ObjectName + '\t' + x.ObjectType + '\t' + x.Source + '\t' + x.displayName + '\t' + reportDate + newline);
    foreach (var x in Bookmarks) sb_Bookmarks.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.Name + '\t' + x.Id + '\t' + x.PageName + '\t' + x.PageId + '\t' + x.VisualId + '\t' + x.VisualHiddenFlag + '\t' + reportDate + newline);
    foreach (var x in Pages) sb_Pages.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.Id + '\t' + x.Name + '\t' + x.Number + '\t' + x.Width + '\t' + x.Height + '\t' + x.HiddenFlag + '\t' + x.VisualCount + '\t' + x.BackgroundImage + '\t' + x.WallpaperImage + '\t' + x.Type + '\t' + reportDate + newline);
    foreach (var x in Visuals) sb_Visuals.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.PageName + '\t' + x.PageId + '\t' + x.Id + '\t' + x.Name + '\t' + x.Type + '\t' + x.CustomVisualFlag + '\t' + x.HiddenFlag + '\t' + x.X + '\t' + x.Y + '\t' + x.Z + '\t' + x.Width + '\t' + x.Height + '\t' + x.ObjectCount + '\t' + x.ShowItemsNoDataFlag + '\t' + x.SlicerType + '\t' + x.ParentGroup + '\t' + reportDate + newline);
    foreach (var x in Connections) sb_Connections.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.ServerName + '\t' + x.Type + '\t' + reportDate + newline);
    foreach (var x in VisualInteractions) sb_VisualInteractions.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.PageName + '\t' + x.PageId + '\t' + x.SourceVisualID + '\t' + x.TargetVisualID + '\t' + x.TypeID + '\t' + x.Type + '\t' + reportDate + newline);
    foreach (var x in ReportLevelMeasures) sb_ReportLevelMeasures.Append(reportName + '\t' + reportId + '\t' + modelId + '\t' + x.TableName + '\t' + x.ObjectName + '\t' + x.ObjectType + '\t' + x.Expression + '\t' + x.HiddenFlag + '\t' + x.FormatString + '\t' + reportDate + newline);
}

// === SAVE OUTPUT SECTION ===
bool saveToFile = true;

if (saveToFile)
{
    File.WriteAllText(Path.Combine(pbiFolderName, "CustomVisuals.txt"), sb_CustomVisuals.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "ReportFilters.txt"), sb_ReportFilters.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "PageFilters.txt"), sb_PageFilters.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "VisualFilters.txt"), sb_VisualFilters.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "VisualObjects.txt"), sb_VisualObjects.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "Visuals.txt"), sb_Visuals.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "Bookmarks.txt"), sb_Bookmarks.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "Pages.txt"), sb_Pages.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "Connections.txt"), sb_Connections.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "VisualInteractions.txt"), sb_VisualInteractions.ToString());
    File.WriteAllText(Path.Combine(pbiFolderName, "ReportLevelMeasures.txt"), sb_ReportLevelMeasures.ToString());
}
else
{
    sb_CustomVisuals.Output();
    sb_ReportFilters.Output();
    sb_PageFilters.Output();
    sb_VisualFilters.Output();
    sb_VisualObjects.Output();
    sb_Visuals.Output();
    sb_Bookmarks.Output();
    sb_Pages.Output();
    sb_Connections.Output();
    sb_VisualInteractions.Output();
    sb_ReportLevelMeasures.Output();
}


foreach (var folder in foldersToDelete)
{
    try
    {
        string layoutPath = Path.Combine(folder, "Report", "Layout");

        // Only delete if Layout.json is NOT present
        if (!File.Exists(layoutPath))
        {
            Directory.Delete(folder, true);
        }
    }
    catch { }
}


 } // ← End of foreach (var rpt in fileList - comment out if using Tabular Editor 3)

// Classes for each object set
public class CustomVisual
{
    public string Name { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string ReportDate { get; set; }
}

public class Bookmark
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string PageName { get; set; }
    public string PageId { get; set; }
    public string VisualId { get; set; }
    public bool VisualHiddenFlag { get; set; }
    public string ReportDate { get; set; }
}

public class ReportFilter
{
    public string displayName { get; set; }
    public string TableName { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string ObjectName { get; set; }
    public string ObjectType { get; set; }
    public string FilterType { get; set; }
    public string HiddenFilter { get; set; }
    public string LockedFilter { get; set; }
    public string AppliedFilterVersion { get; set; }
    public string ReportDate { get; set; }
}

public class VisualObject
{
    public string PageName { get; set; }
    public string PageId { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string displayName { get; set; }
    public string VisualId { get; set; }
    public string VisualType { get; set; }
    public string AppliedFilterVersion { get; set; }
    public bool CustomVisualFlag { get; set; }
    public string TableName { get; set; }
    public string ObjectName { get; set; }
    public string ObjectType { get; set; }
    public string Source { get; set; }
    public string ReportDate { get; set; }
}

public class Visual
{
    public string PageName { get; set; }
    public string PageId { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public bool CustomVisualFlag { get; set; }
    public bool HiddenFlag { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int ObjectCount { get; set; }
    public bool ShowItemsNoDataFlag { get; set; }
    public string SlicerType { get; set; }
    public string ParentGroup {get; set; }
    public string ReportDate { get; set; }
}

public class VisualFilter
{
    public string PageName { get; set; }
    public string PageId { get; set; }
    public string VisualId { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string TableName { get; set; }
    public string ObjectName { get; set; }
    public string ObjectType { get; set; }
    public string FilterType { get; set; }
    public string HiddenFilter { get; set; }
    public string LockedFilter { get; set; }
    public string AppliedFilterVersion { get; set; }
    public string displayName { get; set; }
    public string ReportDate { get; set; }
}

public class PageFilter
{
    public string PageId { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string PageName { get; set; }
    public string displayName {get; set; }
    public string TableName { get; set; }
    public string ObjectName { get; set; }
    public string ObjectType { get; set; }
    public string FilterType { get; set; }   
    public string HiddenFilter { get; set; }
    public string LockedFilter { get; set; } 
    public string AppliedFilterVersion { get; set; }
    public string ReportDate { get; set; } 
}

public class Page
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public int Number { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool HiddenFlag { get; set; }
    public int VisualCount { get; set; }
    public string BackgroundImage { get; set; }
    public string WallpaperImage { get; set; }
    public string Type { get; set; }
    public string ReportDate { get; set; }
}

public class Connection
{
    public string ServerName { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string Type { get; set; }
    public string ReportDate { get; set; }
}

public class VisualInteraction
{
    public string PageName { get; set; }
    public string PageId { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string SourceVisualID { get; set; }
    public string TargetVisualID { get; set; }
    public int TypeID { get; set; }
    public string Type { get; set; }
    public string ReportDate { get; set; }
}

public class ReportLevelMeasure
{
    public string TableName { get; set; }
    public string ObjectName { get; set; }
    public string ObjectType { get; set; }
    public string Expression { get; set; }
    public string HiddenFlag { get; set; }
    public string FormatString { get; set; }
    public string ReportName { get; set; }
    public string ReportID { get; set; }
    public string ModelID { get; set; }
    public string ReportDate { get; set; }
}

static void _() { // Comment out this line if using Tabular Editor 3
