using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySceneController : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DialogueLine[] openingLines;
    public DialogueLine[] dlcOpeningLines;

    private void Start()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("StorySceneController needs a DialogueManager reference.");
            return;
        }

        GameManager gameManager = GameManager.EnsureInstance();
        bool isDiabetesDLC = gameManager.currentGameMode == GameMode.DiabetesDLC;

        dialogueManager.OnDialogueEnd = () =>
        {
            GameManager currentGameManager = GameManager.EnsureInstance();
            if (currentGameManager.currentGameMode == GameMode.MainStory && currentGameManager.currentDay < 2)
            {
                currentGameManager.currentDay = 2;
                Debug.Log("Day1 intro finished. currentDay set to 2 before entering BathhouseMain.");
            }

            SceneManager.LoadScene("BathhouseMain");
        };

        if (isDiabetesDLC)
        {
            if (dlcOpeningLines != null && dlcOpeningLines.Length > 0)
            {
                dialogueManager.StartDialogue(dlcOpeningLines);
                return;
            }

            Debug.LogWarning("StoryScene has no DLC opening dialogue. Loading BathhouseMain.");
            SceneManager.LoadScene("BathhouseMain");
            return;
        }

        if (openingLines != null && openingLines.Length > 0)
        {
            dialogueManager.StartDialogue(openingLines);
            return;
        }

        if (dialogueManager.allDaysDialogues != null && dialogueManager.allDaysDialogues.Length > 0)
        {
            // Backward-compatible fallback for the current StoryScene setup.
            dialogueManager.StartDialogue(dialogueManager.allDaysDialogues[0].lines);
            return;
        }

        Debug.LogWarning("StoryScene has no opening dialogue. Loading BathhouseMain.");
        SceneManager.LoadScene("BathhouseMain");
    }
}
