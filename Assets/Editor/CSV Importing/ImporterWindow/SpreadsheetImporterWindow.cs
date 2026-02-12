using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

#if UNITY_EDITOR
public class SpreadsheetImporterWindow : EditorWindow
{
    // url of the sheet to add
    private string sheetUrl = "";

    // list of saved sheets
    private List<SpreadsheetData> savedSheets = new List<SpreadsheetData>();
    private string configPath => "Assets/CSVFiles/sheetConfig.json";

    // authentication
    private string authJson = "";
    private bool isAuthenticated = false;

    [System.Serializable]
    private class SpreadsheetData
    {
        public string SpreadsheetId;
        public string Gid;
        public string Name;
        public CSVType Type = CSVType.Weapons; // default type
    }

    [System.Serializable]
    private class SpreadsheetListWrapper
    {
        public List<SpreadsheetData> sheets = new List<SpreadsheetData>();
    }

    [MenuItem("Tools/Spreadsheet Importer")]
    public static void ShowWindow()
    {
        GetWindow<SpreadsheetImporterWindow>("Spreadsheet Importer");
    }

    private void OnEnable()
    {
        LoadConfig();
    }

    private void OnGUI()
    {
        GUILayout.Label("Spreadsheet Importer", EditorStyles.boldLabel);

        if (!isAuthenticated)
        {
            DrawAuthWindow();
        }
        else
        {
            DrawImporterWindow();
            DrawAddSheetWindow();
        }
    }

    private void DrawAuthWindow()
    {
        GUILayout.Label("Authentication Required", EditorStyles.boldLabel);
        authJson = EditorGUILayout.TextField("Auth JSON", authJson);

        if (GUILayout.Button("Authenticate"))
        {
            if (!string.IsNullOrEmpty(authJson))
            {
                isAuthenticated = true;
                SaveConfig();
                Debug.Log("Authenticated");
                GUI.FocusControl(null);
                Repaint();
            }
            else
            {
                Debug.LogWarning("Auth JSON cannot be empty.");
            }
        }
    }

    private void DrawAddSheetWindow()
    {
        GUILayout.Space(10);
        GUILayout.Label("Add Spreadsheet", EditorStyles.boldLabel);
        sheetUrl = EditorGUILayout.TextField("Google Sheet URL", sheetUrl);

        using (new EditorGUI.DisabledScope(!isAuthenticated))
        {
            if (GUILayout.Button("Add Sheet"))
            {
                AddSheetFromUrl(sheetUrl);
            }
            if (GUILayout.Button("Update All"))
            {
                UpdateAllSheets();
            }
        }
    }

    private void DrawImporterWindow()
    {
        GUILayout.Label("Saved Spreadsheets", EditorStyles.boldLabel);

        for (int i = 0; i < savedSheets.Count; i++)
        {
            SpreadsheetData sheet = savedSheets[i];

            if (string.IsNullOrEmpty(sheet.Name))
            {
                try
                {
                    // create sheets service
                    SheetsService service = GoogleSheetsDownloader.CreateSheetsService(authJson);
                    // fetch spreadsheet metadata
                    Spreadsheet spreadsheet = service.Spreadsheets.Get(sheet.SpreadsheetId).Execute();
                    // resolve name from gid
                    sheet.Name = GoogleSheetsDownloader.ResolveSheetNameFromGid(spreadsheet, sheet.Gid);
                    SaveConfig();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("failed to fetch sheet name: " + e.Message);
                    sheet.Name = sheet.Gid; // fallback
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(sheet.Name ?? sheet.Gid, GUILayout.Width(200));

            // enum dropdown to select CSV type
            CSVType newType = (CSVType)EditorGUILayout.EnumPopup(sheet.Type, GUILayout.Width(120));
            if (newType != sheet.Type)
            {
                sheet.Type = newType;
                SaveConfig(); // save immediately so the change persists
            }

            if (GUILayout.Button("Download and import", GUILayout.Width(140)))
            {
                DownloadAndImportSheet(sheet.SpreadsheetId, sheet.Gid, sheet.Name + ".csv", sheet.Type);
            }
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                string fileName = (sheet.Name ?? sheet.Gid) + ".csv";
                string assetPath = "Assets/CSVFiles/" + fileName;

                if (File.Exists(assetPath))
                {
                    bool success = AssetDatabase.DeleteAsset(assetPath);
                    if (success)
                    {
                        Debug.Log("Deleted CSV and meta: " + assetPath);
                    }
                    else
                    {
                        Debug.LogWarning("Failed to delete asset: " + assetPath);
                    }
                }

                savedSheets.RemoveAt(i);
                i--;
                SaveConfig();
            }

            GUILayout.EndHorizontal();
        }
    }

    private void AddSheetFromUrl(string url)
    {
        if (!isAuthenticated)
        {
            Debug.LogWarning("Cannot add sheet: not authenticated");
            return;
        }

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("URL cannot be empty");
            return;
        }

        Match match = Regex.Match(url, @"docs\.google\.com/spreadsheets/d/([a-zA-Z0-9-_]+).*gid=([0-9]+)");
        if (!match.Success)
        {
            Debug.LogWarning("Invalid Google Sheets URL");
            return;
        }

        string spreadsheetId = match.Groups[1].Value;
        string gid = match.Groups[2].Value;

        SpreadsheetData newSheet = new SpreadsheetData
        {
            SpreadsheetId = spreadsheetId,
            Gid = gid,
            Name = null // will be resolved automatically
        };

        savedSheets.Add(newSheet);
        SaveConfig();

        sheetUrl = "";
        GUI.FocusControl(null);
        Repaint();

        Debug.Log("Added sheet (gid: " + gid + ")");
    }

    
    private void LoadConfig()
    {
        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            SpreadsheetListWrapper wrapper = JsonUtility.FromJson<SpreadsheetListWrapper>(json);
            savedSheets = wrapper.sheets ?? new List<SpreadsheetData>();

            string authPath = Path.Combine(Path.GetDirectoryName(configPath), "auth.json");
            if (File.Exists(authPath))
            {
                authJson = File.ReadAllText(authPath);
                isAuthenticated = !string.IsNullOrEmpty(authJson);
            }
        }
    }

    private void SaveConfig()
    {
        Directory.CreateDirectory("Assets/CSVFiles");

        string json = JsonUtility.ToJson(new SpreadsheetListWrapper { sheets = savedSheets }, true);
        File.WriteAllText(configPath, json);

        string authPath = Path.Combine(Path.GetDirectoryName(configPath), "auth.json");
        File.WriteAllText(authPath, authJson);

        AssetDatabase.Refresh();
    }

    private void DownloadAndImportSheet(string spreadsheetId, string gid, string fileName, CSVType type)
    {
        string path = Path.Combine(Application.dataPath, "CSVFiles", fileName);
        GoogleSheetsDownloader.DownloadSheetToCsv(authJson, spreadsheetId, gid, path);
        AssetDatabase.Refresh();

        // import using selected type
        CSVImporter.ImportCSV(type);
    }

    private void UpdateAllSheets()
    {
        foreach (SpreadsheetData sheet in savedSheets)
        {
            if (string.IsNullOrEmpty(sheet.Name))
            {
                try
                {
                    // create sheets service
                    SheetsService service = GoogleSheetsDownloader.CreateSheetsService(authJson);
                    // fetch spreadsheet metadata
                    Spreadsheet spreadsheet = service.Spreadsheets.Get(sheet.SpreadsheetId).Execute();
                    // resolve name from gid
                    sheet.Name = GoogleSheetsDownloader.ResolveSheetNameFromGid(spreadsheet, sheet.Gid);
                    SaveConfig();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("failed to fetch sheet name: " + e.Message);
                    sheet.Name = sheet.Gid; // fallback
                }
            }
        
            // pass the CSVType from the sheet
            DownloadAndImportSheet(sheet.SpreadsheetId, sheet.Gid, sheet.Name + ".csv", sheet.Type);
        }
    }
}
#endif