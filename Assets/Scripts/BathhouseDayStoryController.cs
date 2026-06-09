using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plays the current day's short bathhouse intro when BathhouseMain opens.
public class BathhouseDayStoryController : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DailyDialogue[] bathhouseIntroDialogues = new DailyDialogue[7];
    public DailyDialogue[] beforeCombatDialogues = new DailyDialogue[7];
    public DailyDialogue[] dlcBathhouseIntroDialogues = new DailyDialogue[3];
    public DailyDialogue[] dlcBeforeCombatDialogues = new DailyDialogue[3];

    private static readonly HashSet<int> playedBathhouseIntroDays = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPlayedDays()
    {
        ResetPlayedBathhouseIntroDays();
    }

    public static void ResetPlayedBathhouseIntroDays()
    {
        playedBathhouseIntroDays.Clear();
    }

    private IEnumerator Start()
    {
        // Wait one frame so DialogueManager.Start can finish hiding/resetting the panel first.
        yield return null;
        TryPlayTodayBathhouseIntro();
    }

    private void TryPlayTodayBathhouseIntro()
    {
        GameManager gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager.currentDay;

        if (gameManager.currentGameMode == GameMode.DiabetesDLC)
        {
            TryPlayTodayDLCBathhouseIntro(currentDay);
            return;
        }

        if (currentDay <= 1 || currentDay > 7)
        {
            return;
        }

        if (playedBathhouseIntroDays.Contains(currentDay))
        {
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogWarning("BathhouseDayStoryController needs a DialogueManager reference.");
            return;
        }

        int index = currentDay - 1;

        if (bathhouseIntroDialogues == null ||
            index < 0 ||
            index >= bathhouseIntroDialogues.Length ||
            bathhouseIntroDialogues[index] == null ||
            bathhouseIntroDialogues[index].lines == null ||
            bathhouseIntroDialogues[index].lines.Length == 0)
        {
            Debug.LogWarning("No BathhouseIntro lines found for day " + currentDay + " at element " + index + ".");
            return;
        }

        playedBathhouseIntroDays.Add(currentDay);
        dialogueManager.StartDialogue(bathhouseIntroDialogues[index].lines);
    }

    private void TryPlayTodayDLCBathhouseIntro(int currentDay)
    {
        if (currentDay < 1 || currentDay > 3)
        {
            Debug.LogWarning("DLC Day " + currentDay + " is outside supported BathhouseIntro range Day1-Day3. Skipping.");
            return;
        }

        if (playedBathhouseIntroDays.Contains(currentDay))
        {
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogWarning("BathhouseDayStoryController needs a DialogueManager reference.");
            return;
        }

        int index = currentDay - 1;
        if (dlcBathhouseIntroDialogues == null ||
            index < 0 ||
            index >= dlcBathhouseIntroDialogues.Length ||
            dlcBathhouseIntroDialogues[index] == null ||
            dlcBathhouseIntroDialogues[index].lines == null ||
            dlcBathhouseIntroDialogues[index].lines.Length == 0)
        {
            Debug.LogWarning("No DLC BathhouseIntro lines found for day " + currentDay + " at element " + index + ". Skipping without using main story dialogue.");
            return;
        }

        playedBathhouseIntroDays.Add(currentDay);
        dialogueManager.StartDialogue(dlcBathhouseIntroDialogues[index].lines);
    }

    private string FormatPlayedDays()
    {
        if (playedBathhouseIntroDays.Count == 0)
            return "None";

        return string.Join(",", playedBathhouseIntroDays);
    }
}
