using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingCreditsController : MonoBehaviour
{
    [Header("UI")]
    public GameObject endingCreditsPanel;
    public TextMeshProUGUI creditsText;

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
    public float startY = -500f;
    public float endY = 700f;
    [SerializeField] private float dlcExtraScrollPadding = 300f;

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
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0))
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
        float actualEndY = GetCreditsEndYForCurrentMode();

        RectTransform textRect = creditsText.rectTransform;
        Vector2 position = textRect.anchoredPosition;
        position.y = startY;
        textRect.anchoredPosition = position;

        creditsRoutine = StartCoroutine(PlayCreditsRoutine(textRect, actualEndY));
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

    private float GetCreditsEndYForCurrentMode()
    {
        GameManager gm = GameManager.EnsureInstance();
        if (gm.currentGameMode != GameMode.DiabetesDLC)
            return endY;

        creditsText.ForceMeshUpdate();
        float dlcEndY = startY + creditsText.preferredHeight + dlcExtraScrollPadding;
        return Mathf.Max(endY, dlcEndY);
    }

    private IEnumerator PlayCreditsRoutine(RectTransform textRect, float actualEndY)
    {
        float distance = Mathf.Abs(actualEndY - startY);
        float duration = Mathf.Max(0.1f, distance / Mathf.Max(1f, scrollSpeed));
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector2 position = textRect.anchoredPosition;
            position.y = Mathf.Lerp(startY, actualEndY, t);
            textRect.anchoredPosition = position;
            yield return null;
        }

        ReturnToMainMenu();
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
