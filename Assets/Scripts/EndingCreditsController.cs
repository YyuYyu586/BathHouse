using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingCreditsController : MonoBehaviour
{
    [Header("UI")]
    public GameObject endingCreditsPanel;
    public TextMeshProUGUI creditsText;

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

        RectTransform textRect = creditsText.rectTransform;
        Vector2 position = textRect.anchoredPosition;
        position.y = startY;
        textRect.anchoredPosition = position;

        creditsRoutine = StartCoroutine(PlayCreditsRoutine(textRect));
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
