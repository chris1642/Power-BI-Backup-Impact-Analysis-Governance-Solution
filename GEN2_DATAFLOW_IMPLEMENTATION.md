# Gen 2 Dataflow Support Implementation

## Overview
This document describes the implementation of Gen 2 (Fabric) dataflow backup support added to the Power BI Backup Impact Analysis & Governance Solution.

## Background
The original script only supported Gen 1 (classic) dataflows using the Power BI REST API. Gen 2 dataflows, which are used in Fabric and CI/CD scenarios, require a different API and have a different data structure.

## What Changed

### 1. Automatic Generation Detection
The script now reads the `generation` field from each dataflow's metadata to determine whether it's a Gen 1 or Gen 2 dataflow:
- **Gen 1**: Uses the Power BI REST API (`https://api.powerbi.com/v1.0/myorg/groups/{workspaceId}/dataflows/{dataflowId}`)
- **Gen 2**: Uses the Fabric API (`https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/dataflows/{dataflowId}/getDefinition`)

### 2. Fabric API Integration
For Gen 2 dataflows, the script now:
1. Calls the Fabric API `getDefinition` endpoint
2. Receives a response containing definition parts
3. Extracts the `item.dataflow.json` part which contains the PowerQuery definition
4. Decodes the base64-encoded payload
5. Extracts the `document` field containing the PowerQuery M code

### 3. Improved Processing
The script handles the different data formats:
- **Gen 1**: JSON response with escape sequences that need to be processed
- **Gen 2**: Plain text PowerQuery M code that requires minimal processing

### 4. Enhanced Formatting
The formatting logic now differentiates between Gen 1 and Gen 2:
- Gen 1 dataflows: Full escape sequence processing (`\\r\\n`, `\\\"`, etc.)
- Gen 2 dataflows: Simpler processing focusing on structure (removing `section Section1;`, handling `shared` declarations)

## Code Structure

### Key Changes in `Final PS Script.txt`

#### Line 1563: Generation Detection
```powershell
$dataflowGeneration = $dataflow.generation
```

#### Lines 1582-1630: Gen 2 Processing
```powershell
if ($dataflowGeneration -eq 2) {
    # Use Fabric API
    $fabricApiUrl = "https://api.fabric.microsoft.com/v1/workspaces/$workspaceId/dataflows/$dataflowId/getDefinition"
    $fabricResponse = Invoke-RestMethod -Uri $fabricApiUrl ...
    # Extract and decode base64 payload
    # Parse JSON to get document field
}
```

#### Lines 1631-1670: Gen 1 Processing (Original Logic)
```powershell
else {
    # Use Power BI REST API
    $apiUrl = "https://api.powerbi.com/v1.0/myorg/groups/$workspaceId/dataflows/$dataflowId"
    # Extract document from JSON response
}
```

#### Lines 1674-1687: Differential Formatting (Step 1)
```powershell
if ($dataflowGeneration -eq 2) {
    # Gen 2: minimal escape sequence processing
} else {
    # Gen 1: full escape sequence processing
}
```

#### Lines 1705-1719: Differential Formatting (Step 2)
```powershell
if ($dataflowGeneration -eq 2) {
    # Gen 2: handle "shared" declarations and section headers
} else {
    # Gen 1: handle escape sequences
}
```

## API Reference

### Fabric API Endpoint
- **URL**: `https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/dataflows/{dataflowId}/getDefinition`
- **Method**: POST
- **Authentication**: Bearer token (same as Power BI API)
- **Response Structure**:
  ```json
  {
    "definition": {
      "parts": [
        {
          "path": "item.dataflow.json",
          "payload": "<base64-encoded-json>"
        }
      ]
    }
  }
  ```

### Decoded Payload Structure
The `item.dataflow.json` payload contains:
```json
{
  "document": "section Section1;\nshared Query1 = ...",
  ...
}
```

## Testing Recommendations

To test this implementation:

1. **Verify Gen 1 Dataflows Still Work**
   - Run the script against workspaces with Gen 1 dataflows
   - Verify backup files are created correctly
   - Check that queries are extracted properly

2. **Test Gen 2 Dataflows**
   - Run the script against workspaces with Gen 2 (Fabric) dataflows
   - Verify the Fabric API is called successfully
   - Check that backup files contain the PowerQuery M code
   - Verify queries are extracted and formatted correctly

3. **Mixed Environment**
   - Test in a workspace with both Gen 1 and Gen 2 dataflows
   - Verify each is processed correctly

## Error Handling

The implementation includes comprehensive error handling:
- API call failures are caught and logged
- Missing payload parts are detected
- Invalid response structures are handled gracefully
- The script continues processing other dataflows even if one fails

## Permissions Required

- **Gen 1 Dataflows**: Edit rights on the dataflow (same as before)
- **Gen 2 Dataflows**: Edit rights on the dataflow + Fabric API access

The Fabric API uses the same authentication token as the Power BI API, so no additional authentication setup is required.

## Backward Compatibility

This implementation maintains full backward compatibility:
- Gen 1 dataflow processing remains unchanged
- Existing backup files and outputs are not affected
- The script automatically detects the generation and uses the appropriate method

## Known Limitations

1. **Fabric API Availability**: The Fabric API must be accessible in your environment
2. **Token Permissions**: The access token must have permissions for both Power BI and Fabric APIs
3. **Gen 2 Format Variations**: PowerQuery M code structure may vary; the formatting logic handles common patterns but may need adjustment for edge cases

## Future Enhancements

Potential improvements for future versions:
1. Enhanced error messages specific to Fabric API issues
2. Support for additional Gen 2 dataflow metadata
3. Parallel processing of Gen 1 and Gen 2 dataflows for performance
4. Caching of Fabric API responses to reduce API calls

## References

- [Microsoft Fabric Dataflow API Documentation](https://learn.microsoft.com/en-us/rest/api/fabric/dataflow/items/get-dataflow-definition)
- [Power BI REST API Documentation](https://learn.microsoft.com/en-us/rest/api/power-bi/)
