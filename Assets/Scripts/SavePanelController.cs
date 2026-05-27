using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class SavePanelController : MonoBehaviour
{
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

        PlayerPrefs.SetInt("currentDay", gameManager.currentDay);
        PlayerPrefs.SetInt("playerGold", gameManager.playerGold);
        PlayerPrefs.SetInt("playerHP", gameManager.playerHP);
        PlayerPrefs.SetInt("playerSP", gameManager.playerSP);
        PlayerPrefs.SetInt("soapCount", gameManager.soapCount);
        PlayerPrefs.SetInt("teaCount", gameManager.teaCount);
        PlayerPrefs.SetInt("waterLadleCount", gameManager.waterLadleCount);
        PlayerPrefs.SetInt("towelCount", gameManager.towelCount);
        PlayerPrefs.SetInt("hasWaterLadle", gameManager.hasWaterLadle ? 1 : 0);
        PlayerPrefs.SetInt("hasGoldenTowel", gameManager.hasGoldenTowel ? 1 : 0);
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
        PlayerPrefs.DeleteKey("currentDay");
        PlayerPrefs.DeleteKey("playerGold");
        PlayerPrefs.DeleteKey("playerHP");
        PlayerPrefs.DeleteKey("playerSP");
        PlayerPrefs.DeleteKey("soapCount");
        PlayerPrefs.DeleteKey("teaCount");
        PlayerPrefs.DeleteKey("waterLadleCount");
        PlayerPrefs.DeleteKey("towelCount");
        PlayerPrefs.DeleteKey("hasWaterLadle");
        PlayerPrefs.DeleteKey("hasGoldenTowel");
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
