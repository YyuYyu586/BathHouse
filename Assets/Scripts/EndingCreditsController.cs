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
        "糖尿病不是一句“要自律”就能解决的事。\n\n" +
        "它关乎身体，\n" +
        "也关乎每天的选择、担心、疲惫和坚持。\n\n" +
        "测血糖不是审判，\n" +
        "而是一种提醒：\n" +
        "提醒我们更了解自己的身体，\n" +
        "也提醒我们可以慢慢学习照顾自己。\n\n" +
        "感到害怕、沮丧、困惑，\n" +
        "并不代表你做得不好。\n\n" +
        "真正重要的，\n" +
        "不是一次就变得完美，\n" +
        "而是在理解和支持中，\n" +
        "一点一点找回生活的节奏。\n\n" +
        "愿每一只不安的鼠鼠，\n" +
        "都能被温柔地听见。\n\n" +
        "愿每一次理解，\n" +
        "都能让沉重的心轻一点。";

    [Header("Scroll")]
    public float scrollSpeed = 60f;
    public float startY = -500f;
    public float endY = 700f;

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

        RectTransform textRect = creditsText.rectTransform;
        Vector2 position = textRect.anchoredPosition;
        position.y = startY;
        textRect.anchoredPosition = position;

        creditsRoutine = StartCoroutine(PlayCreditsRoutine(textRect));
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

    private IEnumerator PlayCreditsRoutine(RectTransform textRect)
    {
        float distance = Mathf.Abs(endY - startY);
        float duration = Mathf.Max(0.1f, distance / Mathf.Max(1f, scrollSpeed));
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector2 position = textRect.anchoredPosition;
            position.y = Mathf.Lerp(startY, endY, t);
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
