using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DialogueCsvImporter
{
    private const string DefaultCsvPath = "Assets/Dialogue/鼠鼠DAY1_Sheet1.csv";
    private const string StoryScenePath = "Assets/Scenes/StoryScene.unity";

    [MenuItem("Tools/Dialogue/Import Day1 Intro CSV")]
    public static void ImportDay1IntroCsv()
    {
        string csvPath = GetCsvPath();
        if (string.IsNullOrEmpty(csvPath))
            return;

        ImportResult result = ReadDay1IntroLines(csvPath);
        if (result.lines.Count == 0)
        {
            Debug.LogWarning("Dialogue CSV import finished, but no Intro Day 1 lines were found.");
            return;
        }

        StorySceneController controller = GetStorySceneController();
        if (controller == null)
        {
            Debug.LogError("Dialogue CSV import failed: StorySceneController was not found in StoryScene.");
            return;
        }

        controller.openingLines = result.lines.ToArray();
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        EditorSceneManager.SaveScene(controller.gameObject.scene);

        string missingSprites = result.missingSprites.Count > 0
            ? string.Join(", ", result.missingSprites.OrderBy(name => name))
            : "None";

        Debug.Log(
            "Dialogue CSV import complete.\n" +
            "Imported lines: " + result.lines.Count + "\n" +
            "Skipped rows: " + result.skippedRows + "\n" +
            "Missing sprites: " + missingSprites + "\n" +
            "Target: StorySceneController.openingLines on " + controller.gameObject.name);
    }

    private static string GetCsvPath()
    {
        Directory.CreateDirectory("Assets/Dialogue");

        if (File.Exists(DefaultCsvPath))
            return DefaultCsvPath;

        string absolutePath = EditorUtility.OpenFilePanel("Select Day1 Intro CSV", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(absolutePath))
            return null;

        string projectPath = Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/");
        absolutePath = absolutePath.Replace("\\", "/");

        if (absolutePath.StartsWith(projectPath + "/", StringComparison.OrdinalIgnoreCase))
            return absolutePath.Substring(projectPath.Length + 1);

        return absolutePath;
    }

    private static ImportResult ReadDay1IntroLines(string csvPath)
    {
        string csvText = File.ReadAllText(csvPath, new UTF8Encoding(false, true));
        List<List<string>> rows = ParseCsv(csvText);
        ImportResult result = new ImportResult();

        if (rows.Count == 0)
            return result;

        Dictionary<string, int> headers = BuildHeaderMap(rows[0]);

        for (int i = 1; i < rows.Count; i++)
        {
            List<string> row = rows[i];
            if (IsEmptyRow(row))
            {
                result.skippedRows++;
                continue;
            }

            string storyType = GetValue(row, headers, "StoryType");
            int day = ParseInt(GetValue(row, headers, "Day"), -1);
            int order = ParseInt(GetValue(row, headers, "Order"), int.MaxValue);

            if (!string.Equals(storyType, "Intro", StringComparison.OrdinalIgnoreCase) || day != 1)
            {
                result.skippedRows++;
                continue;
            }

            string portraitName = GetValue(row, headers, "Portrait");
            string backgroundName = GetValue(row, headers, "Background Image");

            DialogueLine line = new DialogueLine
            {
                speakerName = GetValue(row, headers, "Speaker Name"),
                text = GetValue(row, headers, "Text"),
                portrait = FindSpriteOrNull(portraitName, result.missingSprites),
                backgroundImage = FindSpriteOrNull(backgroundName, result.missingSprites),
                isLeftPortrait = IsLeftSide(GetValue(row, headers, "Side"))
            };

            result.orderedLines.Add(new OrderedDialogueLine(order, line));
        }

        result.lines = result.orderedLines
            .OrderBy(item => item.order)
            .Select(item => item.line)
            .ToList();

        return result;
    }

    private static StorySceneController GetStorySceneController()
    {
        if (SceneManager.GetActiveScene().path != StoryScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return null;

            EditorSceneManager.OpenScene(StoryScenePath, OpenSceneMode.Single);
        }

        StorySceneController controller = UnityEngine.Object.FindObjectOfType<StorySceneController>(true);
        return controller;
    }

    private static Dictionary<string, int> BuildHeaderMap(List<string> headerRow)
    {
        Dictionary<string, int> headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headerRow.Count; i++)
        {
            string header = headerRow[i].Trim();
            if (!headers.ContainsKey(header))
                headers.Add(header, i);
        }

        return headers;
    }

    private static string GetValue(List<string> row, Dictionary<string, int> headers, string headerName)
    {
        if (!headers.TryGetValue(headerName, out int index))
            return "";

        if (index < 0 || index >= row.Count)
            return "";

        return row[index].Trim();
    }

    private static int ParseInt(string value, int fallback)
    {
        return int.TryParse(value, out int parsed) ? parsed : fallback;
    }

    private static bool IsLeftSide(string side)
    {
        return string.Equals(side, "L", StringComparison.OrdinalIgnoreCase);
    }

    private static Sprite FindSpriteOrNull(string spriteName, HashSet<string> missingSprites)
    {
        if (IsNone(spriteName))
            return null;

        string[] guids = AssetDatabase.FindAssets(spriteName + " t:Sprite");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && string.Equals(sprite.name, spriteName, StringComparison.OrdinalIgnoreCase))
                return sprite;
        }

        missingSprites.Add(spriteName);
        Debug.LogWarning("Dialogue CSV import: sprite not found: " + spriteName);
        return null;
    }

    private static bool IsNone(string value)
    {
        return string.IsNullOrWhiteSpace(value) ||
               string.Equals(value.Trim(), "none", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value.Trim(), "null", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEmptyRow(List<string> row)
    {
        return row == null || row.All(string.IsNullOrWhiteSpace);
    }

    private static List<List<string>> ParseCsv(string csvText)
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> currentRow = new List<string>();
        StringBuilder currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < csvText.Length; i++)
        {
            char c = csvText[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    currentField.Append(c);
                }

                continue;
            }

            if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                currentRow.Add(currentField.ToString());
                currentField.Clear();
            }
            else if (c == '\n')
            {
                currentRow.Add(currentField.ToString().TrimEnd('\r'));
                currentField.Clear();
                rows.Add(currentRow);
                currentRow = new List<string>();
            }
            else
            {
                currentField.Append(c);
            }
        }

        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField.ToString());
            rows.Add(currentRow);
        }

        return rows;
    }

    private class ImportResult
    {
        public List<DialogueLine> lines = new List<DialogueLine>();
        public List<OrderedDialogueLine> orderedLines = new List<OrderedDialogueLine>();
        public HashSet<string> missingSprites = new HashSet<string>();
        public int skippedRows;
    }

    private class OrderedDialogueLine
    {
        public readonly int order;
        public readonly DialogueLine line;

        public OrderedDialogueLine(int order, DialogueLine line)
        {
            this.order = order;
            this.line = line;
        }
    }
}
