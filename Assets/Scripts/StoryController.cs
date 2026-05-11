using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StorySceneController : MonoBehaviour
{
    public DialogueManager dialogueManager;

    void Start()
    {
        int today = GameManager.Instance.currentDay;

        if (dialogueManager.allDaysDialogues.Length >= today)
        {
            DialogueLine[] lines = dialogueManager.allDaysDialogues[today - 1].lines;

            dialogueManager.OnDialogueEnd = () =>
            {
                SceneManager.LoadScene("BathhouseMain");
            };

            dialogueManager.StartDialogue(lines);
        }
        else
        {
            Debug.LogError("剧情配置不足，请检查 allDaysDialogues。");
        }
    }
}