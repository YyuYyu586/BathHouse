using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TutorialGuideController : MonoBehaviour
{
    [FormerlySerializedAs("tutorialQuestPanel")]
    [SerializeField] private GameObject missionFrame;

    [FormerlySerializedAs("tutorialQuestText")]
    [SerializeField] private TextMeshProUGUI missionText;

    [SerializeField] private TextMeshProUGUI missionDayText;

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject shopPanel;

    private enum TutorialState
    {
        None,
        WaitingForCustomer,
        WaitingForCombat,
        Completed
    }

    private const string BeginnerInitialMissionText =
        "新手任务\n" +
        "◆靠近顾客，按F接待\n" +
        "◇使用WASD移动\n" +
        "◇战斗前可去柜台购买道具\n" +
        "◇部分物件可按F调查";

    private const string BeginnerAfterCustomerMissionText =
        "新手任务\n" +
        "✓已接待顾客\n" +
        "◆前往蓝色地毯，按F开始战斗\n" +
        "可选准备\n" +
        "◇去柜台购买肥皂或花茶\n" +
        "◇调查澡堂里的物件";

    private const string RegularInitialMissionText =
        "今日目标\n" +
        "◆接待今天的顾客\n" +
        "◇战斗前可去柜台购买道具\n" +
        "◇部分物件可按F调查";

    private const string RegularAfterCustomerMissionText =
        "今日目标\n" +
        "✓已接待今天的顾客\n" +
        "◆前往蓝色地毯，按F开始战斗\n" +
        "可选准备\n" +
        "◇去柜台购买肥皂或花茶\n" +
        "◇调查澡堂里的物件";

    private TutorialState state = TutorialState.None;
    private bool shouldRunMissionFrame;
    private bool warnedMissingMissionReferences;

    private void OnEnable()
    {
        CustomerTrigger.CustomerInteractionCompleted += OnCustomerInteractionCompleted;
        CombatTrigger.CombatTriggered += OnCombatTriggered;
    }

    private void OnDisable()
    {
        CustomerTrigger.CustomerInteractionCompleted -= OnCustomerInteractionCompleted;
        CombatTrigger.CombatTriggered -= OnCombatTriggered;
    }

    private void Start()
    {
        InitializeForCurrentDay();
    }

    private void Update()
    {
        if (!shouldRunMissionFrame)
            return;

        if (!ShouldShowMissionFrameForCurrentDay())
        {
            HideQuestPanel();
            enabled = false;
            return;
        }

        RefreshQuestPanelVisibility();
    }

    private void InitializeForCurrentDay()
    {
        shouldRunMissionFrame = ShouldShowMissionFrameForCurrentDay();

        if (!shouldRunMissionFrame)
        {
            HideQuestPanel();
            enabled = false;
            return;
        }

        if (!HasRequiredMissionReferences())
        {
            HideQuestPanel();
            return;
        }

        state = HasCompletedCustomerInteraction() ? TutorialState.WaitingForCombat : TutorialState.WaitingForCustomer;
        UpdateQuestText();
        RefreshQuestPanelVisibility();
    }

    private bool ShouldShowMissionFrameForCurrentDay()
    {
        GameManager gameManager = GameManager.Instance != null ? GameManager.Instance : GameManager.EnsureInstance();

        if (gameManager.currentGameMode == GameMode.MainStory)
            return gameManager.currentDay >= 2 && gameManager.currentDay <= 7;

        if (gameManager.currentGameMode == GameMode.DiabetesDLC)
            return gameManager.currentDay >= 1 && gameManager.currentDay <= 3;

        return false;
    }

    private bool IsBeginnerGuideDay()
    {
        GameManager gameManager = GameManager.Instance != null ? GameManager.Instance : GameManager.EnsureInstance();

        return (gameManager.currentGameMode == GameMode.MainStory && gameManager.currentDay == 2) ||
               (gameManager.currentGameMode == GameMode.DiabetesDLC && gameManager.currentDay == 1);
    }

    private string GetMissionDayTitle()
    {
        GameManager gameManager = GameManager.Instance != null ? GameManager.Instance : GameManager.EnsureInstance();

        if (gameManager.currentGameMode == GameMode.DiabetesDLC)
            return "特别活动 DAY " + gameManager.currentDay;

        if (gameManager.currentDay == 2)
            return "DAY 2 试营业";

        return "DAY " + gameManager.currentDay;
    }

    private string GetInitialMissionText()
    {
        return IsBeginnerGuideDay() ? BeginnerInitialMissionText : RegularInitialMissionText;
    }

    private string GetAfterCustomerMissionText()
    {
        return IsBeginnerGuideDay() ? BeginnerAfterCustomerMissionText : RegularAfterCustomerMissionText;
    }

    private bool HasCompletedCustomerInteraction()
    {
        CustomerTrigger[] customerTriggers = FindObjectsOfType<CustomerTrigger>(true);

        for (int i = 0; i < customerTriggers.Length; i++)
        {
            if (customerTriggers[i] != null && customerTriggers[i].HasTalked)
                return true;
        }

        return false;
    }

    private void OnCustomerInteractionCompleted(CustomerTrigger customerTrigger)
    {
        if (!shouldRunMissionFrame || !ShouldShowMissionFrameForCurrentDay() || state == TutorialState.Completed)
            return;

        if (!HasRequiredMissionReferences())
            return;

        state = TutorialState.WaitingForCombat;
        UpdateQuestText();
        RefreshQuestPanelVisibility();
    }

    private void OnCombatTriggered(CombatTrigger combatTrigger)
    {
        if (!shouldRunMissionFrame || !ShouldShowMissionFrameForCurrentDay())
            return;

        state = TutorialState.Completed;
        HideQuestPanel();
    }

    private void UpdateQuestText()
    {
        if (!HasRequiredMissionReferences())
            return;

        if (missionDayText != null)
            missionDayText.text = GetMissionDayTitle();

        if (state == TutorialState.WaitingForCustomer)
        {
            missionText.text = GetInitialMissionText();
        }
        else if (state == TutorialState.WaitingForCombat)
        {
            missionText.text = GetAfterCustomerMissionText();
        }
    }

    private void RefreshQuestPanelVisibility()
    {
        if (state == TutorialState.Completed || state == TutorialState.None)
        {
            HideQuestPanel();
            return;
        }

        SetQuestPanelVisible(!ShouldTemporarilyHideQuestPanel());
    }

    private bool ShouldTemporarilyHideQuestPanel()
    {
        if (SavePanelController.IsPanelOpen)
            return true;

        if (IsBlockingPanelOpen(dialoguePanel))
            return true;

        if (IsBlockingPanelOpen(shopPanel))
            return true;

        return false;
    }

    private bool IsBlockingPanelOpen(GameObject panel)
    {
        if (panel == null)
            return false;

        if (missionFrame != null &&
            (panel == missionFrame || panel.transform.IsChildOf(missionFrame.transform)))
        {
            return false;
        }

        return panel.activeInHierarchy;
    }

    private bool HasRequiredMissionReferences()
    {
        if (missionFrame != null && missionText != null)
            return true;

        if (!warnedMissingMissionReferences)
        {
            Debug.LogWarning("TutorialGuideController needs missionFrame and missionText references.");
            warnedMissingMissionReferences = true;
        }

        return false;
    }

    private void SetQuestPanelVisible(bool visible)
    {
        if (missionFrame == null)
            return;

        if (missionFrame.activeSelf == visible)
            return;

        missionFrame.SetActive(visible);
    }

    private void HideQuestPanel()
    {
        SetQuestPanelVisible(false);
    }
}
