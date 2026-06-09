using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Minimal turn-based battle controller for CombatScene.
// All main UI objects should already exist in the scene and be assigned in the Inspector.
public class BattleManager : MonoBehaviour
{
    #region Nested Types

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

    #endregion

    #region Serialized Fields

    [Header("Player Stats")]
    [SerializeField] private int maxPlayerHP = 100;
    [SerializeField] private int maxPlayerSP = 50;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int attackSPRecover = 5;
    [SerializeField] private int ultimateDamage = 80;
    [SerializeField] private int ultimateSPCost = 40;

    [Header("Enemy Stats")]
    [SerializeField] private int maxEnemyHP = 80;
    [SerializeField] private int enemyAttackDamage = 8;
    [SerializeField] private float enemyTurnDelay = 0.8f;
    [SerializeField] private float playerDamageMessageDelay = 1.0f;
    [SerializeField] private float fillSmoothTime = 0.2f;
    [SerializeField] private EnemyDayData[] enemiesByDay =
    {
        new EnemyDayData(1, "Day1兜底 / 泥巴怪", 30, 3, 0),
        new EnemyDayData(2, "实习生鼠鼠 / 焦虑的泥巴", 70, 7, 15),
        new EnemyDayData(3, "主管鼠鼠 / 坚硬的外壳", 85, 8, 18),
        new EnemyDayData(4, "清洁工鼠鼠 / 模糊的自我", 110, 10, 22),
        new EnemyDayData(5, "外卖员鼠鼠 / 厌倦的狂风", 130, 14, 26),
        new EnemyDayData(6, "大学生鼠鼠 / 迷茫的泡影", 160, 17, 32),
        new EnemyDayData(7, "临近崩溃的主管 / 崩溃的外壳", 240, 20, 0)
    };
    [SerializeField] private int defeatGoldRewardDivisor = 2;

    [Header("Failsafe")]
    [SerializeField] private int bathGodTurnLimit = 8;

    [Header("Bath God Effect")]
    [SerializeField] private GameObject bathGodEffect;
    [SerializeField] private Image bathGodImage;
    [SerializeField] private Sprite[] bathGodFrames;
    [SerializeField] private float bathGodFrameRate = 8f;
    [SerializeField] private Animator bathGodAnimator;
    [SerializeField] private float bathGodEffectDuration = 3f;
    [SerializeField] private string bathGodAnimationStateName = "";

    [Header("Player UI")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image spFillImage;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI playerSPText;
    [SerializeField] private TextMeshProUGUI playerBattleMessageText;
    [SerializeField] private TextMeshProUGUI battleLogText;
    [SerializeField] private int maxBattleLogLines = 5;
    [SerializeField] private float battleLogLineDelay = 0.75f;
    [SerializeField] private RectTransform playerDamagePopupPoint;

    [Header("Enemy UI")]
    [SerializeField] private Image enemyImage;
    [SerializeField] private EnemyAnimationPlayer enemyAnimationPlayer;
    [SerializeField] private Image enemyHPFillImage;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI enemyBattleMessageText;
    [SerializeField] private RectTransform enemyDamagePopupPoint;

    [Header("Enemy Sprites")]
    [SerializeField] private Sprite day1FallbackEnemySprite;
    [SerializeField] private Sprite day2EnemySprite;
    [SerializeField] private Sprite day3EnemySprite;
    [SerializeField] private Sprite day4EnemySprite;
    [SerializeField] private Sprite day5EnemySprite;
    [SerializeField] private Sprite day6EnemySprite;
    [FormerlySerializedAs("day7Phase1Sprite")]
    [SerializeField] private Sprite day7BossSprite;
    [FormerlySerializedAs("day7Phase2Sprite")]
    [SerializeField] private Sprite day7BossWeakenedSprite;
    [SerializeField] private Sprite[] dlcEnemySprites = new Sprite[3];

    [Header("Enemy Idle Animation Frames")]
    [SerializeField] private Sprite[] day2EnemyIdleFrames;
    [SerializeField] private Sprite[] day3EnemyIdleFrames;
    [SerializeField] private Sprite[] day4EnemyIdleFrames;
    [SerializeField] private Sprite[] day5EnemyIdleFrames;
    [SerializeField] private Sprite[] day6EnemyIdleFrames;
    [FormerlySerializedAs("day7Phase1IdleFrames")]
    [SerializeField] private Sprite[] day7BossIdleFrames;
    [FormerlySerializedAs("day7Phase2IdleFrames")]
    [SerializeField] private Sprite[] day7BossWeakenedIdleFrames;

    [Header("Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button blurButton;
    [SerializeField] private Button polishButton;
    [SerializeField] private Button ultimateButton;

    [Header("Item Buttons")]
    public Button soapButton;
    public Button teaButton;

    [Header("Victory")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Day7 Interlude")]
    [SerializeField] private GameObject day7InterludePanel;
    [SerializeField] private TextMeshProUGUI day7InterludeText;
    [SerializeField] private RectTransform day7InterludeTextRect;
    [SerializeField] private Button day7InterludeContinueButton;
    [SerializeField] private float day7InterludeScrollDuration = 10f;
    [SerializeField] private float day7InterludeStartY = -500f;
    [SerializeField] private float day7InterludeEndY = 500f;
    [SerializeField] private float day7WeakenedPlayerDamageMultiplier = 1.3f;
    [SerializeField] private float day7WeakenedEnemyAttackMultiplier = 0.8f;

    [Header("Damage Popup")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private Vector2 playerDamagePopupOffset = new Vector2(0f, -18f);
    [SerializeField] private Vector2 enemyDamagePopupOffset = new Vector2(0f, -18f);

    [Header("Hit Feedback")]
    public HitFeedback enemyHitFeedback;
    public HitFeedback playerHitFeedback;

    #endregion

    #region Runtime State

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
    private bool isDay7Phase2;
    private bool isChangingDay7Phase;
    private bool isDay7InterludePlaying;
    private bool day7InterludeTriggered;
    private bool day7BossWeakened;
    private bool enemyStunned;
    private bool warnedMissingItemGameManager;
    private bool warnedMissingSoapButtonLabel;
    private bool warnedMissingTeaButtonLabel;
    private bool warnedMissingEnemyHitFeedback;
    private bool warnedMissingPlayerHitFeedback;
    private int currentRound = 1;
    private readonly List<string> battleLogLines = new List<string>();
    private readonly Queue<string> battleLogQueue = new Queue<string>();
    private Coroutine battleLogRoutine;
    private Coroutine playerHPFillRoutine;
    private Coroutine playerSPFillRoutine;
    private Coroutine enemyHPFillRoutine;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        LogInspectorReferences();
        ResolveButtonReferences();
        ResolveEnemyAnimationPlayer();
        BindButtonEvents();
        ConfigureButtons();
        StartBattle();
    }

    private void Update()
    {
        if (isDay7InterludePlaying)
            return;

        if (Input.GetKeyDown(KeyCode.H))
            UseSoap();

        if (Input.GetKeyDown(KeyCode.J))
            UseTea();
    }

    #endregion

    #region Battle Setup

    // Resets battle state and initializes every assigned UI field.
    private void StartBattle()
    {
        ApplyDemoCombatTuning();

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
        isDay7Phase2 = false;
        isChangingDay7Phase = false;
        isDay7InterludePlaying = false;
        day7InterludeTriggered = false;
        day7BossWeakened = false;
        enemyStunned = false;
        currentRound = 1;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (day7InterludePanel != null)
            day7InterludePanel.SetActive(false);

        if (bathGodEffect != null)
            bathGodEffect.SetActive(false);

        ClearBattleLog();
        ApplyEnemyVisualForCurrentDay();
        RefreshActionButtonsForCurrentState("battle start");
        SetPlayerMessage(BuildBattleStartMessage());
        SetEnemyMessage("选择行动。");
        RefreshAllUI();
        LogBattleState("Battle started");
    }

    private string BuildBattleStartMessage()
    {
        return "Day " + currentDay + " 战斗开始！\n" +
               currentEnemyName + " 出现了！";
    }

    private void ApplyDemoCombatTuning()
    {
        maxPlayerHP = 100;
        maxPlayerSP = 50;
        attackDamage = 10;
        attackSPRecover = 5;
        ultimateDamage = 80;
        ultimateSPCost = 40;
        day7WeakenedPlayerDamageMultiplier = 1.3f;
        day7WeakenedEnemyAttackMultiplier = 0.8f;
    }

    #endregion

    #region Player Actions

    // Attack is the basic no-cost player action.
    public void OnAttackButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();

        if (!IsAttackUnlocked())
        {
            SetPlayerMessage("普通搓澡需要 Day2 开始才能使用。");
            RefreshActionButtonsForCurrentState("attack locked by day");
            return;
        }

        int damage = GetPlayerDamageAfterModifiers(attackDamage);

        BeginPlayerAction("普通搓澡");
        currentPlayerSP = Mathf.Min(maxPlayerSP, currentPlayerSP + attackSPRecover);
        RefreshPlayerUI();
        SetPlayerMessage("普通搓澡！造成 " + damage + " 点伤害，回复 " + attackSPRecover + " SP。");
        DealDamageToEnemy(damage, "小福进行了普通搓澡，敌人受到了 " + damage + " 点伤害！");
    }

    #endregion

    #region Skill Actions

    // Polish is the fixed combo skill. Not enough SP does not spend the player turn.
    public void OnPolishButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();

        if (!IsPolishUnlocked())
        {
            SetPlayerMessage("搓澡巾连击需要 Day2 开始才能使用。");
            RefreshActionButtonsForCurrentState("polish locked by day");
            return;
        }

        int spCost = 15;
        int damage = GetPlayerDamageAfterModifiers(25);

        if (currentPlayerSP < spCost)
        {
            SetPlayerMessage("SP 不足，无法使用搓澡巾连击。");
            RefreshActionButtonsForCurrentState("polish blocked by SP");
            return;
        }

        BeginPlayerAction("珍珠浴球大冲洗");
        currentPlayerSP = Mathf.Max(0, currentPlayerSP - spCost);
        SetPlayerMessage("珍珠浴球大冲洗发动！");
        RefreshPlayerUI();
        StartCoroutine(PolishRoutine(damage));
    }

    // Blur is bubble eye: damage, heal, and make the enemy skip one action.
    public void OnBlurButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();

        if (!IsBlurUnlocked())
        {
            SetPlayerMessage("泡泡迷人眼需要 Day4 开始才能使用。");
            RefreshActionButtonsForCurrentState("blur locked by day");
            return;
        }

        int spCost = 20;
        int damage = GetPlayerDamageAfterModifiers(15);
        int heal = 15;

        if (currentPlayerSP < spCost)
        {
            SetPlayerMessage("SP 不足，无法使用泡泡迷人眼。");
            RefreshActionButtonsForCurrentState("blur blocked by SP");
            return;
        }

        BeginPlayerAction("泡泡迷人眼");
        currentPlayerSP = Mathf.Max(0, currentPlayerSP - spCost);

        currentPlayerHP = Mathf.Min(maxPlayerHP, currentPlayerHP + heal);
        enemyStunned = true;

        SetPlayerMessage("泡泡迷人眼！造成 " + damage + " 点伤害，回复 " + heal + " HP，并让敌人跳过一次行动。");
        RefreshPlayerUI();
        DealDamageToEnemy(damage, "泡泡迷人眼命中，敌人受到了 " + damage + " 点伤害！");
    }

    // Reserved for the future. Current version does not spend the turn.
    public void OnUltimateButton()
    {
        if (!CanPlayerAct())
            return;

        ClearSelectedButton();

        if (!IsUltimateUnlockedForToday())
        {
            SetPlayerMessage("灵魂抛光需要 Day5 及以后才能使用。");
            RefreshActionButtonsForCurrentState("ultimate locked by day");
            return;
        }

        if (currentPlayerSP < ultimateSPCost)
        {
            SetPlayerMessage("SP不足，无法使用灵魂抛光。");
            RefreshActionButtonsForCurrentState("ultimate blocked by SP");
            return;
        }

        int damage = GetPlayerDamageAfterModifiers(ultimateDamage);

        BeginPlayerAction("灵魂抛光");
        currentPlayerSP = Mathf.Max(0, currentPlayerSP - ultimateSPCost);
        SetPlayerMessage("灵魂抛光！造成 " + damage + " 点伤害。");
        RefreshPlayerUI();
        DealDamageToEnemy(damage, "灵魂抛光发动，敌人受到了 " + damage + " 点伤害！");
    }

    #endregion

    #region Victory and Scene Flow

    // Optional hook for a Continue button inside VictoryPanel.
    public void OnVictoryContinueButton()
    {
        SceneManager.LoadScene("AfterCombatScene");
    }

    // Trailer helper only: lets TrailerModeController show the existing victory flow without changing normal battle logic.
    public void TrailerForceWinBattle()
    {
        WinBattle();
    }

    #endregion

    #region Damage and HP

    private void BeginPlayerAction(string actionName)
    {
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        LogBattleState("Player action: " + actionName);
    }

    private IEnumerator PolishRoutine(int totalDamage)
    {
        int hitCount = Random.Range(2, 4);
        int baseHitDamage = totalDamage / hitCount;
        int remainder = totalDamage % hitCount;

        for (int i = 0; i < hitCount; i++)
        {
            if (currentEnemyHP <= 0)
                break;

            int hitDamage = baseHitDamage + (i == hitCount - 1 ? remainder : 0);
            bool shouldTriggerDay7Interlude = ApplyPolishHit(hitDamage, i + 1, hitCount);

            if (shouldTriggerDay7Interlude)
            {
                yield return new WaitForSeconds(0.2f);
                yield return Day7HalfHpInterludeRoutine();
                yield break;
            }

            if (currentEnemyHP <= 0)
                break;

            if (i < hitCount - 1)
                yield return new WaitForSeconds(0.2f);
        }

        if (currentEnemyHP <= 0)
        {
            WinBattle();
            yield break;
        }

        if (HasReachedBathGodTurnLimit())
        {
            TriggerBathGodIntervention("Turn limit reached.");
            yield break;
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    private bool ApplyPolishHit(int hitDamage, int hitIndex, int hitCount)
    {
        int previousEnemyHP = currentEnemyHP;
        int nextEnemyHP = Mathf.Max(0, currentEnemyHP - hitDamage);
        bool shouldTriggerDay7Interlude = ShouldTriggerDay7HalfHpInterlude(nextEnemyHP);

        if (shouldTriggerDay7Interlude && nextEnemyHP <= 0)
            nextEnemyHP = 1;

        currentEnemyHP = nextEnemyHP;
        int appliedDamage = Mathf.Max(0, previousEnemyHP - currentEnemyHP);

        RefreshEnemyUI();
        SetEnemyMessage("连击 " + hitIndex + " / " + hitCount + "：敌人受到了 " + appliedDamage + " 点伤害！");
        SpawnDamagePopup(enemyDamagePopupPoint, "-" + appliedDamage, enemyDamagePopupOffset);
        PlayEnemyHitFeedback();
        LogBattleState("Polish hit " + hitIndex + "/" + hitCount);

        return shouldTriggerDay7Interlude;
    }

    private void DealDamageToEnemy(int damage, string message)
    {
        int nextEnemyHP = Mathf.Max(0, currentEnemyHP - damage);
        bool shouldTriggerDay7Interlude = ShouldTriggerDay7HalfHpInterlude(nextEnemyHP);

        if (shouldTriggerDay7Interlude && nextEnemyHP <= 0)
            nextEnemyHP = 1;

        currentEnemyHP = nextEnemyHP;
        RefreshEnemyUI();
        SetEnemyMessage(message);
        SpawnDamagePopup(enemyDamagePopupPoint, "-" + damage, enemyDamagePopupOffset);
        PlayEnemyHitFeedback();
        LogBattleState("Enemy damaged");

        if (shouldTriggerDay7Interlude)
        {
            StartCoroutine(Day7HalfHpInterludeRoutine());
            return;
        }

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

    #endregion

    #region Enemy Turn

    private IEnumerator EnemyTurnRoutine()
    {
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        LogBattleState("Enemy turn started");

        yield return new WaitForSeconds(enemyTurnDelay);

        if (enemyStunned)
        {
            enemyStunned = false;
            SetEnemyMessage(currentEnemyName + "被泡泡迷住了，无法行动！");
            yield return new WaitForSeconds(playerDamageMessageDelay);

            currentRound++;
            isPlayerTurn = true;
            RefreshActionButtonsForCurrentState("enemy stunned");
            SetPlayerMessage("敌人露出破绽，继续行动！");
            LogBattleState("Enemy turn skipped by blur");
            yield break;
        }

        currentPlayerHP = Mathf.Max(0, currentPlayerHP - enemyAttackDamage);
        RefreshPlayerUI();
        SetPlayerMessage(currentEnemyName + "反击！你受到 " + enemyAttackDamage + " 点伤害。");
        SpawnDamagePopup(playerDamagePopupPoint, "-" + enemyAttackDamage, playerDamagePopupOffset);
        PlayPlayerHitFeedback();
        LogBattleState("Enemy attacked");

        if (currentPlayerHP <= 0)
        {
            TriggerBathGodIntervention("Player HP reached zero.");
            yield break;
        }

        yield return new WaitForSeconds(playerDamageMessageDelay);

        currentRound++;
        isPlayerTurn = true;
        RefreshActionButtonsForCurrentState("player turn restored");
        LogBattleState("Player turn started");
    }

    #endregion

    #region Victory and Scene Flow

    private void WinBattle()
    {
        if (battleEnded)
            return;

        battleEnded = true;
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        SavePlayerState();
        int gainedGold = GiveGoldReward(currentEnemyGoldReward);
        SetEnemyMessage(currentEnemyName + "被净化了。");
        if (currentDay >= 7 || gainedGold <= 0)
            SetPlayerMessage("最终战胜利！点击 Continue 进入战后剧情。");
        else
            SetPlayerMessage("净化成功！获得 " + gainedGold + " 金币。点击 Continue 进入战后剧情。");
        LogBattleState("Battle won");

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    #endregion

    #region Day7 Boss

    private void TriggerBathGodIntervention(string reason)
    {
        if (battleEnded || isChangingDay7Phase || bathGodIntervened)
            return;

        StartCoroutine(BathGodInterventionRoutine(reason));
    }

    private IEnumerator BathGodInterventionRoutine(string reason)
    {
        battleEnded = true;
        isPlayerTurn = false;
        bathGodIntervened = true;
        SetActionButtonsInteractable(false);

        if (ShouldEnterDay7Phase2())
        {
            SetPlayerMessage("搓澡之神降临！先把这层崩溃的外壳洗掉！");
            SetEnemyMessage(reason);
            SpawnDamagePopup(enemyDamagePopupPoint, "-999", enemyDamagePopupOffset);
            yield return StartCoroutine(PlayBathGodEffectRoutine());

            currentPlayerHP = Mathf.Max(1, currentPlayerHP);
            currentEnemyHP = 0;
            SavePlayerState();
            RefreshAllUI();

            battleEnded = false;
            yield return SwitchToDay7Phase2Routine();
            yield break;
        }

        if (currentDay >= 7)
        {
            SetPlayerMessage("你已经很努力了……但你不是一个人在战斗。");
            SetEnemyMessage(reason);
            yield return StartCoroutine(PlayBathGodEffectRoutine());
            SetPlayerMessage("全员意志支撑着你，最终 Boss 露出了破绽。");

            currentPlayerHP = Mathf.Min(maxPlayerHP, Mathf.Max(1, currentPlayerHP) + Mathf.CeilToInt(maxPlayerHP * 0.5f));
            currentPlayerSP = Mathf.Min(maxPlayerSP, currentPlayerSP + Mathf.CeilToInt(maxPlayerSP * 0.5f));

            if (!day7InterludeTriggered)
                yield return Day7InterludeRoutine();

            ApplyDay7BossWeakening();
            battleEnded = false;
            isPlayerTurn = true;
            SavePlayerState();
            RefreshAllUI();
            SetEnemyMessage(currentEnemyName + "进入虚弱状态。");
            RefreshActionButtonsForCurrentState("day7 bath god support");
            LogBattleState("Day7 bath god support: " + reason);
            yield break;
        }
        else
        {
            SetPlayerMessage("搓澡之神降临！今天也不能卡在这里！");
            SetEnemyMessage(reason);
            SpawnDamagePopup(enemyDamagePopupPoint, "-999", enemyDamagePopupOffset);
            yield return StartCoroutine(PlayBathGodEffectRoutine());
        }

        currentPlayerHP = Mathf.Max(1, currentPlayerHP);
        currentEnemyHP = 0;
        SavePlayerState();
        RefreshAllUI();

        int reducedReward = defeatGoldRewardDivisor > 0
            ? currentEnemyGoldReward / defeatGoldRewardDivisor
            : 0;

        int gainedGold = GiveGoldReward(reducedReward);
        SetEnemyMessage(currentEnemyName + "被保底净化了。");
        if (currentDay >= 7 || gainedGold <= 0)
            SetPlayerMessage("最终战胜利！点击 Continue 进入战后剧情。");
        else
            SetPlayerMessage("搓澡之神帮你完成了净化，但今天状态很差，金币奖励减少。获得 " + gainedGold + " 金币。");
        LogBattleState("Bath god intervention: " + reason);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    private IEnumerator PlayBathGodEffectRoutine()
    {
        if (bathGodEffect != null)
            bathGodEffect.SetActive(true);

        if (bathGodImage == null && bathGodEffect != null)
            bathGodImage = bathGodEffect.GetComponent<Image>();

        float elapsed = 0f;
        float duration = Mathf.Max(0f, bathGodEffectDuration);
        bool canPlayFrames = bathGodImage != null && bathGodFrames != null && bathGodFrames.Length > 0;

        if (canPlayFrames)
        {
            float frameDelay = 1f / Mathf.Max(0.01f, bathGodFrameRate);

            for (int i = 0; i < bathGodFrames.Length; i++)
            {
                if (bathGodFrames[i] != null)
                    bathGodImage.sprite = bathGodFrames[i];

                yield return new WaitForSeconds(frameDelay);
                elapsed += frameDelay;
            }

            float remainingDuration = duration - elapsed;
            if (remainingDuration > 0f)
                yield return new WaitForSeconds(remainingDuration);
        }
        else
        {
            if (bathGodAnimator != null && !string.IsNullOrEmpty(bathGodAnimationStateName))
                bathGodAnimator.Play(bathGodAnimationStateName, 0, 0f);

            if (duration > 0f)
                yield return new WaitForSeconds(duration);
        }

        if (bathGodEffect != null)
            bathGodEffect.SetActive(false);
    }

    private bool CanPlayerAct()
    {
        return !battleEnded && isPlayerTurn;
    }

    private int GetPlayerDamageAfterModifiers(int baseDamage)
    {
        if (currentDay == 7 && day7BossWeakened)
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * Mathf.Max(1f, day7WeakenedPlayerDamageMultiplier)));

        return baseDamage;
    }

    private bool ShouldTriggerDay7HalfHpInterlude(int nextEnemyHP)
    {
        if (currentDay != 7 || day7InterludeTriggered || day7BossWeakened)
            return false;

        int halfHpThreshold = Mathf.CeilToInt(maxEnemyHP * 0.5f);
        return nextEnemyHP <= halfHpThreshold;
    }

    private IEnumerator Day7HalfHpInterludeRoutine()
    {
        if (isChangingDay7Phase)
        {
            RestorePlayerTurnAfterDay7Transition("day7 half hp interlude skipped because phase is already changing");
            yield break;
        }

        day7InterludeTriggered = true;
        isChangingDay7Phase = true;
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);
        SetEnemyMessage(currentEnemyName + "的气势开始动摇。");

        yield return Day7InterludeRoutine();

        ApplyDay7BossWeakening();
        PlayEnemyIdleAnimation(GetEnemyIdleFramesForCurrentDay(), GetEnemySpriteForCurrentDay());
        SetPlayerMessage("大家的意志支撑着你。继续战斗！");
        SetEnemyMessage(currentEnemyName + "进入虚弱状态。");
        RestorePlayerTurnAfterDay7Transition("day7 half hp interlude complete");
        LogBattleState("Day7 half HP interlude finished");
    }

    private void RestorePlayerTurnAfterDay7Transition(string reason)
    {
        if (battleEnded || currentEnemyHP <= 0)
        {
            RefreshActionButtonsForCurrentState(reason + " skipped because battle ended");
            return;
        }

        battleEnded = false;
        isChangingDay7Phase = false;
        isDay7InterludePlaying = false;
        isPlayerTurn = true;
        RefreshAllUI();
        RefreshActionButtonsForCurrentState(reason);
    }

    private void ApplyDay7BossWeakening()
    {
        day7InterludeTriggered = true;

        if (day7BossWeakened)
            return;

        day7BossWeakened = true;
        enemyAttackDamage = Mathf.Max(1, Mathf.RoundToInt(enemyAttackDamage * Mathf.Max(0.01f, day7WeakenedEnemyAttackMultiplier)));
    }

    #endregion

    #region Skill Unlocks

    private bool HasReachedBathGodTurnLimit()
    {
        return bathGodTurnLimit > 0 && currentRound >= bathGodTurnLimit;
    }

    private bool IsUltimateAvailable()
    {
        return IsUltimateUnlockedForToday() && currentPlayerSP >= ultimateSPCost;
    }

    private bool IsAttackUnlocked()
    {
        if (IsDiabetesDLCMode())
            return true;

        return currentDay >= 2;
    }

    private bool IsUltimateUnlockedForToday()
    {
        if (ShouldUnlockAllDLCSkills())
            return true;

        if (IsDiabetesDLCMode())
            return currentDay >= 3;

        return currentDay >= 5;
    }

    private bool IsPolishUnlocked()
    {
        if (ShouldUnlockAllDLCSkills())
            return true;

        if (IsDiabetesDLCMode())
            return currentDay >= 2;

        return currentDay >= 2;
    }

    private bool IsBlurUnlocked()
    {
        if (ShouldUnlockAllDLCSkills())
            return true;

        if (IsDiabetesDLCMode())
            return currentDay >= 3;

        return currentDay >= 4;
    }

    private bool IsDiabetesDLCMode()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        return gameManager != null && gameManager.currentGameMode == GameMode.DiabetesDLC;
    }

    private bool ShouldUnlockAllDLCSkills()
    {
        return IsDiabetesDLCMode() && gameManager.dlcUnlockAllSkills;
    }

    #endregion

    #region Items

    public void UseSoap()
    {
        if (!CanPlayerAct())
            return;

        if (gameManager == null)
            gameManager = GameManager.EnsureInstance();

        if (currentPlayerHP >= maxPlayerHP)
        {
            SetPlayerMessage("HP 已满，不需要使用肥皂。");
            RefreshItemButtonsForCurrentState("soap blocked by full HP");
            return;
        }

        if (gameManager.soapCount <= 0)
        {
            SetPlayerMessage("没有肥皂了。");
            RefreshItemButtonsForCurrentState("soap blocked by count");
            return;
        }

        gameManager.soapCount--;
        currentPlayerHP = Mathf.Min(maxPlayerHP, currentPlayerHP + 30);
        SavePlayerState();
        RefreshPlayerUI();
        RefreshItemButtonsForCurrentState("soap used");
        SetPlayerMessage("使用肥皂，恢复 30 HP。剩余肥皂：" + gameManager.soapCount);
    }

    public void UseTea()
    {
        if (!CanPlayerAct())
            return;

        if (gameManager == null)
            gameManager = GameManager.EnsureInstance();

        if (currentPlayerSP >= maxPlayerSP)
        {
            SetPlayerMessage("SP 已满，不需要使用花茶。");
            RefreshItemButtonsForCurrentState("tea blocked by full SP");
            return;
        }

        if (gameManager.teaCount <= 0)
        {
            SetPlayerMessage("没有花茶了。");
            RefreshItemButtonsForCurrentState("tea blocked by count");
            return;
        }

        gameManager.teaCount--;
        currentPlayerSP = Mathf.Min(maxPlayerSP, currentPlayerSP + 20);
        SavePlayerState();
        RefreshPlayerUI();
        RefreshItemButtonsForCurrentState("tea used");
        SetPlayerMessage("饮用花茶，恢复 20 SP。剩余花茶：" + gameManager.teaCount);
    }

    private void SavePlayerState()
    {
        if (gameManager == null)
            gameManager = GameManager.EnsureInstance();

        gameManager.playerHP = currentPlayerHP;
        gameManager.playerSP = currentPlayerSP;
    }

    private int GiveGoldReward(int amount)
    {
        if (gameManager == null)
            gameManager = GameManager.EnsureInstance();

        if (amount <= 0)
        {
            return 0;
        }

        gameManager.playerGold += amount;
        return amount;
    }

    #endregion

    #region DLC Battle

    private EnemyDayData GetEnemyForDay(int day)
    {
        if (gameManager != null && gameManager.currentGameMode == GameMode.DiabetesDLC)
            return GetDiabetesDLCEnemyForDay(day);

        // Use code-defined data so old serialized Inspector values cannot block the demo loop.
        return GetDefaultEnemyForDay(day);
    }

    private EnemyDayData GetDiabetesDLCEnemyForDay(int day)
    {
        switch (day)
        {
            case 1:
                return new EnemyDayData(1, "纸团", 70, 6, 15);
            case 2:
                return new EnemyDayData(2, "疑问的雾", 100, 8, 18);
            case 3:
                return new EnemyDayData(3, "杂乱的线结", 170, 12, 22);
        }

        Debug.LogWarning("No Diabetes DLC enemy configured for day " + day + ". Using safe DLC fallback enemy instead of main story data.");
        return new EnemyDayData(day, "纸团", 70, 7, 15);
    }

    private EnemyDayData GetDefaultEnemyForDay(int day)
    {
        switch (day)
        {
            case 1:
                return new EnemyDayData(1, "Day1兜底 / 泥巴怪", 30, 3, 0);
            case 2:
                return new EnemyDayData(2, "实习生鼠鼠 / 焦虑的泥巴", 70, 7, 15);
            case 3:
                return new EnemyDayData(3, "主管鼠鼠 / 坚硬的外壳", 85, 8, 18);
            case 4:
                return new EnemyDayData(4, "清洁工鼠鼠 / 模糊的自我", 110, 10, 22);
            case 5:
                return new EnemyDayData(5, "外卖员鼠鼠 / 厌倦的狂风", 130, 14, 26);
            case 6:
                return new EnemyDayData(6, "大学生鼠鼠 / 迷茫的泡影", 160, 17, 32);
            case 7:
                return new EnemyDayData(7, "临近崩溃的主管 / 崩溃的外壳", 240, 20, 0);
            default:
                return new EnemyDayData(day, "Day1兜底 / 泥巴怪", 30, 3, 0);
        }
    }

    private EnemyDayData GetDay7Phase2Enemy()
    {
        return new EnemyDayData(7, "不安愤怒沮丧焦虑失望悲伤", 260, 18, 0);
    }

    private bool ShouldEnterDay7Phase2()
    {
        return false;
    }

    #endregion

    #region Day7 Boss

    private IEnumerator SwitchToDay7Phase2Routine()
    {
        if (isChangingDay7Phase)
        {
            RestorePlayerTurnAfterDay7Transition("day7 phase2 skipped because phase is already changing");
            yield break;
        }

        isChangingDay7Phase = true;
        isPlayerTurn = false;
        SetActionButtonsInteractable(false);

        yield return Day7InterludeRoutine();

        EnemyDayData phase2Enemy = GetDay7Phase2Enemy();
        currentEnemyName = phase2Enemy.enemyName;
        maxEnemyHP = phase2Enemy.maxHP;
        enemyAttackDamage = phase2Enemy.attackDamage;
        currentEnemyGoldReward = phase2Enemy.goldReward;
        currentEnemyHP = maxEnemyHP;
        currentRound = 1;
        isDay7Phase2 = true;

        PlayEnemyIdleAnimation(day7BossWeakenedIdleFrames, day7BossWeakenedSprite);
        SetEnemyMessage("Day 7 Phase 2: " + currentEnemyName);
        RestorePlayerTurnAfterDay7Transition("day7 phase2 complete");
        LogBattleState("Day7 phase2 started");
    }

    private IEnumerator Day7InterludeRoutine()
    {
        isDay7InterludePlaying = true;
        SetActionButtonsInteractable(false);

        string interludeText =
            "小福，你这几天真的很努力了。\n" +
            "你洗净了很多人的疲惫。\n\n" +
            "这座城市还没有完全变好。\n" +
            "但它已经因为你，多了一点喘息的地方。\n\n" +
            "可是……\n" +
            "那些不安、愤怒、沮丧、焦虑、失望、悲伤……\n" +
            "还没有消失。\n\n" +
            "有什么东西出现了。";

        if (day7InterludePanel != null)
            day7InterludePanel.SetActive(true);
        else
            Debug.LogWarning("day7InterludePanel is not assigned. Showing Day7 interlude in BattleMessageText instead.");

        if (day7InterludeContinueButton != null)
            day7InterludeContinueButton.gameObject.SetActive(false);

        if (day7InterludeTextRect == null && day7InterludeText != null)
            day7InterludeTextRect = day7InterludeText.rectTransform;

        if (day7InterludeText != null)
            day7InterludeText.text = interludeText;
        else
            Debug.LogWarning("day7InterludeText is not assigned. Showing Day7 interlude in BattleMessageText instead.");

        if (day7InterludeTextRect != null)
        {
            Vector2 startPosition = day7InterludeTextRect.anchoredPosition;
            startPosition.y = day7InterludeStartY;
            day7InterludeTextRect.anchoredPosition = startPosition;

            float duration = Mathf.Max(0.1f, day7InterludeScrollDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector2 position = day7InterludeTextRect.anchoredPosition;
                position.y = Mathf.Lerp(day7InterludeStartY, day7InterludeEndY, t);
                day7InterludeTextRect.anchoredPosition = position;
                yield return null;
            }

            Vector2 endPosition = day7InterludeTextRect.anchoredPosition;
            endPosition.y = day7InterludeEndY;
            day7InterludeTextRect.anchoredPosition = endPosition;
        }
        else
        {
            SetPlayerMessage(interludeText);
            yield return new WaitForSeconds(Mathf.Max(1f, day7InterludeScrollDuration));
        }

        yield return new WaitForSeconds(0.5f);

        if (day7InterludeContinueButton != null)
            day7InterludeContinueButton.gameObject.SetActive(false);

        if (day7InterludePanel != null)
            day7InterludePanel.SetActive(false);

        isDay7InterludePlaying = false;
    }

    #endregion

    #region UI Refresh

    private void RefreshAllUI()
    {
        RefreshPlayerUI();
        RefreshEnemyUI();
    }

    private Sprite GetEnemySpriteForCurrentDay()
    {
        switch (currentDay)
        {
            case 1:
                return day1FallbackEnemySprite;
            case 2:
                return day2EnemySprite;
            case 3:
                return day3EnemySprite;
            case 4:
                return day4EnemySprite;
            case 5:
                return day5EnemySprite;
            case 6:
                return day6EnemySprite;
            case 7:
                return day7BossWeakened && day7BossWeakenedSprite != null ? day7BossWeakenedSprite : day7BossSprite;
            default:
                return day1FallbackEnemySprite;
        }
    }

    private Sprite[] GetEnemyIdleFramesForCurrentDay()
    {
        switch (currentDay)
        {
            case 2:
                return day2EnemyIdleFrames;
            case 3:
                return day3EnemyIdleFrames;
            case 4:
                return day4EnemyIdleFrames;
            case 5:
                return day5EnemyIdleFrames;
            case 6:
                return day6EnemyIdleFrames;
            case 7:
                return day7BossWeakened && day7BossWeakenedIdleFrames != null && day7BossWeakenedIdleFrames.Length > 0
                    ? day7BossWeakenedIdleFrames
                    : day7BossIdleFrames;
            default:
                return null;
        }
    }

    private void ApplyEnemyVisualForCurrentDay()
    {
        if (TryApplyDlcEnemyVisual())
            return;

        PlayEnemyIdleAnimation(GetEnemyIdleFramesForCurrentDay(), GetEnemySpriteForCurrentDay());
    }

    private bool TryApplyDlcEnemyVisual()
    {
        if (!IsDiabetesDLCMode())
            return false;

        int index = currentDay - 1;
        if (index < 0 || index >= 3)
        {
            Debug.LogWarning("DLC enemy sprite day " + currentDay + " is outside supported range Day1-Day3. Using default enemy visual.");
            return false;
        }

        if (dlcEnemySprites == null || index >= dlcEnemySprites.Length || dlcEnemySprites[index] == null)
        {
            Debug.LogWarning("DLC enemy sprite is not assigned for day " + currentDay + ". Using default enemy visual.");
            return false;
        }

        Sprite dlcSprite = dlcEnemySprites[index];
        if (enemyImage != null)
            enemyImage.enabled = true;

        if (enemyAnimationPlayer != null)
        {
            if (enemyAnimationPlayer.targetImage == null && enemyImage != null)
                enemyAnimationPlayer.targetImage = enemyImage;

            enemyAnimationPlayer.StopAndShow(dlcSprite);
        }
        else
        {
            ApplyEnemySprite(dlcSprite);
        }

        return true;
    }

    private void PlayEnemyIdleAnimation(Sprite[] idleFrames, Sprite fallbackSprite)
    {
        if (enemyAnimationPlayer == null)
        {
            Debug.LogWarning("enemyAnimationPlayer is not assigned. Falling back to static enemy sprite.");
            ApplyEnemySprite(fallbackSprite);
            return;
        }

        if (enemyAnimationPlayer.targetImage == null && enemyImage != null)
            enemyAnimationPlayer.targetImage = enemyImage;

        enemyAnimationPlayer.Play(idleFrames, fallbackSprite);
    }

    private void ApplyEnemySprite(Sprite sprite)
    {
        if (enemyImage == null)
        {
            Debug.LogWarning("enemyImage is not assigned. Drag the CombatScene enemy Image into BattleManager.enemyImage.");
            return;
        }

        if (sprite == null)
        {
            Debug.LogWarning("Enemy sprite is not assigned for currentDay=" + currentDay + ", isDay7Phase2=" + isDay7Phase2 + ". Keeping current enemy image.");
            return;
        }

        enemyImage.sprite = sprite;
    }

    private void RefreshPlayerUI()
    {
        playerHPFillRoutine = SetFillAmount(hpFillImage, currentPlayerHP, maxPlayerHP, playerHPFillRoutine, "hpFillImage");
        playerSPFillRoutine = SetFillAmount(spFillImage, currentPlayerSP, maxPlayerSP, playerSPFillRoutine, "spFillImage");

        if (playerHPText != null)
            playerHPText.text = "HP " + currentPlayerHP + " / " + maxPlayerHP;

        if (playerSPText != null)
            playerSPText.text = "SP " + currentPlayerSP + " / " + maxPlayerSP;
    }

    private void RefreshEnemyUI()
    {
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

    #endregion

    #region Messages and Feedback

    private void SetPlayerMessage(string message)
    {
        if (battleLogText != null)
        {
            EnqueueBattleLog(message);
            return;
        }

        if (playerBattleMessageText != null)
        {
            playerBattleMessageText.text = message;
        }
        else
        {
            Debug.LogWarning("playerBattleMessageText is not assigned.");
        }
    }

    private void SetEnemyMessage(string message)
    {
        if (battleLogText != null)
        {
            EnqueueBattleLog(message);
            return;
        }

        if (enemyBattleMessageText != null)
        {
            enemyBattleMessageText.text = message;
        }
        else
        {
            Debug.LogWarning("enemyBattleMessageText is not assigned.");
        }
    }

    private void EnqueueBattleLog(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        string[] lines = message.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (!string.IsNullOrEmpty(line))
                battleLogQueue.Enqueue(line);
        }

        if (battleLogRoutine == null)
            battleLogRoutine = StartCoroutine(PlayBattleLogQueue());
    }

    private IEnumerator PlayBattleLogQueue()
    {
        while (battleLogQueue.Count > 0)
        {
            AddBattleLog(battleLogQueue.Dequeue());
            yield return new WaitForSeconds(Mathf.Max(0f, battleLogLineDelay));
        }

        battleLogRoutine = null;
    }

    private void AddBattleLog(string message)
    {
        if (string.IsNullOrEmpty(message) || battleLogText == null)
            return;

        battleLogLines.Add(message);

        int safeMaxLines = Mathf.Max(1, maxBattleLogLines);
        while (battleLogLines.Count > safeMaxLines)
            battleLogLines.RemoveAt(0);

        RefreshBattleLogText();
    }

    private void ClearBattleLog()
    {
        battleLogLines.Clear();
        battleLogQueue.Clear();

        if (battleLogRoutine != null)
        {
            StopCoroutine(battleLogRoutine);
            battleLogRoutine = null;
        }

        if (battleLogText != null)
            battleLogText.text = "";
    }

    private void RefreshBattleLogText()
    {
        if (battleLogText != null)
            battleLogText.text = string.Join("\n", battleLogLines);
    }

    #endregion

    #region UI Binding and Buttons

    private void BindButtonEvents()
    {
        BindButton(attackButton, OnAttackButton);
        BindButton(blurButton, OnBlurButton);
        BindButton(polishButton, OnPolishButton);
        BindButton(ultimateButton, OnUltimateButton);
        BindButton(soapButton, UseSoap);
        BindButton(teaButton, UseTea);
    }

    private void ResolveButtonReferences()
    {
        if (attackButton == null)
            attackButton = FindButtonByName("Attack");

        if (blurButton == null)
            blurButton = FindButtonByName("Blur");

        if (polishButton == null)
            polishButton = FindButtonByNames("Polish", "PolishButton", "Polish Button", "PolishSkillButton", "Button_Polish");

        if (ultimateButton == null)
            ultimateButton = FindButtonByName("Ultimate");

        LogButtonState("after resolving button references");
    }

    private void ResolveEnemyAnimationPlayer()
    {
        if (enemyAnimationPlayer == null && enemyImage != null)
            enemyAnimationPlayer = enemyImage.GetComponent<EnemyAnimationPlayer>();

        if (enemyAnimationPlayer != null && enemyAnimationPlayer.targetImage == null)
            enemyAnimationPlayer.targetImage = enemyImage;

        if (enemyAnimationPlayer == null)
            Debug.LogWarning("enemyAnimationPlayer is not assigned. Enemy idle animation will fall back to static sprites.");
    }

    private Button FindButtonByNames(params string[] objectNames)
    {
        foreach (string objectName in objectNames)
        {
            GameObject buttonObject = GameObject.Find(objectName);
            if (buttonObject == null)
                continue;

            Button button = buttonObject.GetComponent<Button>();
            if (button != null)
                return button;

            Debug.LogWarning("BattleManager found " + objectName + " but it has no Button component.");
        }

        Debug.LogWarning("BattleManager could not find button by names: " + string.Join(", ", objectNames) + ".");
        return null;
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
        ConfigureButton(polishButton);
        ConfigureButton(ultimateButton);
        ConfigureButton(soapButton);
        ConfigureButton(teaButton);
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
        SetButtonInteractable(polishButton, interactable);
        SetButtonInteractable(ultimateButton, interactable);

        if (interactable)
            RefreshItemButtonsForCurrentState("set all buttons interactable = true");
        else
            SetItemButtonsInteractable(false);

        LogButtonState("set all buttons interactable = " + interactable);
    }

    private void RefreshActionButtonsForCurrentState(string reason)
    {
        bool canAct = !battleEnded && isPlayerTurn;

        SetButtonInteractable(attackButton, canAct && IsAttackUnlocked());
        SetButtonInteractable(blurButton, canAct && IsBlurUnlocked());
        SetButtonInteractable(polishButton, canAct && IsPolishUnlocked());
        SetButtonInteractable(ultimateButton, canAct && IsUltimateUnlockedForToday());
        RefreshItemButtonsForCurrentState(reason);
        LogButtonState(reason);
    }

    private void RefreshItemButtonsForCurrentState(string reason)
    {
        bool canAct = !battleEnded && isPlayerTurn;

        if (gameManager == null)
            gameManager = GameManager.Instance;

        int soapCount = gameManager != null ? gameManager.soapCount : 0;
        int teaCount = gameManager != null ? gameManager.teaCount : 0;

        SetButtonInteractable(soapButton, canAct && soapCount > 0 && currentPlayerHP < maxPlayerHP);
        SetButtonInteractable(teaButton, canAct && teaCount > 0 && currentPlayerSP < maxPlayerSP);
        RefreshItemButtonLabels();

    }

    private void RefreshItemButtonLabels()
    {
        SetItemButtonLabel(soapButton, "SoapButton", "肥皂 x" + GetSoapCount(), ref warnedMissingSoapButtonLabel);
        SetItemButtonLabel(teaButton, "TeaButton", "花茶 x" + GetTeaCount(), ref warnedMissingTeaButtonLabel);
    }

    private void SetItemButtonLabel(Button button, string buttonName, string labelText, ref bool warnedMissingLabel)
    {
        if (button == null)
        {
            if (!warnedMissingLabel)
            {
                warnedMissingLabel = true;
                Debug.LogWarning("BattleManager cannot update " + buttonName + " label because the button is not assigned.");
            }

            return;
        }

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null)
        {
            if (!warnedMissingLabel)
            {
                warnedMissingLabel = true;
                Debug.LogWarning("BattleManager could not find TextMeshProUGUI under " + buttonName + ".");
            }

            return;
        }

        label.text = labelText;
    }

    private int GetSoapCount()
    {
        GameManager manager = GetGameManagerForItemCounts();
        return manager != null ? manager.soapCount : 0;
    }

    private int GetTeaCount()
    {
        GameManager manager = GetGameManagerForItemCounts();
        return manager != null ? manager.teaCount : 0;
    }

    private GameManager GetGameManagerForItemCounts()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        if (gameManager == null && !warnedMissingItemGameManager)
        {
            warnedMissingItemGameManager = true;
            Debug.LogWarning("BattleManager could not find GameManager while refreshing item counts.");
        }

        return gameManager;
    }

    private void SetItemButtonsInteractable(bool interactable)
    {
        SetButtonInteractable(soapButton, interactable);
        SetButtonInteractable(teaButton, interactable);
        RefreshItemButtonLabels();
    }

    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private void LogButtonState(string reason)
    {
    }

    private string GetButtonState(Button button)
    {
        if (button == null)
            return "Missing";

        return button.interactable ? "Enabled" : "Disabled";
    }

    #endregion

    #region Messages and Feedback

    private void PlayEnemyHitFeedback()
    {
        if (enemyHitFeedback != null)
        {
            enemyHitFeedback.Play();
            return;
        }

        if (!warnedMissingEnemyHitFeedback)
        {
            warnedMissingEnemyHitFeedback = true;
            Debug.LogWarning("BattleManager enemyHitFeedback is not assigned. Enemy hit flash/shake will be skipped.");
        }
    }

    private void PlayPlayerHitFeedback()
    {
        if (playerHitFeedback != null)
        {
            playerHitFeedback.Play();
            return;
        }

        if (!warnedMissingPlayerHitFeedback)
        {
            warnedMissingPlayerHitFeedback = true;
            Debug.LogWarning("BattleManager playerHitFeedback is not assigned. Player hit flash/shake will be skipped.");
        }
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

    #endregion

    #region Helpers

    private void LogInspectorReferences()
    {
        LogReference("hpFillImage", hpFillImage);
        LogReference("spFillImage", spFillImage);
        LogReference("enemyImage", enemyImage);
        LogReference("enemyAnimationPlayer", enemyAnimationPlayer);
        LogReference("enemyHPFillImage", enemyHPFillImage);
        LogReference("playerBattleMessageText", playerBattleMessageText);
        LogReference("enemyBattleMessageText", enemyBattleMessageText);
        LogReference("playerDamagePopupPoint", playerDamagePopupPoint);
        LogReference("enemyDamagePopupPoint", enemyDamagePopupPoint);
        LogReference("damagePopupPrefab", damagePopupPrefab);
        LogReference("attackButton", attackButton);
        LogReference("blurButton", blurButton);
        LogReference("polishButton", polishButton);
        LogReference("ultimateButton", ultimateButton);
        LogReference("soapButton", soapButton);
        LogReference("teaButton", teaButton);
        LogReference("victoryPanel", victoryPanel);
        LogReference("day7InterludePanel", day7InterludePanel);
        LogReference("day7InterludeText", day7InterludeText);
        LogReference("day7InterludeTextRect", day7InterludeTextRect);
        LogReference("day7InterludeContinueButton", day7InterludeContinueButton);

        CheckFillImage("hpFillImage", hpFillImage);
        CheckFillImage("spFillImage", spFillImage);
        CheckFillImage("enemyHPFillImage", enemyHPFillImage);
    }

    private void LogReference(string fieldName, Object reference)
    {
        if (reference == null)
            Debug.LogWarning(fieldName + " is not assigned.");
    }

    private void LogBattleState(string context)
    {
    }

    private void CheckFillImage(string fieldName, Image image)
    {
        if (image == null)
            return;

        if (image.type != Image.Type.Filled)
            Debug.LogWarning(fieldName + " is assigned, but Image Type is not Filled. Set Image Type = Filled, Fill Method = Horizontal, Fill Origin = Left.");
    }

    #endregion
}
