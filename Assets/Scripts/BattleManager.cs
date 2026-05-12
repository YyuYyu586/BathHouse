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
    [Header("Player Stats")]
    [SerializeField] private int maxPlayerHP = 100;
    [SerializeField] private int maxPlayerSP = 30;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int blurDamage = 18;
    [SerializeField] private int blurSPCost = 5;

    [Header("Enemy Stats")]
    [SerializeField] private int maxEnemyHP = 80;
    [SerializeField] private int enemyAttackDamage = 8;
    [SerializeField] private float enemyTurnDelay = 0.8f;
    [SerializeField] private float fillSmoothTime = 0.2f;

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

    private int currentPlayerHP;
    private int currentPlayerSP;
    private int currentEnemyHP;
    private bool isPlayerTurn;
    private bool battleEnded;
    private Coroutine playerHPFillRoutine;
    private Coroutine playerSPFillRoutine;
    private Coroutine enemyHPFillRoutine;

    private void Start()
    {
        Debug.Log("BattleManager Start");
        LogInspectorReferences();
        BindButtonEvents();
        ConfigureButtons();
        StartBattle();
    }

    // Resets battle state and initializes every assigned UI field.
    private void StartBattle()
    {
        currentPlayerHP = GameManager.Instance != null
            ? Mathf.Clamp(GameManager.Instance.playerHP, 1, maxPlayerHP)
            : maxPlayerHP;

        currentPlayerSP = maxPlayerSP;
        currentEnemyHP = maxEnemyHP;
        isPlayerTurn = true;
        battleEnded = false;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        SetActionButtonsInteractable(true);
        SetPlayerMessage("Choose an action.");
        SetEnemyMessage("");
        RefreshAllUI();
    }

    // Attack is the basic no-cost player action.
    public void OnAttackButton()
    {
        Debug.Log("Attack clicked");

        if (!CanPlayerAct())
            return;

        ClearSelectedButton();
        SetPlayerMessage("Attack!");
        DealDamageToEnemy(attackDamage, "Monster took " + attackDamage + " damage.");
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
            return;
        }

        currentPlayerSP = Mathf.Max(0, currentPlayerSP - blurSPCost);
        SetPlayerMessage("Blur!");
        RefreshPlayerUI();
        DealDamageToEnemy(blurDamage, "Monster took " + blurDamage + " damage.");
    }

    // Reserved for the future. Current version does not spend the turn.
    public void OnUltimateButton()
    {
        Debug.Log("Ultimate clicked");

        if (!CanPlayerAct())
            return;

        ClearSelectedButton();
        SetPlayerMessage("Ultimate is not unlocked.");
    }

    // Optional hook for a Continue button inside VictoryPanel.
    public void OnVictoryContinueButton()
    {
        SceneManager.LoadScene("AfterCombatScene");
    }

    private void DealDamageToEnemy(int damage, string message)
    {
        currentEnemyHP = Mathf.Max(0, currentEnemyHP - damage);
        RefreshEnemyUI();
        SetEnemyMessage(message);
        SpawnDamagePopup(enemyDamagePopupPoint, damage.ToString());

        if (currentEnemyHP <= 0)
        {
            WinBattle();
            return;
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);

        yield return new WaitForSeconds(enemyTurnDelay);

        currentPlayerHP = Mathf.Max(0, currentPlayerHP - enemyAttackDamage);
        RefreshPlayerUI();
        SetPlayerMessage("Player took " + enemyAttackDamage + " damage.");
        SpawnDamagePopup(playerDamagePopupPoint, enemyAttackDamage.ToString());

        if (currentPlayerHP <= 0)
        {
            LoseBattle();
            yield break;
        }

        isPlayerTurn = true;
        SetActionButtonsInteractable(true);
        SetPlayerMessage("Choose an action.");
    }

    private void WinBattle()
    {
        battleEnded = true;
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        SavePlayerHP();
        SetEnemyMessage("Enemy defeated.");

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    private void LoseBattle()
    {
        battleEnded = true;
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        SavePlayerHP();
        SetPlayerMessage("Player was defeated.");
    }

    private bool CanPlayerAct()
    {
        return !battleEnded && isPlayerTurn;
    }

    private void SavePlayerHP()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.playerHP = currentPlayerHP;
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
        SetButtonInteractable(attackButton, interactable);
        SetButtonInteractable(blurButton, interactable);
        SetButtonInteractable(ultimateButton, interactable);
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

    private void SpawnDamagePopup(RectTransform popupPoint, string text)
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
        popupRect.anchoredPosition = popupPoint.anchoredPosition;
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

    private void CheckFillImage(string fieldName, Image image)
    {
        if (image == null)
            return;

        if (image.type != Image.Type.Filled)
            Debug.LogWarning(fieldName + " is assigned, but Image Type is not Filled. Set Image Type = Filled, Fill Method = Horizontal, Fill Origin = Left.");
    }
}
