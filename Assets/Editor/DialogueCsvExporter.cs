using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DialogueCsvExporter
{
    private const string BathhouseMainScenePath = "Assets/Scenes/BathhouseMain.unity";
    private const string AfterCombatScenePath = "Assets/Scenes/AfterCombatScene.unity";
    private const string ExportFolderPath = "Assets/Dialogue/Exported";

    private static readonly string[] CsvHeader =
    {
        "Text_ID",
        "StoryType",
        "Day",
        "Order",
        "Speaker Name",
        "Text",
        "Background Image",
        "Portrait",
        "Side"
    };

    [MenuItem("Tools/Dialogue/Export Dialogue CSVs")]
    public static void ExportDialogueCsvs()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Dictionary<int, List<string[]>> rowsByDay = CreateDayRowBuckets();

        ExportBathhouseRows(rowsByDay);
        ExportAfterCombatRows(rowsByDay);
        WriteCsvFiles(rowsByDay);

        AssetDatabase.Refresh();
    }

    private static void ExportBathhouseRows(Dictionary<int, List<string[]>> rowsByDay)
    {
        EditorSceneManager.OpenScene(BathhouseMainScenePath, OpenSceneMode.Single);

        BathhouseDayStoryController controller = Object.FindObjectOfType<BathhouseDayStoryController>(true);
        if (controller == null)
        {
            Debug.LogWarning("Dialogue CSV export: BathhouseDayStoryController was not found in BathhouseMain.");
            return;
        }

        AddDailyDialogueRows(rowsByDay, controller.bathhouseIntroDialogues, "BathhouseIntro");
        AddDailyDialogueRows(rowsByDay, controller.beforeCombatDialogues, "BeforeCombat");
    }

    private static void ExportAfterCombatRows(Dictionary<int, List<string[]>> rowsByDay)
    {
        EditorSceneManager.OpenScene(AfterCombatScenePath, OpenSceneMode.Single);

        AfterCombatStoryController controller = Object.FindObjectOfType<AfterCombatStoryController>(true);
        if (controller == null)
        {
            Debug.LogWarning("Dialogue CSV export: AfterCombatStoryController was not found in AfterCombatScene.");
            return;
        }

        AddDailyDialogueRows(rowsByDay, controller.afterCombatDialogues, "AfterCombat");
    }

    private static Dictionary<int, List<string[]>> CreateDayRowBuckets()
    {
        Dictionary<int, List<string[]>> rowsByDay = new Dictionary<int, List<string[]>>();

        for (int day = 2; day <= 7; day++)
            rowsByDay.Add(day, new List<string[]>());

        return rowsByDay;
    }

    private static void AddDailyDialogueRows(
        Dictionary<int, List<string[]>> rowsByDay,
        DailyDialogue[] dailyDialogues,
        string storyType)
    {
        if (dailyDialogues == null)
            return;

        for (int index = 1; index <= 6 && index < dailyDialogues.Length; index++)
        {
            int day = index + 1;
            DailyDialogue dailyDialogue = dailyDialogues[index];
            if (dailyDialogue == null || dailyDialogue.lines == null || dailyDialogue.lines.Length == 0)
                continue;

            for (int i = 0; i < dailyDialogue.lines.Length; i++)
            {
                DialogueLine line = dailyDialogue.lines[i];
                if (line == null)
                    continue;

                int order = i + 1;
                rowsByDay[day].Add(CreateRow(day, storyType, order, line));
            }
        }
    }

    private static string[] CreateRow(int day, string storyType, int order, DialogueLine line)
    {
        string textId = "D" + day + "_" + storyType + "_" + order.ToString("000");
        string backgroundName = line.backgroundImage != null ? line.backgroundImage.name : "";
        string portraitName = line.portrait != null ? line.portrait.name : "";
        string side = line.isLeftPortrait ? "Left" : "Right";

        return new[]
        {
            textId,
            storyType,
            day.ToString(),
            order.ToString(),
            line.speakerName ?? "",
            line.text ?? "",
            backgroundName,
            portraitName,
            side
        };
    }

    private static void WriteCsvFiles(Dictionary<int, List<string[]>> rowsByDay)
    {
        Directory.CreateDirectory(ExportFolderPath);

        StringBuilder exportedFiles = new StringBuilder();
        exportedFiles.AppendLine("Dialogue CSV export complete.");

        foreach (KeyValuePair<int, List<string[]>> pair in rowsByDay)
        {
            int day = pair.Key;
            string relativePath = ExportFolderPath + "/DAY" + day + "_export.csv";
            WriteCsvFile(relativePath, pair.Value);
            exportedFiles.AppendLine(relativePath + " rows: " + pair.Value.Count);
        }

        Debug.Log(exportedFiles.ToString());
    }

    private static void WriteCsvFile(string path, List<string[]> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(ToCsvLine(CsvHeader));

        foreach (string[] row in rows)
            builder.AppendLine(ToCsvLine(row));

        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
    }

    private static string ToCsvLine(string[] values)
    {
        for (int i = 0; i < values.Length; i++)
            values[i] = EscapeCsv(values[i]);

        return string.Join(",", values);
    }

    private static string EscapeCsv(string value)
    {
        if (value == null)
            return "";

        bool needsQuotes = value.Contains(",") ||
                           value.Contains("\"") ||
                           value.Contains("\n") ||
                           value.Contains("\r");

        if (!needsQuotes)
            return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
