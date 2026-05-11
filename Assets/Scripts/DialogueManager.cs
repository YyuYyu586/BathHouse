using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public Sprite portrait;

    [Header("勾选代表在左边，不勾选代表在右边")]
    public bool isLeftPortrait = true;

    [TextArea(2, 4)]
    public string text;
    public Sprite backgroundImage;
}

[System.Serializable]
public class DailyDialogue
{
    public string dayName;
    public DialogueLine[] lines;
}

public class DialogueManager : MonoBehaviour
{
    [Header("对话框整体")]
    public GameObject dialoguePanel;

    [Header("UI 绑定")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("左右立绘框")]
    public Image portraitLeft;
    public Image portraitRight;

    [Header("背景图，可不填")]
    public Image backgroundImage;

    [Header("7天的所有剧情配置，可先不管")]
    public DailyDialogue[] allDaysDialogues;

    [Header("打字速度")]
    public float typingSpeed = 0.05f;

    private int currentIndex = 0;
    private bool isTyping = false;
    private DialogueLine[] currentLines;

    void Start()
    {
        if (portraitLeft != null)
            portraitLeft.gameObject.SetActive(false);

        if (portraitRight != null)
            portraitRight.gameObject.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
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
                    EndDialogue();
                }
            }
        }
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("没有可播放的对话。");
            return;
        }

        currentLines = lines;
        currentIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        PlayDialogue(currentLines[currentIndex]);
    }

    void PlayDialogue(DialogueLine line)
    {
        if (nameText != null)
            nameText.text = line.speakerName;

        if (backgroundImage != null && line.backgroundImage != null)
            backgroundImage.sprite = line.backgroundImage;

        if (portraitLeft != null)
            portraitLeft.gameObject.SetActive(false);

        if (portraitRight != null)
            portraitRight.gameObject.SetActive(false);

        if (line.portrait != null)
        {
            if (line.isLeftPortrait)
            {
                if (portraitLeft != null)
                {
                    portraitLeft.sprite = line.portrait;
                    portraitLeft.gameObject.SetActive(true);
                }
            }
            else
            {
                if (portraitRight != null)
                {
                    portraitRight.sprite = line.portrait;
                    portraitRight.gameObject.SetActive(true);
                }
            }
        }

        StopAllCoroutines();
        StartCoroutine(TypeText(line.text));
    }

    void EndDialogue()
    {
        StopAllCoroutines();
        isTyping = false;

        if (portraitLeft != null)
            portraitLeft.gameObject.SetActive(false);

        if (portraitRight != null)
            portraitRight.gameObject.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (nameText != null)
            nameText.text = "";

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}