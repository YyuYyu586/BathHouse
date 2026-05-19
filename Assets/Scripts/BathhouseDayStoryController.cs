using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plays the current day's before-combat dialogue when BathhouseMain opens.
public class BathhouseDayStoryController : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DailyDialogue[] beforeCombatDialogues = new DailyDialogue[7];

    private static readonly HashSet<int> playedBeforeCombatDays = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPlayedDays()
    {
        playedBeforeCombatDays.Clear();
    }

    private IEnumerator Start()
    {
        // Wait one frame so DialogueManager.Start can finish hiding/resetting the panel first.
        yield return null;
        TryPlayTodayBeforeCombat();
    }

    private void TryPlayTodayBeforeCombat()
    {
        GameManager gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager.currentDay;

        if (currentDay <= 1 || currentDay > 7)
            return;

        if (playedBeforeCombatDays.Contains(currentDay))
            return;

        if (dialogueManager == null)
        {
            Debug.LogWarning("BathhouseDayStoryController needs a DialogueManager reference.");
            return;
        }

        int index = currentDay - 1;
        if (beforeCombatDialogues == null ||
            index < 0 ||
            index >= beforeCombatDialogues.Length ||
            beforeCombatDialogues[index] == null ||
            beforeCombatDialogues[index].lines == null ||
            beforeCombatDialogues[index].lines.Length == 0)
        {
            return;
        }

        playedBeforeCombatDays.Add(currentDay);
        dialogueManager.StartDialogue(beforeCombatDialogues[index].lines);
    }
}
