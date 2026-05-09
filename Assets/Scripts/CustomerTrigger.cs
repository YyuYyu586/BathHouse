using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerTrigger : MonoBehaviour
{
    [Header("顾客要说的话")]
    public DialogueLine[] lines;

    [Header("状态控制 (拖入对应的物体)")]
    public GameObject exclamationMark; // 拖入顾客头上的感叹号图标
    public GameObject combatTrigger;   // 拖入右侧蓝色地毯的 CombatTrigger 物体

    private bool isPlayerInRange;
    private bool hasTalked = false;    // 防止重复对话

    void Start()
    {
        // 刚进大厅时：显示感叹号，关闭战斗入口，不让玩家逃课
        if (exclamationMark != null) exclamationMark.SetActive(true);
        if (combatTrigger != null) combatTrigger.SetActive(false);
    }

    void Update()
    {
        // 玩家在范围内，按下 F 键，且还没聊过天
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F) && !hasTalked)
        {
            hasTalked = true; // 标记为已对话

            // 1. 触发大厅的游玩对话 (调用你原本写好的 InGameDialogueManager)
            //FindObjectOfType<InGameDialogueManager>().StartDialogue(lines);
            Debug.Log("接待完成！地毯出现吧！");
            // 2. 隐藏感叹号，表示任务已接
            if (exclamationMark != null) exclamationMark.SetActive(false);

            // 3. 激活右侧蓝色地毯的触发器！现在玩家可以进战斗了
            if (combatTrigger != null) combatTrigger.SetActive(true);

            // 4. (可选) 把顾客的模型隐藏掉，假装他脱衣服进澡堂了
            // gameObject.GetComponent<SpriteRenderer>().enabled = false; 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // 可以在这里加个 UI 显示 "按 F 接待"
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}