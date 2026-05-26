using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogueLine
{
    public string textId;
    public int order;
    public string speakerName;
    public string portraitSourceName;
    public string side;
    public Sprite portrait;

    [Header("Checked = left portrait, unchecked = right portrait")]
    public bool isLeftPortrait = true;

    [TextArea(2, 4)]
    public string text;

    [Header("Optional. Leave empty to keep the current scene background.")]
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
    public Action OnDialogueEnd;
    private Action dialogueEndListeners;

    [Header("Dialogue Panel")]
    public GameObject dialoguePanel;

    [Header("Text")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Portraits")]
    public Image portraitLeft;
    public Image portraitRight;

    [Header("Optional Background Image")]
    public Image backgroundImage;

    [Header("Optional Daily Dialogue Fallback")]
    public DailyDialogue[] allDaysDialogues;

    [Header("Typing")]
    public float typingSpeed = 0.05f;

    private DialogueLine[] currentLines;
    private int currentIndex;
    private bool isTyping;

    private void Start()
    {
        HidePortraits();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (SavePanelController.IsPanelOpen)
            return;

        if (dialoguePanel == null || !dialoguePanel.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
        {
            ContinueDialogue();
        }
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        if (dialoguePanel == null)
        {
            Debug.LogError("DialogueManager needs a DialoguePanel reference.");
            return;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("DialogueManager received no dialogue lines.");
            return;
        }

        StopAllCoroutines();
        currentLines = lines;
        currentIndex = 0;
        isTyping = false;

        dialoguePanel.SetActive(true);
        PlayDialogue(currentLines[currentIndex]);
    }

    public void AddDialogueEndListener(Action callback)
    {
        dialogueEndListeners -= callback;
        dialogueEndListeners += callback;
    }

    public void RemoveDialogueEndListener(Action callback)
    {
        dialogueEndListeners -= callback;
    }

    private void ContinueDialogue()
    {
        if (currentLines == null || currentLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();

            if (dialogueText != null)
                dialogueText.text = currentLines[currentIndex].text;

            isTyping = false;
            return;
        }

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

    private void PlayDialogue(DialogueLine line)
    {
        if (line == null)
        {
            ContinueDialogue();
            return;
        }

        if (nameText != null)
            nameText.text = line.speakerName;

        // Empty background means "keep the current Unity scene as the background".
        if (backgroundImage != null && line.backgroundImage != null)
        {
            backgroundImage.sprite = line.backgroundImage;
            backgroundImage.gameObject.SetActive(true);
        }

        HidePortraits();

        if (line.portrait != null)
        {
            Image targetPortrait = line.isLeftPortrait ? portraitLeft : portraitRight;
            if (targetPortrait != null)
            {
                targetPortrait.sprite = line.portrait;
                targetPortrait.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log("DialogueManager line has no portrait. Both portrait images are hidden for this line.");
        }

        LogCurrentLine(line);

        StopAllCoroutines();
        StartCoroutine(TypeText(line.text));
    }

    private void EndDialogue()
    {
        StopAllCoroutines();
        isTyping = false;
        currentLines = null;
        currentIndex = 0;

        HidePortraits();

        if (dialogueText != null)
            dialogueText.text = "";

        if (nameText != null)
            nameText.text = "";

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Action endCallback = OnDialogueEnd;
        Action listenerCallbacks = dialogueEndListeners;
        OnDialogueEnd = null;
        endCallback?.Invoke();
        listenerCallbacks?.Invoke();
    }

    private void HidePortraits()
    {
        if (portraitLeft != null)
            portraitLeft.gameObject.SetActive(false);

        if (portraitRight != null)
            portraitRight.gameObject.SetActive(false);
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = "";

        string safeText = text ?? "";

        foreach (char letter in safeText)
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void LogCurrentLine(DialogueLine line)
    {
        int currentDay = GameManager.Instance != null ? GameManager.Instance.currentDay : -1;
        string portraitName = line.portrait != null ? line.portrait.name : "None";
        string preview = GetPreview(line.text, 20);

        Debug.Log(
            "Dialogue line play: currentDay=" + currentDay +
            ", dialogueIndex=" + currentIndex +
            ", lineOrder=" + line.order +
            ", textId=" + line.textId +
            ", speakerName=" + line.speakerName +
            ", portraitSource=" + line.portraitSourceName +
            ", portraitSprite=" + portraitName +
            ", isLeftPortrait=" + line.isLeftPortrait +
            ", side=" + line.side +
            ", textPreview=" + preview);
    }

    private string GetPreview(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text.Length <= maxLength ? text : text.Substring(0, maxLength);
    }
}
