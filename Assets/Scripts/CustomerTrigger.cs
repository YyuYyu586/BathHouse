using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerTrigger : MonoBehaviour
{
    [Header("对话管理器")]
    public DialogueManager dialogueManager;

    [Header("顾客要说的话")]
    public DialogueLine[] lines;

    [Header("状态控制")]
    public GameObject exclamationMark;
    public GameObject combatTrigger;

    private bool playerNear = false;
    private bool hasTalked = false;

    void Start()
    {
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

                dialogueManager.StartDialogue(lines);
            }
        }
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