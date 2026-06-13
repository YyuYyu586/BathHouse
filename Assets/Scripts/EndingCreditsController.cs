using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EndingCreditsController : MonoBehaviour
{
    [Header("UI")]
    public GameObject endingCreditsPanel;
    public TextMeshProUGUI creditsText;
    public RectTransform viewport;

    [Header("Credits Text")]
    [TextArea(8, 20)]
    public string mainStoryCreditsText;
    [TextArea(8, 20)]
    public string diabetesDlcCreditsText =
        "鼠鼠大澡堂\n" +
        "糖尿病特别活动\n\n\n" +
        "三天的特别活动结束了。\n\n" +
        "这一次，你面对的并不是糖尿病本身，\n" +
        "而是围绕它出现的担心、误解、疲惫和不安。\n" +
        "鼠们不该被责备，\n" +
        "也不该被一句“你要自律”轻轻带过。\n\n" +
        "照顾身体是一件长期的事。\n\n" +
        "理解血糖、学习记录、寻求帮助，\n" +
        "都不是失败的证明，\n" +
        "而是慢慢把生活重新握回手里的方式。\n\n" +
        "愿每一只正在适应变化的鼠鼠，\n" +
        "都能被认真听见。\n\n" +
        "愿每一次解释、陪伴和理解，\n" +
        "都能让心里的雾散开一点。\n\n\n" +
        "感谢游玩\n" +
        "糖尿病特别活动\n\n" +
        "THE END";

    [Header("Scroll")]
    public float scrollSpeed = 60f;
    [FormerlySerializedAs("dlcExtraScrollPadding")]
    public float extraScrollPadding = 300f;
    public float startPadding = 30f;
    public float endWaitSeconds = 1f;
    public float maxScrollSeconds = 180f;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPlaying;
    private bool isReturning;
    private Coroutine creditsRoutine;

    private void Awake()
    {
        if (endingCreditsPanel != null)
            endingCreditsPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying || isReturning)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            ReturnToMainMenu();
        }
    }

    public void PlayCredits()
    {
        if (isPlaying)
            return;

        if (endingCreditsPanel == null)
        {
            Debug.LogWarning("EndingCreditsController endingCreditsPanel is not assigned. Returning to MainMenu.");
            ReturnToMainMenu();
            return;
        }

        if (creditsText == null)
        {
            Debug.LogWarning("EndingCreditsController creditsText is not assigned. Returning to MainMenu.");
            ReturnToMainMenu();
            return;
        }

        isPlaying = true;

        endingCreditsPanel.SetActive(true);
        creditsText.gameObject.SetActive(true);
        ApplyCreditsTextForCurrentMode();

        PrepareCreditsText();

        RectTransform textRect = creditsText.rectTransform;
        RectTransform viewportRect = GetCreditsViewportRect(textRect);

        MoveCreditsToViewportBottom(textRect, viewportRect);

        creditsRoutine = StartCoroutine(PlayCreditsRoutine(textRect, viewportRect));
    }

    private void ApplyCreditsTextForCurrentMode()
    {
        GameManager gm = GameManager.EnsureInstance();
        string selectedText = gm.currentGameMode == GameMode.DiabetesDLC
            ? diabetesDlcCreditsText
            : mainStoryCreditsText;

        if (!string.IsNullOrWhiteSpace(selectedText))
            creditsText.text = selectedText;
    }

    private void PrepareCreditsText()
    {
        creditsText.overflowMode = TextOverflowModes.Overflow;

        Canvas.ForceUpdateCanvases();
        creditsText.ForceMeshUpdate(true, true);

        RectTransform textRect = creditsText.rectTransform;
        float preferredHeight = Mathf.Max(creditsText.preferredHeight, textRect.rect.height);
        Vector2 sizeDelta = textRect.sizeDelta;
        sizeDelta.y = preferredHeight;
        textRect.sizeDelta = sizeDelta;

        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
        Canvas.ForceUpdateCanvases();
        creditsText.ForceMeshUpdate(true, true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
        Canvas.ForceUpdateCanvases();
    }

    private void MoveCreditsToViewportBottom(RectTransform textRect, RectTransform viewportRect)
    {
        RefreshCreditsLayout(textRect);

        for (int i = 0; i < 12; i++)
        {
            GetWorldVerticalBounds(textRect, out _, out float textTop);
            GetWorldVerticalBounds(viewportRect, out float viewportBottom, out _);
            float targetTop = viewportBottom + Mathf.Max(0f, startPadding);

            if (Mathf.Abs(textTop - targetTop) <= 0.01f)
                return;

            Vector2 position = textRect.anchoredPosition;
            position.y += targetTop - textTop;
            textRect.anchoredPosition = position;
            RefreshCreditsLayout(textRect);
        }
    }

    private RectTransform GetCreditsViewportRect(RectTransform textRect)
    {
        if (viewport != null)
            return viewport;

        Canvas canvas = textRect.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.transform is RectTransform canvasRect)
            return canvasRect;

        if (endingCreditsPanel != null && endingCreditsPanel.TryGetComponent(out RectTransform panelRect))
            return panelRect;

        return textRect;
    }

    private IEnumerator PlayCreditsRoutine(RectTransform textRect, RectTransform viewportRect)
    {
        float elapsed = 0f;

        while (!HasCreditsExitedViewport(textRect, viewportRect))
        {
            elapsed += Time.deltaTime;
            if (elapsed >= Mathf.Max(0.1f, maxScrollSeconds))
            {
                Debug.LogWarning($"EndingCreditsController credits scroll exceeded maxScrollSeconds={maxScrollSeconds:0.###}. Returning to MainMenu.");
                ReturnToMainMenu();
                yield break;
            }

            textRect.anchoredPosition += Vector2.up * Mathf.Max(1f, scrollSpeed) * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(Mathf.Max(0f, endWaitSeconds));

        ReturnToMainMenu();
    }

    private void RefreshCreditsLayout(RectTransform textRect)
    {
        Canvas.ForceUpdateCanvases();
        creditsText.ForceMeshUpdate(true, true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
        Canvas.ForceUpdateCanvases();
    }

    private bool HasCreditsExitedViewport(RectTransform textRect, RectTransform viewportRect)
    {
        GetWorldVerticalBounds(textRect, out float textBottom, out _);
        GetWorldVerticalBounds(viewportRect, out _, out float viewportTop);
        return textBottom > viewportTop + Mathf.Max(0f, extraScrollPadding);
    }

    private static void GetWorldVerticalBounds(RectTransform rectTransform, out float bottom, out float top)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        bottom = corners[0].y;
        top = corners[0].y;

        for (int i = 1; i < corners.Length; i++)
        {
            bottom = Mathf.Min(bottom, corners[i].y);
            top = Mathf.Max(top, corners[i].y);
        }
    }

    private void ReturnToMainMenu()
    {
        if (isReturning)
            return;

        isReturning = true;
        isPlaying = false;

        if (creditsRoutine != null)
            StopCoroutine(creditsRoutine);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
