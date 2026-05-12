using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Minimal turn-based combat controller for CombatScene.
public class BattleManager : MonoBehaviour
{
    [Header("Battle Stats")]
    public int maxPlayerHP = 100;
    public int maxEnemyHP = 60;
    public int playerAttackDamage = 18;
    public int enemyAttackDamage = 12;
    public float enemyTurnDelay = 0.7f;
    public float victorySceneDelay = 0.8f;

    [Header("Optional UI References")]
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI battleMessageText;
    public Button attackButton;
    public Button defendButton;
    public Slider playerHPSlider;
    public Slider enemyHPSlider;
    public GameObject commandPanel;
    public GameObject victoryPanel;

    private int playerHP;
    private int enemyHP;
    private bool isDefending;
    private bool battleEnded;
    private bool playerTurn;

    // Keeps CombatScene runnable even before a BattleManager object is manually added.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForCombatScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreateForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreateForScene(scene);
    }

    private static void TryCreateForScene(Scene scene)
    {
        if (scene.name != "CombatScene" || FindObjectOfType<BattleManager>() != null)
            return;

        new GameObject("BattleManager").AddComponent<BattleManager>();
    }

    private void Start()
    {
        BindExistingOrCreateUI();
        StartBattle();
    }

    // Initializes the one-battle state. Player HP is read from GameManager when available.
    private void StartBattle()
    {
        playerHP = GameManager.Instance != null ? Mathf.Clamp(GameManager.Instance.playerHP, 1, maxPlayerHP) : maxPlayerHP;
        enemyHP = maxEnemyHP;
        playerTurn = true;
        battleEnded = false;
        isDefending = false;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (commandPanel != null)
            commandPanel.SetActive(true);

        SetButtonsInteractable(true);
        RefreshUI("A bathhouse troublemaker appears.");
    }

    // Called by the Attack button.
    public void OnAttackButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();
        enemyHP = Mathf.Max(0, enemyHP - playerAttackDamage);
        RefreshUI("Player attacks for " + playerAttackDamage + " damage.");

        if (enemyHP <= 0)
        {
            WinBattle();
            return;
        }

        StartCoroutine(EnemyTurn());
    }

    // Called by the Defend button.
    public void OnDefendButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();
        isDefending = true;
        RefreshUI("Player defends. Incoming damage is reduced.");
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        playerTurn = false;
        SetButtonsInteractable(false);

        yield return new WaitForSeconds(enemyTurnDelay);

        int damage = isDefending ? Mathf.CeilToInt(enemyAttackDamage * 0.5f) : enemyAttackDamage;
        isDefending = false;
        playerHP = Mathf.Max(0, playerHP - damage);
        RefreshUI("Enemy attacks for " + damage + " damage.");

        if (playerHP <= 0)
        {
            LoseBattle();
            yield break;
        }

        playerTurn = true;
        SetButtonsInteractable(true);
    }

    private void WinBattle()
    {
        battleEnded = true;
        playerTurn = false;
        SavePlayerHP();
        SetButtonsInteractable(false);

        if (commandPanel != null)
            commandPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        RefreshUI("Victory!");
        BeginPostBattleStory();
    }

    private void LoseBattle()
    {
        battleEnded = true;
        playerTurn = false;
        SavePlayerHP();
        SetButtonsInteractable(false);
        RefreshUI("Defeat.");
    }

    // Starts the post-battle story scene after the player can see the victory state.
    public void BeginPostBattleStory()
    {
        StartCoroutine(LoadAfterCombatScene());
    }

    private IEnumerator LoadAfterCombatScene()
    {
        yield return new WaitForSeconds(victorySceneDelay);
        SceneManager.LoadScene("AfterCombatScene");
    }

    private bool CanPlayerAct()
    {
        return !battleEnded && playerTurn;
    }

    private void SavePlayerHP()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.playerHP = playerHP;
    }

    private void RefreshUI(string message)
    {
        if (playerHPText != null)
            playerHPText.text = "Player HP: " + playerHP + " / " + maxPlayerHP;

        if (enemyHPText != null)
            enemyHPText.text = "Enemy HP: " + enemyHP + " / " + maxEnemyHP;

        if (battleMessageText != null)
            battleMessageText.text = message;

        if (playerHPSlider != null)
        {
            playerHPSlider.maxValue = maxPlayerHP;
            playerHPSlider.value = playerHP;
        }

        if (enemyHPSlider != null)
        {
            enemyHPSlider.maxValue = maxEnemyHP;
            enemyHPSlider.value = enemyHP;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (attackButton != null)
            attackButton.interactable = interactable;

        if (defendButton != null)
            defendButton.interactable = interactable;
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    // Uses the existing CombatScene UI when possible and creates missing TMP labels/panels.
    private void BindExistingOrCreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        if (attackButton == null)
            attackButton = FindButton("Attack");

        if (defendButton == null)
            defendButton = FindButton("Defend") ?? FindButton("Ultimate");

        if (attackButton == null)
            attackButton = CreateButton(canvas.transform, "Attack", new Vector2(-140f, -220f));

        if (defendButton == null)
            defendButton = CreateButton(canvas.transform, "Defend", new Vector2(140f, -220f));

        SetButtonLabel(attackButton, "Attack");
        SetButtonLabel(defendButton, "Defend");
        ConfigureButtonVisuals(attackButton);
        ConfigureButtonVisuals(defendButton);

        attackButton.onClick.RemoveListener(OnAttackButton);
        defendButton.onClick.RemoveListener(OnDefendButton);
        attackButton.onClick.AddListener(OnAttackButton);
        defendButton.onClick.AddListener(OnDefendButton);

        if (commandPanel == null && attackButton.transform.parent == defendButton.transform.parent)
            commandPanel = attackButton.transform.parent.gameObject;

        if (playerHPText == null)
            playerHPText = CreateText(canvas.transform, "PlayerHPText", new Vector2(-260f, 210f), "Player HP");

        if (enemyHPText == null)
            enemyHPText = CreateText(canvas.transform, "EnemyHPText", new Vector2(260f, 210f), "Enemy HP");

        if (playerHPSlider == null)
            playerHPSlider = CreateHPSlider(canvas.transform, "PlayerHPSlider", new Vector2(-260f, 170f));

        if (enemyHPSlider == null)
            enemyHPSlider = CreateHPSlider(canvas.transform, "EnemyHPSlider", new Vector2(260f, 170f));

        if (battleMessageText == null)
            battleMessageText = CreateText(canvas.transform, "BattleMessageText", new Vector2(0f, -120f), "");

        if (victoryPanel == null)
            victoryPanel = CreateVictoryPanel(canvas.transform);
    }

    private Button FindButton(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<Button>() : null;
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        return canvas;
    }

    private Button CreateButton(Transform parent, string label, Vector2 position)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(180f, 64f);
        rect.anchoredPosition = position;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.92f, 0.92f, 0.92f, 1f);

        CreateText(buttonObject.transform, "Text (TMP)", Vector2.zero, label);
        Button button = buttonObject.GetComponent<Button>();
        ConfigureButtonVisuals(button);
        return button;
    }

    private Slider CreateHPSlider(Transform parent, string objectName, Vector2 position)
    {
        GameObject sliderObject = new GameObject(objectName, typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);

        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300f, 24f);
        rect.anchoredPosition = position;

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(sliderObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;
        backgroundObject.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f, 1f);

        GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2f, 2f);
        fillAreaRect.offsetMax = new Vector2(-2f, -2f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillObject.GetComponent<Image>().color = new Color(0.85f, 0.12f, 0.12f, 1f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.fillRect = fillRect;
        slider.targetGraphic = fillObject.GetComponent<Image>();
        return slider;
    }

    private void ConfigureButtonVisuals(Button button)
    {
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        colors.highlightedColor = new Color(1f, 0.92f, 0.45f, 1f);
        colors.pressedColor = new Color(0.95f, 0.45f, 0.18f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.65f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    private TextMeshProUGUI CreateText(Transform parent, string objectName, Vector2 position, string text)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420f, 60f);
        rect.anchoredPosition = position;

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 28f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return label;
    }

    private GameObject CreateVictoryPanel(Transform parent)
    {
        GameObject panel = new GameObject("VictoryPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420f, 180f);
        rect.anchoredPosition = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.72f);

        TextMeshProUGUI label = CreateText(panel.transform, "VictoryText", Vector2.zero, "Victory");
        label.fontSize = 42f;

        panel.SetActive(false);
        return panel;
    }

    private void SetButtonLabel(Button button, string label)
    {
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = label;
    }
}
