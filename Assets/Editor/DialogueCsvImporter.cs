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
    private const string DefaultDay1CsvPath = "Assets/Dialogue/DAY1_Sheet1.csv";
    private const string StoryScenePath = "Assets/Scenes/StoryScene.unity";
    private const string AfterCombatScenePath = "Assets/Scenes/AfterCombatScene.unity";

    private static readonly string[] AfterCombatCsvPaths =
    {
        "Assets/Dialogue/DAY2.csv",
        "Assets/Dialogue/DAY3.csv",
        "Assets/Dialogue/DAY4.csv",
        "Assets/Dialogue/DAY5.csv"
    };

    [MenuItem("Tools/Dialogue/Import Day1 Intro CSV")]
    public static void ImportDay1IntroCsv()
    {
        string csvPath = GetDay1CsvPath();
        if (string.IsNullOrEmpty(csvPath))
            return;

        ImportResult result = ReadStoryTypeLines(csvPath, "Intro", 1);
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

        Debug.Log(
            "Dialogue CSV import complete.\n" +
            "Imported lines: " + result.lines.Count + "\n" +
            "Skipped rows: " + result.skippedRows + "\n" +
            "Missing sprites: " + FormatMissingSprites(result.missingSprites) + "\n" +
            "Target: StorySceneController.openingLines on " + controller.gameObject.name);
    }

    [MenuItem("Tools/Dialogue/Import AfterCombat CSVs")]
    public static void ImportAfterCombatCsvs()
    {
        AfterCombatStoryController controller = GetAfterCombatStoryController();
        if (controller == null)
        {
            Debug.LogError("AfterCombat CSV import failed: AfterCombatStoryController was not found in AfterCombatScene.");
            return;
        }

        EnsureAfterCombatArray(controller);

        Dictionary<int, List<OrderedDialogueLine>> linesByDay = new Dictionary<int, List<OrderedDialogueLine>>();
        Dictionary<string, CsvImportStats> fileStats = new Dictionary<string, CsvImportStats>();
        HashSet<string> missingSprites = new HashSet<string>();
        int totalSkippedBeforeCombat = 0;

        foreach (string csvPath in AfterCombatCsvPaths)
        {
            CsvImportStats stats = ReadAfterCombatLines(csvPath, linesByDay, missingSprites);
            fileStats[csvPath] = stats;
            totalSkippedBeforeCombat += stats.skippedBeforeCombatRows;
        }

        Dictionary<int, int> importedByDay = new Dictionary<int, int>();
        foreach (KeyValuePair<int, List<OrderedDialogueLine>> pair in linesByDay)
        {
            int day = pair.Key;
            int index = day - 1;

            if (index < 0 || index >= controller.afterCombatDialogues.Length)
            {
                Debug.LogWarning("AfterCombat CSV import: day out of range, skipped day " + day);
                continue;
            }

            DailyDialogue dailyDialogue = controller.afterCombatDialogues[index];
            if (dailyDialogue == null)
                dailyDialogue = new DailyDialogue();

            dailyDialogue.dayName = "Day " + day;
            dailyDialogue.lines = pair.Value
                .OrderBy(item => item.order)
                .Select(item => item.line)
                .ToArray();

            controller.afterCombatDialogues[index] = dailyDialogue;
            importedByDay[day] = dailyDialogue.lines.Length;
        }

        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        EditorSceneManager.SaveScene(controller.gameObject.scene);

        Debug.Log(BuildAfterCombatImportSummary(fileStats, importedByDay, totalSkippedBeforeCombat, missingSprites, controller));
    }

    private static string GetDay1CsvPath()
    {
        Directory.CreateDirectory("Assets/Dialogue");

        if (File.Exists(DefaultDay1CsvPath))
            return DefaultDay1CsvPath;

        string absolutePath = EditorUtility.OpenFilePanel("Select Day1 Intro CSV", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(absolutePath))
            return null;

        string projectPath = Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/");
        absolutePath = absolutePath.Replace("\\", "/");

        if (absolutePath.StartsWith(projectPath + "/", StringComparison.OrdinalIgnoreCase))
            return absolutePath.Substring(projectPath.Length + 1);

        return absolutePath;
    }

    private static ImportResult ReadStoryTypeLines(string csvPath, string targetStoryType, int targetDay)
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

            if (!string.Equals(storyType, targetStoryType, StringComparison.OrdinalIgnoreCase) || day != targetDay)
            {
                result.skippedRows++;
                continue;
            }

            DialogueLine line = CreateDialogueLine(row, headers, result.missingSprites);
            result.orderedLines.Add(new OrderedDialogueLine(order, line));
        }

        result.lines = result.orderedLines
            .OrderBy(item => item.order)
            .Select(item => item.line)
            .ToList();

        return result;
    }

    private static CsvImportStats ReadAfterCombatLines(
        string csvPath,
        Dictionary<int, List<OrderedDialogueLine>> linesByDay,
        HashSet<string> missingSprites)
    {
        CsvImportStats stats = new CsvImportStats();

        if (!File.Exists(csvPath))
        {
            Debug.LogWarning("AfterCombat CSV import: file not found: " + csvPath);
            return stats;
        }

        string csvText = File.ReadAllText(csvPath, new UTF8Encoding(false, true));
        List<List<string>> rows = ParseCsv(csvText);
        stats.totalRows = Math.Max(0, rows.Count - 1);

        if (rows.Count == 0)
            return stats;

        Dictionary<string, int> headers = BuildHeaderMap(rows[0]);

        for (int i = 1; i < rows.Count; i++)
        {
            List<string> row = rows[i];
            if (IsEmptyRow(row))
            {
                stats.skippedOtherRows++;
                continue;
            }

            string storyType = GetValue(row, headers, "StoryType");
            if (string.Equals(storyType, "BeforeCombat", StringComparison.OrdinalIgnoreCase))
            {
                stats.skippedBeforeCombatRows++;
                continue;
            }

            if (!string.Equals(storyType, "AfterCombat", StringComparison.OrdinalIgnoreCase))
            {
                stats.skippedOtherRows++;
                continue;
            }

            int day = ParseInt(GetValue(row, headers, "Day"), -1);
            int order = ParseInt(GetValue(row, headers, "Order"), int.MaxValue);
            if (day < 1 || day > 7)
            {
                stats.skippedOtherRows++;
                Debug.LogWarning("AfterCombat CSV import: invalid day in " + csvPath + " row " + (i + 1));
                continue;
            }

            DialogueLine line = CreateDialogueLine(row, headers, missingSprites);
            if (!linesByDay.TryGetValue(day, out List<OrderedDialogueLine> dayLines))
            {
                dayLines = new List<OrderedDialogueLine>();
                linesByDay.Add(day, dayLines);
            }

            dayLines.Add(new OrderedDialogueLine(order, line));
            stats.importedAfterCombatRows++;
        }

        return stats;
    }

    private static StorySceneController GetStorySceneController()
    {
        if (SceneManager.GetActiveScene().path != StoryScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return null;

            EditorSceneManager.OpenScene(StoryScenePath, OpenSceneMode.Single);
        }

        return UnityEngine.Object.FindObjectOfType<StorySceneController>(true);
    }

    private static AfterCombatStoryController GetAfterCombatStoryController()
    {
        if (SceneManager.GetActiveScene().path != AfterCombatScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return null;

            EditorSceneManager.OpenScene(AfterCombatScenePath, OpenSceneMode.Single);
        }

        return UnityEngine.Object.FindObjectOfType<AfterCombatStoryController>(true);
    }

    private static void EnsureAfterCombatArray(AfterCombatStoryController controller)
    {
        if (controller.afterCombatDialogues != null && controller.afterCombatDialogues.Length >= 7)
            return;

        DailyDialogue[] existing = controller.afterCombatDialogues;
        controller.afterCombatDialogues = new DailyDialogue[7];

        if (existing == null)
            return;

        for (int i = 0; i < existing.Length && i < controller.afterCombatDialogues.Length; i++)
            controller.afterCombatDialogues[i] = existing[i];
    }

    private static DialogueLine CreateDialogueLine(
        List<string> row,
        Dictionary<string, int> headers,
        HashSet<string> missingSprites)
    {
        string portraitName = GetValue(row, headers, "Portrait");
        string backgroundName = GetValue(row, headers, "Background Image");

        return new DialogueLine
        {
            speakerName = GetValue(row, headers, "Speaker Name"),
            text = GetValue(row, headers, "Text"),
            portrait = FindSpriteOrNull(portraitName, missingSprites),
            backgroundImage = FindSpriteOrNull(backgroundName, missingSprites),
            isLeftPortrait = IsLeftSide(GetValue(row, headers, "Side"))
        };
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

    private static string FormatMissingSprites(HashSet<string> missingSprites)
    {
        return missingSprites.Count > 0
            ? string.Join(", ", missingSprites.OrderBy(name => name))
            : "None";
    }

    private static string BuildAfterCombatImportSummary(
        Dictionary<string, CsvImportStats> fileStats,
        Dictionary<int, int> importedByDay,
        int totalSkippedBeforeCombat,
        HashSet<string> missingSprites,
        AfterCombatStoryController controller)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("AfterCombat CSV import complete.");

        foreach (KeyValuePair<string, CsvImportStats> pair in fileStats)
        {
            CsvImportStats stats = pair.Value;
            builder.AppendLine(
                pair.Key + ": read " + stats.totalRows +
                " rows, imported AfterCombat " + stats.importedAfterCombatRows +
                ", skipped BeforeCombat " + stats.skippedBeforeCombatRows +
                ", skipped other " + stats.skippedOtherRows);
        }

        foreach (KeyValuePair<int, int> pair in importedByDay.OrderBy(pair => pair.Key))
            builder.AppendLine("Day " + pair.Key + " imported AfterCombat lines: " + pair.Value);

        builder.AppendLine("Total skipped BeforeCombat rows: " + totalSkippedBeforeCombat);
        builder.AppendLine("Missing sprites: " + FormatMissingSprites(missingSprites));
        builder.AppendLine("Target: AfterCombatStoryController.afterCombatDialogues on " + controller.gameObject.name);

        return builder.ToString();
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

    private class CsvImportStats
    {
        public int totalRows;
        public int importedAfterCombatRows;
        public int skippedBeforeCombatRows;
        public int skippedOtherRows;
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
