using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Plays the post-combat story for the current day, then shows the day transition.
public class AfterCombatStoryController : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueManager dialogueManager;

    [Header("Seven Days Post-Combat Dialogue")]
    public DailyDialogue[] afterCombatDialogues = new DailyDialogue[7];

    [Header("DLC Post-Combat Dialogue")]
    public DailyDialogue[] dlcAfterCombatDialogues = new DailyDialogue[3];

    [Header("Day Transition")]
    public GameObject dayTransitionPanel;
    public TextMeshProUGUI transitionText;
    public Button continueButton;

    [Header("Ending Credits")]
    public EndingCreditsController endingCreditsController;

    private const string BathhouseSceneName = "BathhouseMain";
    private const string MainMenuSceneName = "MainMenu";

    private GameManager gameManager;
    private bool storyEnded;
    private bool transitionContinued;

    private void Start()
    {
        gameManager = GameManager.EnsureInstance();

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnDayTransitionContinue);
            continueButton.onClick.AddListener(OnDayTransitionContinue);
        }
        else
        {
            Debug.LogWarning("AfterCombatStoryController continueButton is not assigned. Drag the DayTransitionPanel Continue Button in Inspector.");
        }

        DialogueLine[] todayLines = GetTodayDialogueLines();
        int linesCount = todayLines != null ? todayLines.Length : 0;
        Debug.Log("AfterCombatScene currentDay = " + gameManager.currentDay + ", mode = " + gameManager.currentGameMode + ", index = " + GetTodayIndex() + ", lines = " + linesCount + ".");

        if (todayLines == null || todayLines.Length == 0)
        {
            Debug.LogWarning("No after-combat dialogue found for day " + gameManager.currentDay + ". Showing DayTransitionPanel.");
            EndStory();
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogError("AfterCombatStoryController needs a DialogueManager reference. Showing DayTransitionPanel to avoid blocking the demo.");
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
        ShowDayTransition();
    }

    private void ShowDayTransition()
    {
        gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager.currentDay;

        if (CanWriteTransitionText())
        {
            if (gameManager.IsFinalDay)
            {
                transitionText.text = "七天结束\n鼠鼠澡堂的故事暂告一段落";
            }
            else
            {
                transitionText.text = "第 " + currentDay + " 天结束\n第 " + (currentDay + 1) + " 天，澡堂重新开门";
            }
        }

        if (dayTransitionPanel != null)
        {
            UpdateContinueButtonText(gameManager.IsFinalDay ? "查看结局" : "进入下一天");
            dayTransitionPanel.SetActive(true);
            Debug.Log("Showing DayTransitionPanel. currentDay = " + currentDay + ", isFinalDay = " + gameManager.IsFinalDay + ".");
        }
        else
        {
            Debug.LogError("AfterCombatStoryController dayTransitionPanel is not assigned. Drag the DayTransitionPanel in Inspector.");
        }
    }

    private bool CanWriteTransitionText()
    {
        if (transitionText == null)
            return false;

        if (continueButton != null && transitionText.transform.IsChildOf(continueButton.transform))
        {
            Debug.LogWarning("AfterCombatStoryController transitionText points to ContinueButton text. Skipping transition text write so the button label stays unchanged.");
            return false;
        }

        return true;
    }

    public void OnDayTransitionContinue()
    {
        if (transitionContinued)
            return;

        transitionContinued = true;
        gameManager = GameManager.EnsureInstance();

        if (gameManager.IsFinalDay)
        {
            Debug.Log("DayTransition Continue clicked on final day. currentDay = " + gameManager.currentDay + ".");

            if (endingCreditsController != null)
            {
                if (dayTransitionPanel != null)
                    dayTransitionPanel.SetActive(false);

                endingCreditsController.PlayCredits();
                return;
            }

            Debug.LogWarning("AfterCombatStoryController endingCreditsController is not assigned. Returning to MainMenu.");
            SceneManager.LoadScene(MainMenuSceneName);
            return;
        }

        int previousDay = gameManager.currentDay;
        gameManager.AdvanceDay();
        Debug.Log("DayTransition Continue clicked. Advanced from day " + previousDay + " to day " + gameManager.currentDay + ", next scene = " + BathhouseSceneName + ".");
        SceneManager.LoadScene(BathhouseSceneName);
    }

    private void UpdateContinueButtonText(string text)
    {
        if (continueButton == null)
            return;

        TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (buttonText == null)
        {
            Debug.LogWarning("AfterCombatStoryController could not find TMP text under continueButton.");
            return;
        }

        buttonText.text = text;
    }

    private DialogueLine[] GetTodayDialogueLines()
    {
        if (gameManager == null)
            gameManager = GameManager.EnsureInstance();

        if (gameManager.currentGameMode == GameMode.DiabetesDLC)
            return GetTodayDLCDialogueLines();

        int index = GetTodayIndex();

        if (afterCombatDialogues != null &&
            index >= 0 &&
            index < afterCombatDialogues.Length &&
            afterCombatDialogues[index] != null &&
            afterCombatDialogues[index].lines != null &&
            afterCombatDialogues[index].lines.Length > 0)
        {
            return afterCombatDialogues[index].lines;
        }

        return null;
    }

    private DialogueLine[] GetTodayDLCDialogueLines()
    {
        int currentDay = gameManager.currentDay;
        if (currentDay < 1 || currentDay > 3)
        {
            Debug.LogWarning("DLC after-combat day " + currentDay + " is outside supported range Day1-Day3. Skipping without using main story dialogue.");
            return null;
        }

        int index = currentDay - 1;
        if (dlcAfterCombatDialogues != null &&
            index >= 0 &&
            index < dlcAfterCombatDialogues.Length &&
            dlcAfterCombatDialogues[index] != null &&
            dlcAfterCombatDialogues[index].lines != null &&
            dlcAfterCombatDialogues[index].lines.Length > 0)
        {
            return dlcAfterCombatDialogues[index].lines;
        }

        Debug.LogWarning("No DLC after-combat dialogue found for day " + currentDay + " at element " + index + ". Showing DayTransitionPanel without using main story dialogue.");
        return null;
    }

    private int GetTodayIndex()
    {
        gameManager = GameManager.EnsureInstance();
        return Mathf.Clamp(gameManager.currentDay, 1, gameManager.maxDay) - 1;
    }
}
