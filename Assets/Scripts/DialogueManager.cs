using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public Sprite portrait;

    [Header("勾选代表在左边，不勾选在右边")]
    public bool isLeftPortrait = true;

    [TextArea(2, 4)]
    public string text;
    public Sprite backgroundImage;
}

[System.Serializable]
public class DailyDialogue
{
    public string dayName; // 比如 "Day 1"
    public DialogueLine[] lines;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI 绑定")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("左右立绘框")]
    public Image portraitLeft;   // 左侧立绘框
    public Image portraitRight;  // 右侧立绘框
    public Image backgroundImage;

    [Header("7天的所有剧情配置")]
    public DailyDialogue[] allDaysDialogues;
    public float typingSpeed = 0.05f;

    private int currentIndex = 0;
    private bool isTyping = false;
    private DialogueLine[] currentDayLines;

    void Start()
    {
        // 初始隐藏立绘
        portraitLeft.gameObject.SetActive(false);
        portraitRight.gameObject.SetActive(false);

        // --- 核心：从 GameManager 获取今天是第几天 ---
        // 确保你已经创建了下面的 GameManager 脚本
        int today = GameManager.Instance.currentDay;

        // 获取当天的剧情数组（下标从0开始，所以today-1）
        if (allDaysDialogues.Length >= today)
        {
            currentDayLines = allDaysDialogues[today - 1].lines;
            if (currentDayLines.Length > 0)
            {
                PlayDialogue(currentDayLines[currentIndex]);
            }
        }
        else
        {
            Debug.LogError("剧情配置不足！请在 Inspector 面板检查 allDaysDialogues 是否配够了 7 天。");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 如果正在打字，点击则直接显示全文
                StopAllCoroutines();
                dialogueText.text = currentDayLines[currentIndex].text;
                isTyping = false;
            }
            else
            {
                // 如果已经显示完，点击进入下一句
                currentIndex++;
                if (currentIndex < currentDayLines.Length)
                {
                    PlayDialogue(currentDayLines[currentIndex]);
                }
                else
                {
                    // 剧情结束，跳转到大厅
                    SceneManager.LoadScene("BathhouseMain");
                }
            }
        }
    }

    void PlayDialogue(DialogueLine line)
    {
        nameText.text = line.speakerName;

        if (line.backgroundImage != null)
        {
            backgroundImage.sprite = line.backgroundImage;
        }

        // 左右立绘逻辑
        if (line.portrait != null)
        {
            if (line.isLeftPortrait)
            {
                portraitLeft.sprite = line.portrait;
                portraitLeft.gameObject.SetActive(true);
                portraitRight.gameObject.SetActive(false);
            }
            else
            {
                portraitRight.sprite = line.portrait;
                portraitRight.gameObject.SetActive(true);
                portraitLeft.gameObject.SetActive(false);
            }
        }
        else
        {
            portraitLeft.gameObject.SetActive(false);
            portraitRight.gameObject.SetActive(false);
        }

        StartCoroutine(TypeText(line.text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }
}