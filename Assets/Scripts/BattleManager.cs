using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the CombatScene turn battle. Main UI is expected to exist in the scene.
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
    [SerializeField] private Slider playerHPSlider;
    [SerializeField] private Slider playerSPSlider;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI playerSPText;

    [Header("Enemy UI")]
    [SerializeField] private Slider enemyHPSlider;
    [SerializeField] private TextMeshProUGUI enemyHPText;

    [Header("Battle UI")]
    [SerializeField] private TextMeshProUGUI battleMessageText;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button blurButton;
    [SerializeField] private Button ultimateButton; // Optional future skill button. Current version does not require it.
    [SerializeField] private GameObject commandPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Damage Popup Points")]
    [SerializeField] private RectTransform enemyDamagePopupPoint;
    [SerializeField] private RectTransform playerDamagePopupPoint;

    private int playerHP;
    private int playerSP;
    private int enemyHP;
    private bool isDefending;
    private bool battleEnded;
    private bool playerTurn;

    private void Start()
    {
        BindButtonEvents();
        ConfigureButtons();
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
        ShowDamagePopup(popupText, enemyDamagePopupPoint);
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
        ShowDamagePopup(popupText, playerDamagePopupPoint);

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

    private void BindButtonEvents()
    {
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(OnAttackButton);
        }

        if (defendButton != null)
        {
            defendButton.onClick.RemoveAllListeners();
            defendButton.onClick.AddListener(OnDefendButton);
        }

        if (blurButton != null)
        {
            blurButton.onClick.RemoveAllListeners();
            blurButton.onClick.AddListener(OnBlurButton);
        }
    }

    private void ConfigureButtons()
    {
        ConfigureButtonVisuals(attackButton);
        ConfigureButtonVisuals(defendButton);
        ConfigureButtonVisuals(blurButton);
        ConfigureButtonVisuals(ultimateButton);
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

    private void ClearSelectedButton()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void ShowDamagePopup(string text, RectTransform popupPoint)
    {
        if (popupPoint == null)
            return;

        DamagePopup popup = DamagePopup.CreateAt(popupPoint, text);
        popup.Play();
    }
}
