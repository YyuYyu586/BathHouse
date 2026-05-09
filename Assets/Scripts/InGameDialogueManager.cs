using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InGameDialogueManager : MonoBehaviour
{
    [Header("UI Bindings")]
    public GameObject dialogueUI; // 整个对话界面的父节点 (用来控制显示/隐藏)
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("左右立绘框")]
    public Image portraitLeft;
    public Image portraitRight;
    public Image backgroundImage;

    [Header("Settings")]
    public float typingSpeed = 0.05f;

    private DialogueLine[] currentLines; // 存放当前正在与之对话的NPC的台词
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;

    void Start()
    {
        // 游戏一开始，确保对话UI是关闭的
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
    }

    void Update()
    {
        // 如果当前没有在对话，直接跳过后面的逻辑
        if (!isDialogueActive) return;

        // 按下鼠标左键 或者 E键 继续对话
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLines[currentIndex].text;
                isTyping = false;
            }
            else
            {
                currentIndex++;
                if (currentIndex < currentLines.Length)
                {
                    PlayDialogue(currentLines[currentIndex]);
                }
                else
                {
                    EndDialogue(); // 台词播完了，结束对话
                }
            }
        }
    }

    // 由NPC触发器调用的方法
    public void StartDialogue(DialogueLine[] lines)
    {
        currentLines = lines;
        currentIndex = 0;
        isDialogueActive = true;
        dialogueUI.SetActive(true); // 显示UI界面

        // 【重要】如果你有玩家的移动脚本，这里可以通过代码把玩家定住
        // FindObjectOfType<PlayerController>().enabled = false;

        PlayDialogue(currentLines[currentIndex]);
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueUI.SetActive(false); // 隐藏UI界面

        // 【重要】对话结束，恢复玩家移动
        // FindObjectOfType<PlayerController>().enabled = true;
    }

    private void PlayDialogue(DialogueLine line)
    {
        nameText.text = line.speakerName;

        if (line.backgroundImage != null)
        {
            backgroundImage.sprite = line.backgroundImage;
            backgroundImage.gameObject.SetActive(true);
        }
        else
        {
            // 如果游玩场景不需要背景图（直接看澡堂背景），就把背景图隐藏
            if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
        }

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