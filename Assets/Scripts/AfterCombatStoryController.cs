using UnityEngine;
using UnityEngine.SceneManagement;

// Plays the post-combat story for the current day, then advances the main loop.
public class AfterCombatStoryController : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueManager dialogueManager;

    [Header("Seven Days Post-Combat Dialogue")]
    public DailyDialogue[] afterCombatDialogues = new DailyDialogue[7];

    private const string BathhouseSceneName = "BathhouseMain";
    private const string MainMenuSceneName = "MainMenu";

    private GameManager gameManager;
    private bool storyEnded;

    private void Start()
    {
        gameManager = GameManager.EnsureInstance();

        DialogueLine[] todayLines = GetTodayDialogueLines();
        if (todayLines == null || todayLines.Length == 0)
        {
            Debug.LogWarning("No after-combat dialogue found for day " + gameManager.currentDay + ". Continuing flow.");
            EndStory();
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogError("AfterCombatStoryController needs a DialogueManager reference. Continuing flow to avoid blocking the demo.");
            EndStory();
            return;
        }

        dialogueManager.OnDialogueEnd = EndStory;
        dialogueManager.StartDialogue(todayLines);
    }

    // Called when DialogueManager finishes all lines, or immediately if today's story is empty.
    private void EndStory()
    {
        if (storyEnded)
            return;

        storyEnded = true;
        gameManager = GameManager.EnsureInstance();

        if (gameManager.IsFinalDay)
        {
            Debug.Log("Day 7 after-combat story finished. Returning to MainMenu without advancing to Day 8.");
            SceneManager.LoadScene(MainMenuSceneName);
            return;
        }

        Debug.Log("AfterCombatScene finished. Advancing from day " + gameManager.currentDay + " to day " + (gameManager.currentDay + 1) + ".");
        gameManager.AdvanceDay();
        SceneManager.LoadScene(BathhouseSceneName);
    }

    private DialogueLine[] GetTodayDialogueLines()
    {
        int index = Mathf.Clamp(gameManager.currentDay, 1, gameManager.maxDay) - 1;

        if (afterCombatDialogues != null &&
            index >= 0 &&
            index < afterCombatDialogues.Length &&
            afterCombatDialogues[index] != null &&
            afterCombatDialogues[index].lines != null &&
            afterCombatDialogues[index].lines.Length > 0)
        {
            return afterCombatDialogues[index].lines;
        }

        // Optional fallback for scenes already configured on DialogueManager.
        if (dialogueManager != null &&
            dialogueManager.allDaysDialogues != null &&
            index >= 0 &&
            index < dialogueManager.allDaysDialogues.Length &&
            dialogueManager.allDaysDialogues[index] != null)
        {
            return dialogueManager.allDaysDialogues[index].lines;
        }

        return null;
    }
}
