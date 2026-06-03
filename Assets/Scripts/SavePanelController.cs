using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class SavePanelController : MonoBehaviour
{
    public const string CurrentDayKey = "currentDay";
    public const string PlayerGoldKey = "playerGold";
    public const string PlayerHPKey = "playerHP";
    public const string PlayerSPKey = "playerSP";
    public const string SoapCountKey = "soapCount";
    public const string TeaCountKey = "teaCount";
    public const string WaterLadleCountKey = "waterLadleCount";
    public const string TowelCountKey = "towelCount";
    public const string HasWaterLadleKey = "hasWaterLadle";
    public const string HasGoldenTowelKey = "hasGoldenTowel";

    public static bool IsPanelOpen { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject savePanel;
    [SerializeField] private bool pauseGameWhenOpen = true;

    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button restartButton;
    [FormerlySerializedAs("quitButton")]
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button settingsButton;

    [Header("Restart")]
    [SerializeField] private string restartSceneName = "MainMenu";

    [Header("Back To Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private float previousTimeScale = 1f;

    private void Awake()
    {
        EnsurePanelReference();

        AutoBindButtons();
        BindButtonEvents();
        ClosePanel();
    }

    public static void ResetPanelState()
    {
        IsPanelOpen = false;
        Time.timeScale = 1f;
        Debug.Log("SavePanel static state reset.");
    }

    public static bool HasSavedGame()
    {
        return PlayerPrefs.HasKey(CurrentDayKey);
    }

    public static bool LoadSavedGame(GameManager gameManager)
    {
        if (gameManager == null)
        {
            Debug.LogWarning("SavePanelController could not load saved game because GameManager is missing.");
            return false;
        }

        if (!HasSavedGame())
        {
            Debug.LogWarning("SavePanelController could not load saved game because no save data exists.");
            return false;
        }

        gameManager.currentDay = Mathf.Clamp(PlayerPrefs.GetInt(CurrentDayKey, gameManager.currentDay), 1, gameManager.maxDay);
        gameManager.playerGold = PlayerPrefs.GetInt(PlayerGoldKey, gameManager.playerGold);
        gameManager.playerHP = PlayerPrefs.GetInt(PlayerHPKey, gameManager.playerHP);
        gameManager.playerSP = PlayerPrefs.GetInt(PlayerSPKey, gameManager.playerSP);
        gameManager.soapCount = PlayerPrefs.GetInt(SoapCountKey, gameManager.soapCount);
        gameManager.teaCount = PlayerPrefs.GetInt(TeaCountKey, gameManager.teaCount);
        gameManager.waterLadleCount = PlayerPrefs.GetInt(WaterLadleCountKey, gameManager.waterLadleCount);
        gameManager.towelCount = PlayerPrefs.GetInt(TowelCountKey, gameManager.towelCount);
        gameManager.hasWaterLadle = PlayerPrefs.GetInt(HasWaterLadleKey, gameManager.hasWaterLadle ? 1 : 0) == 1;
        gameManager.hasGoldenTowel = PlayerPrefs.GetInt(HasGoldenTowelKey, gameManager.hasGoldenTowel ? 1 : 0) == 1;

        Debug.Log("Game loaded. currentDay=" + gameManager.currentDay +
                  ", gold=" + gameManager.playerGold +
                  ", hp=" + gameManager.playerHP +
                  ", sp=" + gameManager.playerSP +
                  ", soap=" + gameManager.soapCount +
                  ", tea=" + gameManager.teaCount +
                  ", hasWaterLadle=" + gameManager.hasWaterLadle +
                  ", hasGoldenTowel=" + gameManager.hasGoldenTowel + ".");

        return true;
    }

    private void Update()
    {
        EnsurePanelReference();

        if (savePanel != null && savePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    public void OpenPanel()
    {
        EnsurePanelReference();

        if (savePanel == null)
        {
            Debug.LogWarning("SavePanelController has no savePanel assigned.");
            return;
        }

        previousTimeScale = Time.timeScale;
        savePanel.SetActive(true);
        IsPanelOpen = true;

        if (pauseGameWhenOpen)
            Time.timeScale = 0f;

        Debug.Log("SavePanel opened.");
    }

    public void ClosePanel()
    {
        EnsurePanelReference();

        if (savePanel != null)
            savePanel.SetActive(false);

        if (pauseGameWhenOpen)
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;

        IsPanelOpen = false;
        Debug.Log("SavePanel closed.");
    }

    public void SaveGame()
    {
        GameManager gameManager = GameManager.EnsureInstance();

        PlayerPrefs.SetInt(CurrentDayKey, gameManager.currentDay);
        PlayerPrefs.SetInt(PlayerGoldKey, gameManager.playerGold);
        PlayerPrefs.SetInt(PlayerHPKey, gameManager.playerHP);
        PlayerPrefs.SetInt(PlayerSPKey, gameManager.playerSP);
        PlayerPrefs.SetInt(SoapCountKey, gameManager.soapCount);
        PlayerPrefs.SetInt(TeaCountKey, gameManager.teaCount);
        PlayerPrefs.SetInt(WaterLadleCountKey, gameManager.waterLadleCount);
        PlayerPrefs.SetInt(TowelCountKey, gameManager.towelCount);
        PlayerPrefs.SetInt(HasWaterLadleKey, gameManager.hasWaterLadle ? 1 : 0);
        PlayerPrefs.SetInt(HasGoldenTowelKey, gameManager.hasGoldenTowel ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Game saved. currentDay=" + gameManager.currentDay +
                  ", gold=" + gameManager.playerGold +
                  ", hp=" + gameManager.playerHP +
                  ", sp=" + gameManager.playerSP +
                  ", soap=" + gameManager.soapCount +
                  ", tea=" + gameManager.teaCount +
                  ", hasCertificate=" + gameManager.hasWaterLadle +
                  ", hasGoldenTowel=" + gameManager.hasGoldenTowel + ".");
    }

    public void RestartGame()
    {
        GameManager gameManager = GameManager.EnsureInstance();
        gameManager.ResetGame();
        ClearSavedGame();

        PrepareForSceneChange();

        Debug.Log("Game restarted. Loading scene: " + restartSceneName);
        SceneManager.LoadScene(restartSceneName);
    }

    public void BackToMainMenu()
    {
        PrepareForSceneChange();
        Debug.Log("BackToMenu button clicked. Loading scene: " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitPanel()
    {
        BackToMainMenu();
    }

    public void OpenSettings()
    {
        Debug.Log("Settings not implemented yet.");
    }

    private void EnsurePanelReference()
    {
        if (savePanel == null)
            savePanel = gameObject;
    }

    private void PrepareForSceneChange()
    {
        if (savePanel != null)
            savePanel.SetActive(false);

        ResetPanelState();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Input.ResetInputAxes();
    }

    private void AutoBindButtons()
    {
        if (saveButton == null)
            saveButton = FindChildButton(new[] { "Save", "SaveButton" }, new string[0]);

        if (restartButton == null)
            restartButton = FindChildButton(new[] { "Restart", "RestartButton" }, new string[0]);

        if (backToMenuButton == null)
            backToMenuButton = FindChildButton(new[] { "BackToMenu", "BackToMenuButton", "MainMenuButton", "quit", "Quit", "QuitButton" }, new string[0]);

        if (closeButton == null)
            closeButton = FindChildButton(new[] { "Close", "CloseButton", "BackButton" }, new string[0], saveButton, restartButton, backToMenuButton);

        if (settingsButton == null)
            settingsButton = FindChildButton(new[] { "Settings", "SettingsButton" }, new string[0], saveButton, restartButton, backToMenuButton, closeButton);
    }

    private Button FindChildButton(string[] names, string[] labels, params Button[] excludedButtons)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (IsExcluded(button, excludedButtons))
                continue;

            foreach (string buttonName in names)
            {
                if (button.name == buttonName)
                    return button;
            }

            string label = GetButtonLabel(button);
            foreach (string expectedLabel in labels)
            {
                if (label == expectedLabel)
                    return button;
            }
        }

        return null;
    }

    private bool IsExcluded(Button button, Button[] excludedButtons)
    {
        foreach (Button excludedButton in excludedButtons)
        {
            if (button == excludedButton)
                return true;
        }

        return false;
    }

    private string GetButtonLabel(Button button)
    {
        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        return label != null ? label.text.Trim() : "";
    }

    private void BindButtonEvents()
    {
        BindButton(saveButton, SaveGame, "saveButton");
        BindButton(restartButton, RestartGame, "restartButton");
        BindButton(backToMenuButton, BackToMainMenu, "backToMenuButton");
        BindButton(closeButton, ClosePanel, "closeButton");
        BindButton(settingsButton, OpenSettings, "settingsButton", false);
    }

    private void ClearSavedGame()
    {
        PlayerPrefs.DeleteKey(CurrentDayKey);
        PlayerPrefs.DeleteKey(PlayerGoldKey);
        PlayerPrefs.DeleteKey(PlayerHPKey);
        PlayerPrefs.DeleteKey(PlayerSPKey);
        PlayerPrefs.DeleteKey(SoapCountKey);
        PlayerPrefs.DeleteKey(TeaCountKey);
        PlayerPrefs.DeleteKey(WaterLadleCountKey);
        PlayerPrefs.DeleteKey(TowelCountKey);
        PlayerPrefs.DeleteKey(HasWaterLadleKey);
        PlayerPrefs.DeleteKey(HasGoldenTowelKey);
        PlayerPrefs.Save();
        Debug.Log("Saved game PlayerPrefs cleared for restart.");
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction action, string fieldName, bool warnIfMissing = true)
    {
        if (button == null)
        {
            if (warnIfMissing)
                Debug.LogWarning("SavePanelController could not bind " + fieldName + ".");

            return;
        }

        button.onClick.RemoveListener(action);

        if (!HasPersistentListener(button, action.Method.Name))
            button.onClick.AddListener(action);

        Debug.Log("SavePanelController bound " + fieldName + " to " + button.name + ".");
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
}
