using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Minimal turn-based battle controller for CombatScene.
// All main UI objects should already exist in the scene and be assigned in the Inspector.
public class BattleManager : MonoBehaviour
{
    [System.Serializable]
    private class EnemyDayData
    {
        public int day = 2;
        public string enemyName = "Monster";
        public int maxHP = 80;
        public int attackDamage = 8;
        public int goldReward = 10;

        public EnemyDayData(int day, string enemyName, int maxHP, int attackDamage, int goldReward)
        {
            this.day = day;
            this.enemyName = enemyName;
            this.maxHP = maxHP;
            this.attackDamage = attackDamage;
            this.goldReward = goldReward;
        }
    }

    [Header("Player Stats")]
    [SerializeField] private int maxPlayerHP = 100;
    [SerializeField] private int maxPlayerSP = 30;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int blurDamage = 18;
    [SerializeField] private int blurSPCost = 5;
    [SerializeField] private bool ultimateUnlocked = false;
    [SerializeField] private int ultimateDamage = 35;
    [SerializeField] private int ultimateSPCost = 20;

    [Header("Enemy Stats")]
    [SerializeField] private int maxEnemyHP = 80;
    [SerializeField] private int enemyAttackDamage = 8;
    [SerializeField] private float enemyTurnDelay = 0.8f;
    [SerializeField] private float fillSmoothTime = 0.2f;
    [SerializeField] private EnemyDayData[] enemiesByDay =
    {
        new EnemyDayData(2, "Bubble Rookie", 70, 7, 10),
        new EnemyDayData(3, "Mud Slime", 85, 8, 12),
        new EnemyDayData(4, "Noise Bubble", 100, 9, 14),
        new EnemyDayData(5, "Tile Monster", 120, 10, 16),
        new EnemyDayData(6, "Storm Scrub", 140, 12, 18),
        new EnemyDayData(7, "Bathhouse Boss", 180, 14, 30)
    };
    [SerializeField] private int defeatGoldRewardDivisor = 2;

    [Header("Failsafe")]
    [SerializeField] private int bathGodTurnLimit = 8;

    [Header("Player UI")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image spFillImage;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI playerSPText;
    [SerializeField] private TextMeshProUGUI playerBattleMessageText;
    [SerializeField] private RectTransform playerDamagePopupPoint;

    [Header("Enemy UI")]
    [SerializeField] private Image enemyHPFillImage;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI enemyBattleMessageText;
    [SerializeField] private RectTransform enemyDamagePopupPoint;

    [Header("Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button blurButton;
    [SerializeField] private Button ultimateButton;

    [Header("Victory")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Damage Popup")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private Vector2 playerDamagePopupOffset = new Vector2(0f, -18f);
    [SerializeField] private Vector2 enemyDamagePopupOffset = new Vector2(0f, -18f);

    private int currentPlayerHP;
    private int currentPlayerSP;
    private int currentEnemyHP;
    private int currentDay;
    private int currentEnemyGoldReward;
    private string currentEnemyName = "Monster";
    private GameManager gameManager;
    private bool isPlayerTurn;
    private bool battleEnded;
    private bool bathGodIntervened;
    private int currentRound = 1;
    private Coroutine playerHPFillRoutine;
    private Coroutine playerSPFillRoutine;
    private Coroutine enemyHPFillRoutine;

    private void Start()
    {
        Debug.Log("BattleManager Start");
        LogInspectorReferences();
        ResolveButtonReferences();
        BindButtonEvents();
        ConfigureButtons();
        StartBattle();
    }

    // Resets battle state and initializes every assigned UI field.
    private void StartBattle()
    {
        gameManager = GameManager.EnsureInstance();
        currentDay = gameManager.currentDay;

        EnemyDayData enemyData = GetEnemyForDay(currentDay);
        if (enemyData != null)
        {
            currentEnemyName = enemyData.enemyName;
            maxEnemyHP = enemyData.maxHP;
            enemyAttackDamage = enemyData.attackDamage;
            currentEnemyGoldReward = enemyData.goldReward;
        }
        else
        {
            currentEnemyName = currentDay <= 1 ? "No Battle" : "Monster";
            currentEnemyGoldReward = 0;
        }

        currentPlayerHP = gameManager != null
            ? Mathf.Clamp(gameManager.playerHP, 1, maxPlayerHP)
            : maxPlayerHP;

        currentPlayerSP = gameManager != null
            ? Mathf.Clamp(gameManager.playerSP, 0, maxPlayerSP)
            : maxPlayerSP;

        currentEnemyHP = maxEnemyHP;
        isPlayerTurn = true;
        battleEnded = false;
        bathGodIntervened = false;
        currentRound = 1;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        RefreshActionButtonsForCurrentState("battle start");
        SetPlayerMessage("Choose an action.");
        SetEnemyMessage("Day " + currentDay + ": " + currentEnemyName);
        RefreshAllUI();
        LogBattleState("Battle started");

        if (currentDay <= 1)
        {
            battleEnded = true;
            isPlayerTurn = false;
            currentEnemyHP = 0;
            RefreshEnemyUI();
            SetActionButtonsInteractable(false);
            SetPlayerMessage("Day 1 is story only. Return and start work tomorrow.");
            SetEnemyMessage("No battle today.");
            LogBattleState("Day 1 no battle");

            if (victoryPanel != null)
                victoryPanel.SetActive(true);
        }
    }

    // Attack is the basic no-cost player action.
    public void OnAttackButton()
    {
        Debug.Log("Attack clicked");

        if (!CanPlayerAct())
            return;

        ClearSelectedButton();
        BeginPlayerAction("Attack");
        SetPlayerMessage("You scrub hard. " + currentEnemyName + " takes " + attackDamage + " damage.");
        DealDamageToEnemy(attackDamage, currentEnemyName + " staggered from the scrub: -" + attackDamage + " HP.");
    }

    // Blur costs SP and deals higher damage. Not enough SP does not spend the player turn.
    public void OnBlurButton()
    {
        Debug.Log("Blur clicked");

        if (!CanPlayerAct())
            return;

        ClearSelectedButton();

        if (currentPlayerSP < blurSPCost)
        {
            SetPlayerMessage("SP is not enough.");
            RefreshActionButtonsForCurrentState("blur blocked by SP");
            return;
        }

        BeginPlayerAction("Blur");
        currentPlayerSP = Mathf.Max(0, currentPlayerSP - blurSPCost);
        SetPlayerMessage("Blur washes over the room. " + currentEnemyName + " takes " + blurDamage + " damage.");
        RefreshPlayerUI();
        DealDamageToEnemy(blurDamage, currentEnemyName + " was slowed by the steam: -" + blurDamage + " HP.");
    }

    // Reserved for the future. Current version does not spend the turn.
    public void OnUltimateButton()
    {
        Debug.Log("Ultimate clicked");

        if (!CanPlayerAct())
            return;

        ClearSelectedButton();

        if (!IsUltimateAvailable())
        {
            SetPlayerMessage("Ultimate is not ready yet.");
            RefreshActionButtonsForCurrentState("ultimate unavailable");
            return;
        }

        BeginPlayerAction("Ultimate");
        currentPlayerSP = Mathf.Max(0, currentPlayerSP - ultimateSPCost);
        SetPlayerMessage("Ultimate scrub unleashed! " + currentEnemyName + " takes " + ultimateDamage + " damage.");
        RefreshPlayerUI();
        DealDamageToEnemy(ultimateDamage, currentEnemyName + " was blasted by the ultimate scrub: -" + ultimateDamage + " HP.");
    }

    // Optional hook for a Continue button inside VictoryPanel.
    public void OnVictoryContinueButton()
    {
        SceneManager.LoadScene("AfterCombatScene");
    }

    private void BeginPlayerAction(string actionName)
    {
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        LogBattleState("Player action: " + actionName);
    }

    private void DealDamageToEnemy(int damage, string message)
    {
        currentEnemyHP = Mathf.Max(0, currentEnemyHP - damage);
        RefreshEnemyUI();
        SetEnemyMessage(message);
        SpawnDamagePopup(enemyDamagePopupPoint, damage.ToString(), enemyDamagePopupOffset);
        LogBattleState("Enemy damaged");

        if (currentEnemyHP <= 0)
        {
            WinBattle();
            return;
        }

        if (HasReachedBathGodTurnLimit())
        {
            TriggerBathGodIntervention("Turn limit reached.");
            return;
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        LogBattleState("Enemy turn started");

        yield return new WaitForSeconds(enemyTurnDelay);

        currentPlayerHP = Mathf.Max(0, currentPlayerHP - enemyAttackDamage);
        RefreshPlayerUI();
        SetPlayerMessage(currentEnemyName + " strikes back. You take " + enemyAttackDamage + " damage.");
        SpawnDamagePopup(playerDamagePopupPoint, enemyAttackDamage.ToString(), playerDamagePopupOffset);
        LogBattleState("Enemy attacked");

        if (currentPlayerHP <= 0)
        {
            TriggerBathGodIntervention("Player HP reached zero.");
            yield break;
        }

        currentRound++;
        isPlayerTurn = true;
        RefreshActionButtonsForCurrentState("player turn restored");
        SetPlayerMessage("Choose an action.");
        LogBattleState("Player turn started");
    }

    private void WinBattle()
    {
        if (battleEnded)
            return;

        battleEnded = true;
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        SavePlayerState();
        GiveGoldReward(currentEnemyGoldReward);
        SetEnemyMessage(currentEnemyName + " defeated.");
        LogBattleState("Battle won");

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    private void TriggerBathGodIntervention(string reason)
    {
        if (battleEnded)
            return;

        battleEnded = true;
        isPlayerTurn = false;
        bathGodIntervened = true;
        SetActionButtonsInteractable(false);

        currentPlayerHP = Mathf.Max(1, currentPlayerHP);
        currentEnemyHP = 0;
        SavePlayerState();
        RefreshAllUI();
        SpawnDamagePopup(enemyDamagePopupPoint, "999", enemyDamagePopupOffset);

        int reducedReward = defeatGoldRewardDivisor > 0
            ? currentEnemyGoldReward / defeatGoldRewardDivisor
            : 0;

        GiveGoldReward(reducedReward);
        SetPlayerMessage("The bath god intervenes! A divine scrub finishes today's work.");
        SetEnemyMessage(reason + " " + currentEnemyName + " was cleansed by the bath god.");
        LogBattleState("Bath god intervention: " + reason);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    private bool CanPlayerAct()
    {
        return !battleEnded && isPlayerTurn;
    }

    private bool HasReachedBathGodTurnLimit()
    {
        return bathGodTurnLimit > 0 && currentRound >= bathGodTurnLimit;
    }

    private bool IsUltimateAvailable()
    {
        return ultimateUnlocked && currentPlayerSP >= ultimateSPCost;
    }

    private void SavePlayerState()
    {
        if (gameManager == null)
            gameManager = GameManager.EnsureInstance();

        gameManager.playerHP = currentPlayerHP;
        gameManager.playerSP = currentPlayerSP;
    }

    private void GiveGoldReward(int amount)
    {
        if (amount <= 0)
            return;

        if (gameManager == null)
            gameManager = GameManager.EnsureInstance();

        gameManager.playerGold += amount;
        Debug.Log("Battle reward gold: " + amount + ". Current gold: " + gameManager.playerGold);
    }

    private EnemyDayData GetEnemyForDay(int day)
    {
        if (enemiesByDay == null)
            return null;

        for (int i = 0; i < enemiesByDay.Length; i++)
        {
            if (enemiesByDay[i] != null && enemiesByDay[i].day == day)
                return enemiesByDay[i];
        }

        return GetDefaultEnemyForDay(day);
    }

    private EnemyDayData GetDefaultEnemyForDay(int day)
    {
        switch (day)
        {
            case 2:
                return new EnemyDayData(2, "Bubble Rookie", 70, 7, 10);
            case 3:
                return new EnemyDayData(3, "Mud Slime", 85, 8, 12);
            case 4:
                return new EnemyDayData(4, "Noise Bubble", 100, 9, 14);
            case 5:
                return new EnemyDayData(5, "Tile Monster", 120, 10, 16);
            case 6:
                return new EnemyDayData(6, "Storm Scrub", 140, 12, 18);
            case 7:
                return new EnemyDayData(7, "Bathhouse Boss", 180, 14, 30);
            default:
                return null;
        }
    }

    private void RefreshAllUI()
    {
        RefreshPlayerUI();
        RefreshEnemyUI();
    }

    private void RefreshPlayerUI()
    {
        Debug.Log("Player HP: " + currentPlayerHP + " / " + maxPlayerHP);
        Debug.Log("Player SP: " + currentPlayerSP + " / " + maxPlayerSP);

        playerHPFillRoutine = SetFillAmount(hpFillImage, currentPlayerHP, maxPlayerHP, playerHPFillRoutine, "hpFillImage");
        playerSPFillRoutine = SetFillAmount(spFillImage, currentPlayerSP, maxPlayerSP, playerSPFillRoutine, "spFillImage");

        if (playerHPText != null)
            playerHPText.text = "HP " + currentPlayerHP + " / " + maxPlayerHP;

        if (playerSPText != null)
            playerSPText.text = "SP " + currentPlayerSP + " / " + maxPlayerSP;
    }

    private void RefreshEnemyUI()
    {
        Debug.Log("Enemy HP: " + currentEnemyHP + " / " + maxEnemyHP);

        enemyHPFillRoutine = SetFillAmount(enemyHPFillImage, currentEnemyHP, maxEnemyHP, enemyHPFillRoutine, "enemyHPFillImage");

        if (enemyHPText != null)
            enemyHPText.text = "HP " + currentEnemyHP + " / " + maxEnemyHP;
    }

    private Coroutine SetFillAmount(Image fillImage, int currentValue, int maxValue, Coroutine currentRoutine, string fieldName)
    {
        if (fillImage == null || maxValue <= 0)
        {
            Debug.LogWarning(fieldName + " is not assigned or max value is invalid.");
            return currentRoutine;
        }

        if (fillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning(fieldName + " Image Type is not Filled. Set Image Type = Filled, Fill Method = Horizontal, Fill Origin = Left. Also make sure this is the FillImage, not the FrameImage.");
        }

        float targetFill = Mathf.Clamp01((float)currentValue / maxValue);

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        return StartCoroutine(SmoothFill(fillImage, targetFill));
    }

    private IEnumerator SmoothFill(Image fillImage, float targetFill)
    {
        float startFill = fillImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < fillSmoothTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fillSmoothTime);
            fillImage.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }

        fillImage.fillAmount = targetFill;
    }

    private void SetPlayerMessage(string message)
    {
        if (playerBattleMessageText != null)
        {
            playerBattleMessageText.text = message;
            Debug.Log("PlayerBattleMessageText: " + message);
        }
        else
        {
            Debug.LogWarning("playerBattleMessageText is not assigned.");
        }
    }

    private void SetEnemyMessage(string message)
    {
        if (enemyBattleMessageText != null)
        {
            enemyBattleMessageText.text = message;
            Debug.Log("EnemyBattleMessageText: " + message);
        }
        else
        {
            Debug.LogWarning("enemyBattleMessageText is not assigned.");
        }
    }

    private void BindButtonEvents()
    {
        BindButton(attackButton, OnAttackButton);
        BindButton(blurButton, OnBlurButton);
        BindButton(ultimateButton, OnUltimateButton);
    }

    private void ResolveButtonReferences()
    {
        if (attackButton == null)
            attackButton = FindButtonByName("Attack");

        if (blurButton == null)
            blurButton = FindButtonByName("Blur");

        if (ultimateButton == null)
            ultimateButton = FindButtonByName("Ultimate");

        LogButtonState("after resolving button references");
    }

    private Button FindButtonByName(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        if (buttonObject == null)
        {
            Debug.LogWarning("BattleManager could not find button named " + objectName + ".");
            return null;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
            Debug.LogWarning("BattleManager found " + objectName + " but it has no Button component.");

        return button;
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void ConfigureButtons()
    {
        ConfigureButton(attackButton);
        ConfigureButton(blurButton);
        ConfigureButton(ultimateButton);
    }

    private void ConfigureButton(Button button)
    {
        if (button == null)
            return;

        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        colors.highlightedColor = new Color(1f, 0.92f, 0.45f, 1f);
        colors.pressedColor = new Color(0.95f, 0.45f, 0.18f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.75f);
        colors.fadeDuration = 0.06f;
        button.colors = colors;
    }

    private void SetActionButtonsInteractable(bool interactable)
    {
        if (!interactable)
            ClearSelectedButton();

        SetButtonInteractable(attackButton, interactable);
        SetButtonInteractable(blurButton, interactable);
        SetButtonInteractable(ultimateButton, interactable);
        LogButtonState("set all buttons interactable = " + interactable);
    }

    private void RefreshActionButtonsForCurrentState(string reason)
    {
        bool canAct = !battleEnded && isPlayerTurn;

        SetButtonInteractable(attackButton, canAct);
        SetButtonInteractable(blurButton, canAct && currentPlayerSP >= blurSPCost);
        SetButtonInteractable(ultimateButton, canAct && IsUltimateAvailable());
        LogButtonState(reason);
    }

    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private void LogButtonState(string reason)
    {
        Debug.Log(
            "Button state [" + reason + "] " +
            "Attack=" + GetButtonState(attackButton) + ", " +
            "Blur=" + GetButtonState(blurButton) + ", " +
            "Ultimate=" + GetButtonState(ultimateButton));
    }

    private string GetButtonState(Button button)
    {
        if (button == null)
            return "Missing";

        return button.interactable ? "Enabled" : "Disabled";
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void SpawnDamagePopup(RectTransform popupPoint, string text, Vector2 offset)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogWarning("damagePopupPrefab is not assigned. Skipping damage popup.");
            return;
        }

        if (popupPoint == null)
        {
            Debug.LogWarning("Damage popup point is not assigned. Skipping damage popup.");
            return;
        }

        DamagePopup popup = Instantiate(damagePopupPrefab, popupPoint.parent);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        popupRect.anchoredPosition = popupPoint.anchoredPosition + offset;
        popup.SetText(text);
        popup.Play();
    }

    private void LogInspectorReferences()
    {
        LogReference("hpFillImage", hpFillImage);
        LogReference("spFillImage", spFillImage);
        LogReference("enemyHPFillImage", enemyHPFillImage);
        LogReference("playerBattleMessageText", playerBattleMessageText);
        LogReference("enemyBattleMessageText", enemyBattleMessageText);
        LogReference("playerDamagePopupPoint", playerDamagePopupPoint);
        LogReference("enemyDamagePopupPoint", enemyDamagePopupPoint);
        LogReference("damagePopupPrefab", damagePopupPrefab);
        LogReference("attackButton", attackButton);
        LogReference("blurButton", blurButton);
        LogReference("ultimateButton", ultimateButton);
        LogReference("victoryPanel", victoryPanel);

        CheckFillImage("hpFillImage", hpFillImage);
        CheckFillImage("spFillImage", spFillImage);
        CheckFillImage("enemyHPFillImage", enemyHPFillImage);
    }

    private void LogReference(string fieldName, Object reference)
    {
        if (reference == null)
            Debug.LogWarning(fieldName + " is not assigned.");
        else
            Debug.Log(fieldName + " assigned: " + reference.name);
    }

    private void LogBattleState(string context)
    {
        Debug.Log(
            "Battle state [" + context + "] " +
            "round=" + currentRound + ", " +
            "playerTurn=" + isPlayerTurn + ", " +
            "playerHP=" + currentPlayerHP + "/" + maxPlayerHP + ", " +
            "playerSP=" + currentPlayerSP + "/" + maxPlayerSP + ", " +
            "enemyHP=" + currentEnemyHP + "/" + maxEnemyHP + ", " +
            "bathGodIntervened=" + bathGodIntervened);
    }

    private void CheckFillImage(string fieldName, Image image)
    {
        if (image == null)
            return;

        if (image.type != Image.Type.Filled)
            Debug.LogWarning(fieldName + " is assigned, but Image Type is not Filled. Set Image Type = Filled, Fill Method = Horizontal, Fill Origin = Left.");
    }
}
