using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    [Header("玩家")]
    public Transform player;

    [Header("对话管理器")]
    public DialogueManager dialogueManager;

    [Header("交互距离")]
    public float interactDistance = 1.5f;

    [Header("顾客要说的话")]
    public DialogueLine[] lines;

    [Header("状态控制")]
    public GameObject exclamationMark;
    public GameObject combatTrigger;

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
        if (player == null || dialogueManager == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.F) && !hasTalked)
        {
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