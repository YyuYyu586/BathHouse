using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the minimal CombatScene turn battle and keeps the UI bindings simple.
public class BattleManager : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxPlayerHP = 100;
    public int maxPlayerSP = 30;
    public int playerAttackDamage = 10;
    public int blurDamage = 18;
    public int blurSPCost = 10;

    [Header("Enemy Stats")]
    public int maxEnemyHP = 80;
    public int enemyAttackDamage = 8;
    public float enemyTurnDelay = 0.7f;
    public float victorySceneDelay = 0.8f;

    [Header("Player UI")]
    public Slider playerHPSlider;
    public Slider playerSPSlider;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerSPText;

    [Header("Enemy UI")]
    public Slider enemyHPSlider;
    public TextMeshProUGUI enemyHPText;

    [Header("Battle UI")]
    public TextMeshProUGUI battleMessageText;
    public Button attackButton;
    public Button defendButton;
    public Button blurButton;
    public Button ultimateButton; // Optional future skill button. Current version does not require it.
    public GameObject commandPanel;
    public GameObject victoryPanel;

    [Header("Damage Popup UI")]
    public Transform popupParent;
    public Vector2 enemyPopupPosition = new Vector2(0f, 250f);
    public Vector2 playerPopupPosition = new Vector2(-520f, -210f);

    private int playerHP;
    private int playerSP;
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

    // Initializes one battle. Player HP comes from GameManager when available.
    private void StartBattle()
    {
        playerHP = GameManager.Instance != null ? Mathf.Clamp(GameManager.Instance.playerHP, 1, maxPlayerHP) : maxPlayerHP;
        playerSP = maxPlayerSP;
        enemyHP = maxEnemyHP;
        playerTurn = true;
        battleEnded = false;
        isDefending = false;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (commandPanel != null)
            commandPanel.SetActive(true);

        SetActionButtonsInteractable(true);
        RefreshUI("Choose an action.");
    }

    // Attack damages the enemy, then hands control to the enemy turn.
    public void OnAttackButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();
        DamageEnemy(playerAttackDamage, "Monster took " + playerAttackDamage + " damage.");

        if (enemyHP <= 0)
        {
            WinBattle();
            return;
        }

        StartCoroutine(EnemyTurn());
    }

    // Defend reduces only the next incoming enemy attack.
    public void OnDefendButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();
        isDefending = true;
        RefreshUI("Player defends.");
        StartCoroutine(EnemyTurn());
    }

    // Blur costs SP. If there is not enough SP, the player keeps the turn.
    public void OnBlurButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();

        if (playerSP < blurSPCost)
        {
            RefreshUI("Not enough SP for Blur.");
            return;
        }

        playerSP = Mathf.Max(0, playerSP - blurSPCost);
        DamageEnemy(blurDamage, "Monster took " + blurDamage + " damage.");

        if (enemyHP <= 0)
        {
            WinBattle();
            return;
        }

        StartCoroutine(EnemyTurn());
    }

    private void DamageEnemy(int damage, string popupText)
    {
        enemyHP = Mathf.Max(0, enemyHP - damage);
        RefreshUI(popupText);
        ShowDamagePopup(popupText, enemyPopupPosition);
    }

    private IEnumerator EnemyTurn()
    {
        playerTurn = false;
        SetActionButtonsInteractable(false);
        RefreshUI("Enemy turn...");

        yield return new WaitForSeconds(enemyTurnDelay);

        int damage = isDefending ? Mathf.CeilToInt(enemyAttackDamage * 0.5f) : enemyAttackDamage;
        isDefending = false;
        playerHP = Mathf.Max(0, playerHP - damage);

        string popupText = "Player took " + damage + " damage.";
        RefreshUI(popupText);
        ShowDamagePopup(popupText, playerPopupPosition);

        if (playerHP <= 0)
        {
            LoseBattle();
            yield break;
        }

        playerTurn = true;
        SetActionButtonsInteractable(true);
        RefreshUI("Choose an action.");
    }

    private void WinBattle()
    {
        battleEnded = true;
        playerTurn = false;
        SavePlayerHP();
        SetActionButtonsInteractable(false);

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
        SetActionButtonsInteractable(false);
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
            playerHPText.text = "HP " + playerHP + " / " + maxPlayerHP;

        if (playerSPText != null)
            playerSPText.text = "SP " + playerSP + " / " + maxPlayerSP;

        if (enemyHPText != null)
            enemyHPText.text = "HP " + enemyHP + " / " + maxEnemyHP;

        if (battleMessageText != null)
            battleMessageText.text = message;

        RefreshSlider(playerHPSlider, playerHP, maxPlayerHP);
        RefreshSlider(playerSPSlider, playerSP, maxPlayerSP);
        RefreshSlider(enemyHPSlider, enemyHP, maxEnemyHP);
    }

    private void RefreshSlider(Slider slider, int currentValue, int maxValue)
    {
        if (slider == null)
            return;

        slider.minValue = 0;
        slider.maxValue = maxValue;
        slider.value = currentValue;
    }

    private void SetActionButtonsInteractable(bool interactable)
    {
        SetButtonInteractable(attackButton, interactable);
        SetButtonInteractable(defendButton, interactable);
        SetButtonInteractable(blurButton, interactable);
    }

    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void ShowDamagePopup(string text, Vector2 anchoredPosition)
    {
        Transform parent = popupParent != null ? popupParent : FindObjectOfType<Canvas>()?.transform;
        if (parent == null)
            return;

        DamagePopup popup = DamagePopup.Create(parent, anchoredPosition, text);
        popup.Play();
    }

    // Uses existing CombatScene UI when possible and creates missing runtime fallback UI.
    private void BindExistingOrCreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        if (popupParent == null)
            popupParent = canvas.transform;

        if (attackButton == null)
            attackButton = FindButton("AttackButton") ?? FindButton("Attack");

        if (defendButton == null)
            defendButton = FindButton("DefendButton") ?? FindButton("Defend");

        if (blurButton == null)
            blurButton = FindButton("BlurButton") ?? FindButton("Blur");

        if (ultimateButton == null)
            ultimateButton = FindButton("UltimateButton") ?? FindButton("Ultimate");

        if (attackButton == null)
            attackButton = CreateButton(canvas.transform, "AttackButton", new Vector2(-280f, -300f), "Attack");

        if (defendButton == null)
            defendButton = CreateButton(canvas.transform, "DefendButton", new Vector2(0f, -300f), "Defend");

        if (blurButton == null)
            blurButton = CreateButton(canvas.transform, "BlurButton", new Vector2(280f, -300f), "Blur");

        PrepareActionButton(attackButton, "Attack", OnAttackButton);
        PrepareActionButton(defendButton, "Defend", OnDefendButton);
        PrepareActionButton(blurButton, "Blur", OnBlurButton);

        // Ultimate is intentionally optional and unbound for the current version.
        if (ultimateButton != null)
            ConfigureButtonVisuals(ultimateButton);

        if (commandPanel == null)
            commandPanel = FindCommonParent(attackButton, defendButton, blurButton);

        if (playerHPText == null)
            playerHPText = FindText("PlayerHPText") ?? CreateText(canvas.transform, "PlayerHPText", new Vector2(-520f, -210f), "HP");

        if (playerSPText == null)
            playerSPText = FindText("PlayerSPText") ?? CreateText(canvas.transform, "PlayerSPText", new Vector2(-520f, -250f), "SP");

        if (enemyHPText == null)
            enemyHPText = FindText("EnemyHPText") ?? CreateText(canvas.transform, "EnemyHPText", new Vector2(0f, 310f), "HP");

        if (battleMessageText == null)
            battleMessageText = FindText("BattleMessageText") ?? CreateText(canvas.transform, "BattleMessageText", new Vector2(0f, -130f), "");

        if (playerHPSlider == null)
            playerHPSlider = FindSlider("PlayerHPSlider") ?? CreateSlider(canvas.transform, "PlayerHPSlider", new Vector2(-300f, -215f), new Color(0.85f, 0.12f, 0.12f, 1f));

        if (playerSPSlider == null)
            playerSPSlider = FindSlider("PlayerSPSlider") ?? CreateSlider(canvas.transform, "PlayerSPSlider", new Vector2(-300f, -255f), new Color(0.15f, 0.35f, 0.95f, 1f));

        if (enemyHPSlider == null)
            enemyHPSlider = FindSlider("EnemyHPSlider") ?? CreateSlider(canvas.transform, "EnemyHPSlider", new Vector2(0f, 280f), new Color(0.85f, 0.12f, 0.12f, 1f));

        if (victoryPanel == null)
            victoryPanel = CreateVictoryPanel(canvas.transform);
    }

    private void PrepareActionButton(Button button, string label, UnityEngine.Events.UnityAction action)
    {
        SetButtonLabel(button, label);
        ConfigureButtonVisuals(button);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private GameObject FindCommonParent(params Button[] buttons)
    {
        Transform parent = null;

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (parent == null)
            {
                parent = button.transform.parent;
                continue;
            }

            if (button.transform.parent != parent)
                return null;
        }

        return parent != null ? parent.gameObject : null;
    }

    private Button FindButton(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<Button>() : null;
    }

    private TextMeshProUGUI FindText(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<TextMeshProUGUI>() : null;
    }

    private Slider FindSlider(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<Slider>() : null;
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        return canvas;
    }

    private Button CreateButton(Transform parent, string objectName, Vector2 position, string label)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 76f);
        rect.anchoredPosition = position;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.92f, 0.92f, 0.92f, 1f);

        TextMeshProUGUI buttonText = CreateText(buttonObject.transform, "Text (TMP)", Vector2.zero, label);
        buttonText.color = Color.black;
        buttonText.GetComponent<RectTransform>().sizeDelta = rect.sizeDelta;

        Button button = buttonObject.GetComponent<Button>();
        ConfigureButtonVisuals(button);
        return button;
    }

    private Slider CreateSlider(Transform parent, string objectName, Vector2 position, Color fillColor)
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
        fillObject.GetComponent<Image>().color = fillColor;

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
        if (button == null)
            return;

        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        colors.highlightedColor = new Color(1f, 0.92f, 0.45f, 1f);
        colors.pressedColor = new Color(0.95f, 0.45f, 0.18f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.75f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.06f;
        button.colors = colors;
    }

    private TextMeshProUGUI CreateText(Transform parent, string objectName, Vector2 position, string text)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420f, 52f);
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
        if (button == null)
            return;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = label;
    }
}
