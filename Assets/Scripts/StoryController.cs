using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySceneController : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DialogueLine[] openingLines;

    private void Start()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("StorySceneController needs a DialogueManager reference.");
            return;
        }

        dialogueManager.OnDialogueEnd = () =>
        {
            GameManager gameManager = GameManager.EnsureInstance();
            if (gameManager.currentDay < 2)
            {
                gameManager.currentDay = 2;
                Debug.Log("Day1 intro finished. currentDay set to 2 before entering BathhouseMain.");
            }

            SceneManager.LoadScene("BathhouseMain");
        };

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
