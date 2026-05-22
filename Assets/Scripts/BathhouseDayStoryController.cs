using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plays the current day's short bathhouse intro when BathhouseMain opens.
public class BathhouseDayStoryController : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DailyDialogue[] bathhouseIntroDialogues = new DailyDialogue[7];
    public DailyDialogue[] beforeCombatDialogues = new DailyDialogue[7];

    private static readonly HashSet<int> playedBathhouseIntroDays = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPlayedDays()
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
        Debug.Log("BathhouseDayStoryController checking bathhouse intro dialogue.");

        GameManager gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager.currentDay;
        Debug.Log("Current day: " + currentDay);

        if (currentDay <= 1 || currentDay > 7)
        {
            Debug.Log("Day " + currentDay + " has no before-combat dialogue. Skipping.");
            return;
        }

        if (playedBathhouseIntroDays.Contains(currentDay))
        {
            Debug.Log("BathhouseIntro already played for day " + currentDay + ".");
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogWarning("BathhouseDayStoryController needs a DialogueManager reference.");
            return;
        }

        int index = currentDay - 1;
        Debug.Log("Reading bathhouseIntroDialogues element " + index + ".");

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
        Debug.Log("Starting BathhouseIntro dialogue for day " + currentDay + ", lines: " + bathhouseIntroDialogues[index].lines.Length);
        dialogueManager.StartDialogue(bathhouseIntroDialogues[index].lines);
    }
}
