using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.IO;
using UnityEngine;

public static class GoogleSheetsDownloader
{
    public static void DownloadSheetToCsv(string serviceAccountJson, string spreadsheetId, string gid, string savePath)
    {
        if (string.IsNullOrEmpty(serviceAccountJson) || string.IsNullOrEmpty(spreadsheetId) || string.IsNullOrEmpty(gid))
        {
            Debug.LogWarning("missing parameters for downloading sheet");
            return;
        }

        SheetsService service = CreateSheetsService(serviceAccountJson);

        Spreadsheet spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();

        string sheetName = ResolveSheetNameFromGid(spreadsheet, gid);
        if (sheetName == null)
        {
            Debug.LogError("could not find sheet with the specified gid");
            return;
        }

        SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, sheetName);
        ValueRange response = request.Execute();
        IList<IList<object>> values = response.Values;

        Directory.CreateDirectory(Path.GetDirectoryName(savePath));

        StreamWriter writer = new StreamWriter(savePath);
        if (values != null)
        {
            foreach (var t in values)
            {
                System.Collections.IList row = (System.Collections.IList)t;
                for (int c = 0; c < row.Count; c++)
                {
                    string cell = row[c].ToString().Replace("\"", "\"\"");
                    if (cell.Contains(",") || cell.Contains("\""))
                        cell = "\"" + cell + "\"";

                    writer.Write(cell);
                    if (c < row.Count - 1)
                        writer.Write(",");
                }
                writer.WriteLine();
            }
        }
        writer.Close();

        Debug.Log("sheet '" + sheetName + "' saved to " + savePath);
    }

    // resolves sheet name from spreadsheet object and gid
    public static string ResolveSheetNameFromGid(Spreadsheet spreadsheet, string gid)
    {
        int gidInt = int.Parse(gid);

        foreach (Sheet t in spreadsheet.Sheets)
        {
            if (t.Properties.SheetId == gidInt)
                return t.Properties.Title;
        }

        return gid; // fallback
    }

    // creates a SheetsService from json
    public static SheetsService CreateSheetsService(string serviceAccountJson)
    {
        MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serviceAccountJson));
        GoogleCredential credential = GoogleCredential.FromStream(stream)
            .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

        return new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "unitysheetscsv"
        });
    }
}
