using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerTrigger : MonoBehaviour
{
    [Header("对话管理器")]
    public DialogueManager dialogueManager;
    public BathhouseDayStoryController bathhouseDayStoryController;

    [Header("顾客要说的话")]
    public DialogueLine[] lines;

    [Header("状态控制")]
    public GameObject exclamationMark;
    public GameObject combatTrigger;

    private bool playerNear = false;
    private bool hasTalked = false;

    void Start()
    {
        if (bathhouseDayStoryController == null)
            bathhouseDayStoryController = FindObjectOfType<BathhouseDayStoryController>();

        if (exclamationMark != null)
            exclamationMark.SetActive(true);

        if (combatTrigger != null)
            combatTrigger.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("按下了F，playerNear = " + playerNear);

            if (playerNear && !hasTalked)
            {
                if (dialogueManager == null)
                {
                    Debug.LogError("DialogueManager 没拖！");
                    return;
                }

                dialogueManager.OnDialogueEnd = () =>
                {
                    hasTalked = true;

                    if (exclamationMark != null)
                        exclamationMark.SetActive(false);

                    if (combatTrigger != null)
                        combatTrigger.SetActive(true);

                    Debug.Log("接待完成，战斗入口开启。");
                };

                DialogueLine[] dialogueLines = GetDialogueLinesForToday();
                if (dialogueLines == null || dialogueLines.Length == 0)
                {
                    Debug.LogWarning("CustomerTrigger has no dialogue lines to play.");
                    return;
                }

                dialogueManager.StartDialogue(dialogueLines);
            }
        }
    }

    private DialogueLine[] GetDialogueLinesForToday()
    {
        if (bathhouseDayStoryController != null)
        {
            GameManager gameManager = GameManager.EnsureInstance();
            int currentDay = gameManager.currentDay;
            int index = currentDay - 1;

            if (bathhouseDayStoryController.beforeCombatDialogues != null &&
                index >= 0 &&
                index < bathhouseDayStoryController.beforeCombatDialogues.Length &&
                bathhouseDayStoryController.beforeCombatDialogues[index] != null &&
                bathhouseDayStoryController.beforeCombatDialogues[index].lines != null &&
                bathhouseDayStoryController.beforeCombatDialogues[index].lines.Length > 0)
            {
                Debug.Log("Using CSV BeforeCombat dialogue for day " + currentDay + ".");
                return bathhouseDayStoryController.beforeCombatDialogues[index].lines;
            }
        }

        if (lines != null && lines.Length > 0)
        {
            Debug.Log("Using fallback CustomerTrigger.lines.");
            return lines;
        }

        Debug.LogWarning("CustomerTrigger could not find CSV BeforeCombat dialogue or fallback lines.");
        return null;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("正在碰到：" + other.name);

        if (other.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            Debug.Log("玩家离开顾客范围");
        }
    }
}
