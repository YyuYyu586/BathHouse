using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject dlcExperiencePanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button continueButton;
    private RectTransform startButtonRect;
    private RectTransform quitButtonRect;
    private Canvas parentCanvas;
    private bool isLoadingScene;

    [Header("Cloud Animation")]
    [SerializeField] private RectTransform[] clouds;
    [SerializeField] private float[] cloudSpeeds = { 18f, 10f, 14f };
    [SerializeField] private float resetLeftX = -1250f;
    [SerializeField] private float resetRightX = 1250f;

    private void Awake()
    {
        SavePanelController.ResetPanelState();
        ResetMenuInputState();
        EnsureButtonBindings();
        RefreshContinueButtonState();
        EnsureClickFallbackReferences();
        BackToMainPanel();

        // 如果 Inspector 没有手动拖引用，就按名字自动找到 MainMenu 里的云。
        if (clouds == null || clouds.Length == 0)
        {
            clouds = new[]
            {
                FindCloud("Cloud_1"),
                FindCloud("Cloud_2"),
                FindCloud("Cloud_3")
            };
        }
    }

    private void Start()
    {
        ResetMenuInputState();
        EnsureButtonBindings();
        RefreshContinueButtonState();
        EnsureClickFallbackReferences();
        BackToMainPanel();
    }

    private void Update()
    {
        HandleMouseClickFallback();
        MoveClouds();
    }

    public void StartGame()
    {
        if (isLoadingScene)
            return;

        OpenModeSelectPanel();
    }

    public void StartGameWithClickSound()
    {
        if (isLoadingScene)
            return;

        StartCoroutine(StartGameWithClickSoundRoutine());
    }

    private System.Collections.IEnumerator StartGameWithClickSoundRoutine()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClickSfx();

        yield return new WaitForSeconds(0.12f);

        OpenModeSelectPanel();
    }

    public void OpenModeSelectPanel()
    {
        SavePanelController.ResetPanelState();
        ResetMenuInputState();
        SetPanelActive(mainPanel, false);
        SetPanelActive(modeSelectPanel, true);
        SetPanelActive(dlcExperiencePanel, false);
        Debug.Log("MainMenu opened mode select panel.");
    }

    public void BackToMainPanel()
    {
        SetPanelActive(mainPanel, true);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(dlcExperiencePanel, false);
    }

    public void ChooseMainStory()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;
        SavePanelController.ResetPanelState();
        ResetMenuInputState();
        GameManager.EnsureInstance().StartMainStory();
        Debug.Log("MainMenu selected MainStory. Loading StoryScene.");
        SceneManager.LoadScene("StoryScene");
    }

    public void OpenDLCExperiencePanel()
    {
        SetPanelActive(mainPanel, false);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(dlcExperiencePanel, true);
        Debug.Log("MainMenu opened DLC experience panel.");
    }

    public void ChooseDLCPlayedMainStory()
    {
        StartDiabetesDLC(true);
    }

    public void ChooseDLCFirstTime()
    {
        StartDiabetesDLC(false);
    }

    public void ContinueGame()
    {
        if (isLoadingScene)
            return;

        if (!SavePanelController.HasSavedGame())
        {
            Debug.LogWarning("MainMenu ContinueGame clicked, but no saved game exists.");
            RefreshContinueButtonState();
            return;
        }

        GameManager gameManager = GameManager.EnsureInstance();
        if (!SavePanelController.LoadSavedGame(gameManager))
            return;

        isLoadingScene = true;
        SavePanelController.ResetPanelState();
        ResetMenuInputState();
        Debug.Log("MainMenu ContinueGame clicked. Loading BathhouseMain.");
        SceneManager.LoadScene("BathhouseMain");
    }

    public void QuitGame()
    {
        Debug.Log("已经点击了退出游戏");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void StartDiabetesDLC(bool hasPlayedMainStory)
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;
        SavePanelController.ResetPanelState();
        ResetMenuInputState();
        GameManager.EnsureInstance().StartDiabetesDLC(hasPlayedMainStory);
        Debug.Log("MainMenu selected DiabetesDLC. hasPlayedMainStory=" + hasPlayedMainStory + ". Loading StoryScene.");
        SceneManager.LoadScene("StoryScene");
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    private RectTransform FindCloud(string cloudName)
    {
        GameObject cloudObject = GameObject.Find(cloudName);
        return cloudObject != null ? cloudObject.GetComponent<RectTransform>() : null;
    }

    private void ResetMenuInputState()
    {
        Time.timeScale = 1f;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Input.ResetInputAxes();
    }

    private void EnsureButtonBindings()
    {
        if (startButton == null)
            startButton = FindButton("StartButton");

        if (quitButton == null)
            quitButton = FindButton("QuitButton");

        BindStartButton();
        BindContinueButton();
        BindButton(quitButton, QuitGame, "QuitGame", "QuitButton");
    }

    private void BindContinueButton()
    {
        if (continueButton == null)
            continueButton = FindButton("ContinueButton");

        if (continueButton == null)
        {
            Debug.LogWarning("MainMenuManager could not find ContinueButton.");
            return;
        }

        if (!HasPersistentListener(continueButton, "ContinueGame"))
        {
            continueButton.onClick.RemoveListener(ContinueGame);
            continueButton.onClick.AddListener(ContinueGame);
        }
    }

    private void RefreshContinueButtonState()
    {
        if (continueButton == null)
            return;

        bool hasSave = SavePanelController.HasSavedGame();
        continueButton.gameObject.SetActive(hasSave);
        continueButton.enabled = hasSave;
        continueButton.interactable = hasSave;
    }

    private void BindStartButton()
    {
        if (startButton == null)
        {
            Debug.LogWarning("MainMenuManager could not find StartButton.");
            return;
        }

        startButton.enabled = true;
        startButton.interactable = true;

        if (HasPersistentListener(startButton, "StartGame") ||
            HasPersistentListener(startButton, "StartGameWithClickSound"))
        {
            return;
        }

        startButton.onClick.RemoveListener(StartGameWithClickSound);
        startButton.onClick.AddListener(StartGameWithClickSound);
    }

    private Button FindButton(string buttonName)
    {
        GameObject buttonObject = GameObject.Find(buttonName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction action, string methodName, string buttonName)
    {
        if (button == null)
        {
            Debug.LogWarning("MainMenuManager could not find " + buttonName + ".");
            return;
        }

        button.enabled = true;
        button.interactable = true;

        if (!HasPersistentListener(button, methodName))
        {
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }

    private void EnsureClickFallbackReferences()
    {
        if (startButton != null && startButtonRect == null)
            startButtonRect = startButton.GetComponent<RectTransform>();

        if (quitButton != null && quitButtonRect == null)
            quitButtonRect = quitButton.GetComponent<RectTransform>();

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }

    private void HandleMouseClickFallback()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Camera eventCamera = null;
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            eventCamera = parentCanvas.worldCamera;

        if (IsButtonClicked(startButton, startButtonRect, eventCamera))
        {
            Debug.Log("MainMenu StartButton fallback click.");
            StartGameWithClickSound();
            return;
        }

        if (IsButtonClicked(quitButton, quitButtonRect, eventCamera))
        {
            Debug.Log("MainMenu QuitButton fallback click.");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayClickSfx();

            QuitGame();
        }
    }

    private bool IsButtonClicked(Button button, RectTransform rectTransform, Camera eventCamera)
    {
        if (button == null || rectTransform == null)
            return false;

        if (!button.enabled || !button.interactable || !button.gameObject.activeInHierarchy)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, eventCamera);
    }

    private bool HasPersistentListener(Button button, string methodName)
    {
        int eventCount = button.onClick.GetPersistentEventCount();

        for (int i = 0; i < eventCount; i++)
        {
            if (button.onClick.GetPersistentMethodName(i) == methodName)
                return true;
        }

        return false;
    }

    // 让云从左往右缓慢移动，移出画面后回到左侧继续循环。
    private void MoveClouds()
    {
        if (clouds == null)
        {
            return;
        }

        for (int i = 0; i < clouds.Length; i++)
        {
            RectTransform cloud = clouds[i];
            if (cloud == null)
            {
                continue;
            }

            float speed = i < cloudSpeeds.Length ? cloudSpeeds[i] : cloudSpeeds[0];
            Vector2 position = cloud.anchoredPosition;
            position.x += speed * Time.deltaTime;

            if (position.x > resetRightX)
            {
                position.x = resetLeftX;
            }

            cloud.anchoredPosition = position;
        }
    }
}
