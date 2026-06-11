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

    private const string MissionDayTitle = "DAY 2 试营业";

    private const string InitialQuestText =
        "新手任务\n" +
        "◆靠近顾客，按F接待\n" +
        "◇使用WASD移动\n" +
        "◇战斗前可去商店购买道具\n" +
        "◇部分物件可按F调查";

    private const string AfterCustomerQuestText =
        "新手任务\n" +
        "✓已接待顾客\n" +
        "◆前往蓝色地毯，按F开始战斗\n\n" +
        "可选准备\n" +
        "◇去商店购买肥皂或花茶\n" +
        "◇调查澡堂里的物件";

    private TutorialState state = TutorialState.None;
    private bool isMainStoryDay2;

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
        ResolveOptionalPanelReferences();
        InitializeForCurrentDay();
    }

    private void Update()
    {
        if (!isMainStoryDay2)
            return;

        if (!IsMainStoryDay2())
        {
            HideQuestPanel();
            enabled = false;
            return;
        }

        RefreshQuestPanelVisibility();
    }

    private void InitializeForCurrentDay()
    {
        isMainStoryDay2 = IsMainStoryDay2();

        if (!isMainStoryDay2)
        {
            HideQuestPanel();
            enabled = false;
            return;
        }

        state = HasCompletedCustomerInteraction() ? TutorialState.WaitingForCombat : TutorialState.WaitingForCustomer;
        UpdateQuestText();
        RefreshQuestPanelVisibility();
    }

    private bool IsMainStoryDay2()
    {
        GameManager gameManager = GameManager.Instance != null ? GameManager.Instance : GameManager.EnsureInstance();
        return gameManager.currentGameMode == GameMode.MainStory && gameManager.currentDay == 2;
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
        if (!isMainStoryDay2 || !IsMainStoryDay2() || state == TutorialState.Completed)
            return;

        state = TutorialState.WaitingForCombat;
        UpdateQuestText();
        RefreshQuestPanelVisibility();
    }

    private void OnCombatTriggered(CombatTrigger combatTrigger)
    {
        if (!isMainStoryDay2 || !IsMainStoryDay2())
            return;

        state = TutorialState.Completed;
        HideQuestPanel();
    }

    private void UpdateQuestText()
    {
        if (missionText == null)
            return;

        if (missionDayText != null)
            missionDayText.text = MissionDayTitle;

        if (state == TutorialState.WaitingForCustomer)
        {
            missionText.text = InitialQuestText;
        }
        else if (state == TutorialState.WaitingForCombat)
        {
            missionText.text = AfterCustomerQuestText;
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
        ResolveOptionalPanelReferences();

        if (SavePanelController.IsPanelOpen)
            return true;

        if (dialoguePanel != null && dialoguePanel.activeInHierarchy)
            return true;

        if (shopPanel != null && shopPanel.activeInHierarchy)
            return true;

        return false;
    }

    private void ResolveOptionalPanelReferences()
    {
        if (dialoguePanel == null)
        {
            DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
                dialoguePanel = dialogueManager.dialoguePanel;
        }

        if (shopPanel == null)
        {
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null)
                shopPanel = shopManager.shopPanel;
        }
    }

    private void SetQuestPanelVisible(bool visible)
    {
        if (missionFrame != null)
            missionFrame.SetActive(visible);
    }

    private void HideQuestPanel()
    {
        SetQuestPanelVisible(false);
    }
}
