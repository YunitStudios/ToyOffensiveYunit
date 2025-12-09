using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

/// credits
/// Code by Katie of https://katie.games
/// credits

public class CSVParser
{
    public static List<string[]> LoadFromCSV(string fileName)
    {
        TextAsset csvText = LoadCSVFile(fileName);

        if(csvText == null)
        {
            Debug.LogError($"{fileName}.csv could not be loaded.");
            return new List<string[]>();
        }

        return ParseCsv(csvText);
    }

    // literally just load the csv file
    private static TextAsset LoadCSVFile(string fileName)
    {
        // load the csv from the projects assets
        string[] guids = AssetDatabase.FindAssets($"{fileName} t:TextAsset");

        if (guids.Length == 0)
        {
            Debug.LogError($"{fileName}.csv could not be found in project");
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        TextAsset textFile = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

        if (textFile == null)
        {
            Debug.LogError("csv file could not be loaded");
            return null;
        }

        return textFile;
    }

    // generic csv parser that splits it into rows and columns
    private static List<string[]> ParseCsv(TextAsset textFile)
    {
        List<string[]> rows = new List<string[]>();
        if (textFile == null) return rows;

        string text = textFile.text; // the contents of the text file

        List<string> currentRow = new List<string>();
        string currentColumn = "";
        bool inQuotes = false;

        // iterate over all the columns and rows
        for (int i = 0; i < text.Length; i++)
        {
            // iterate each char so we can see if its quotes, commas or a newline
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // check if its an escaped quote
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        currentColumn += '"';
                        i++; // skip the escaped quote
                    }
                    else
                    {
                        // end of quoted column
                        inQuotes = false;
                    }
                }
                else
                {
                    currentColumn += c;
                }
            }
            else
            {
                // we are outside of quotes now
                if (c == '"')
                { 
                    inQuotes = true;    // now we are in them again
                }
                else if (c == ',')
                {
                    // if there is a comma outside of the quotes it means its the end of this column
                    currentRow.Add(currentColumn);
                    currentColumn = "";
                }
                else if (c == '\n' || c == '\r')
                {
                    // theres a newline outside the quotes so we are at the end of this row
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') 
                    {
                        i++;   // this is just because windows does line endings differently with \r\n (i find it hilarious this is named after typewriter functions)
                    }

                    currentRow.Add(currentColumn);
                    currentColumn = "";

                    if (currentRow.Count > 0)
                        rows.Add(currentRow.ToArray());
                    
                    currentRow.Clear();
                }
                else
                {
                    currentColumn += c;
                }
            }
        }

        // add last row
        currentRow.Add(currentColumn);

        if (currentRow.Count > 0)
        {
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }

    // this one is generic with T because its meant to work with any class that maps to a csv
    public static List<T> MapCSVRows<T>(List<string[]> rows, Func<string[], T> mapRow)
    {
        List<T> list = new List<T>();

        // iterate the rows
        foreach (string[] columns in rows)
        {
            try
            {
                list.Add(mapRow(columns));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to map row: {ex.Message}");
            }
        }
        return list;
    }
}
