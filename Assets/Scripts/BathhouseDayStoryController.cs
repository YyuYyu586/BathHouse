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
        Debug.Log("BathhouseDayStoryController checking BathhouseIntro dialogue.");

        GameManager gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager.currentDay;
        int introArrayLength = bathhouseIntroDialogues != null ? bathhouseIntroDialogues.Length : 0;
        Debug.Log("[BATH_INTRO] Start, currentDay=" + currentDay +
                  ", currentGameMode=" + gameManager.currentGameMode +
                  ", playedDays=" + FormatPlayedDays() +
                  ", dialogueManager null?=" + (dialogueManager == null) +
                  ", intro array length=" + introArrayLength + ".");
        Debug.Log("Current day: " + currentDay);

        if (gameManager.currentGameMode == GameMode.DiabetesDLC)
        {
            TryPlayTodayDLCBathhouseIntro(currentDay);
            return;
        }

        if (currentDay <= 1 || currentDay > 7)
        {
            Debug.Log("[BATH_INTRO] no dialogue for day " + currentDay + ".");
            Debug.Log("Day " + currentDay + " has no BathhouseIntro dialogue. Skipping.");
            return;
        }

        if (playedBathhouseIntroDays.Contains(currentDay))
        {
            Debug.Log("[BATH_INTRO] skipped because already played day " + currentDay + ".");
            Debug.Log("BathhouseIntro already played for day " + currentDay + ".");
            return;
        }

        if (dialogueManager == null)
        {
            Debug.Log("[BATH_INTRO] no dialogue for day " + currentDay + ", dialogueManager is null.");
            Debug.LogWarning("BathhouseDayStoryController needs a DialogueManager reference.");
            return;
        }

        int index = currentDay - 1;
        Debug.Log("Reading bathhouseIntroDialogues element " + index + " for currentDay " + currentDay + ".");

        if (bathhouseIntroDialogues == null ||
            index < 0 ||
            index >= bathhouseIntroDialogues.Length ||
            bathhouseIntroDialogues[index] == null ||
            bathhouseIntroDialogues[index].lines == null ||
            bathhouseIntroDialogues[index].lines.Length == 0)
        {
            Debug.Log("[BATH_INTRO] no dialogue for day " + currentDay + ".");
            Debug.LogWarning("No BathhouseIntro lines found for day " + currentDay + " at element " + index + ".");
            return;
        }

        playedBathhouseIntroDays.Add(currentDay);
        Debug.Log("[BATH_INTRO] play day " + currentDay + ".");
        Debug.Log("Starting BathhouseIntro dialogue for day " + currentDay + ", lines: " + bathhouseIntroDialogues[index].lines.Length);
        dialogueManager.StartDialogue(bathhouseIntroDialogues[index].lines);
    }

    private void TryPlayTodayDLCBathhouseIntro(int currentDay)
    {
        if (currentDay < 1 || currentDay > 3)
        {
            Debug.Log("[BATH_INTRO] no dialogue for day " + currentDay + ".");
            Debug.LogWarning("DLC Day " + currentDay + " is outside supported BathhouseIntro range Day1-Day3. Skipping.");
            return;
        }

        if (playedBathhouseIntroDays.Contains(currentDay))
        {
            Debug.Log("[BATH_INTRO] skipped because already played day " + currentDay + ".");
            Debug.Log("DLC BathhouseIntro already played for day " + currentDay + ".");
            return;
        }

        if (dialogueManager == null)
        {
            Debug.Log("[BATH_INTRO] no dialogue for day " + currentDay + ", dialogueManager is null.");
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
            Debug.Log("[BATH_INTRO] no dialogue for day " + currentDay + ".");
            Debug.LogWarning("No DLC BathhouseIntro lines found for day " + currentDay + " at element " + index + ". Skipping without using main story dialogue.");
            return;
        }

        playedBathhouseIntroDays.Add(currentDay);
        Debug.Log("[BATH_INTRO] play day " + currentDay + ".");
        Debug.Log("Starting DLC BathhouseIntro dialogue for day " + currentDay + ", lines: " + dlcBathhouseIntroDialogues[index].lines.Length);
        dialogueManager.StartDialogue(dlcBathhouseIntroDialogues[index].lines);
    }

    private string FormatPlayedDays()
    {
        if (playedBathhouseIntroDays.Count == 0)
            return "None";

        return string.Join(",", playedBathhouseIntroDays);
    }
}
