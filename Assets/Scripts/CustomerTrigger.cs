using TMPro;
using UnityEngine;

public class CustomerTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueManager dialogueManager;
    public BathhouseDayStoryController bathhouseDayStoryController;

    [Header("Fallback Dialogue")]
    public DialogueLine[] lines;

    [Header("State")]
    public GameObject exclamationMark;
    public GameObject combatTrigger;
    public TextMeshProUGUI preparationHintText;

    [Header("Collision")]
    public float interactionDistance = 1.8f;
    public bool forceCustomerColliderBlocksPlayer = true;

    private bool playerNear = false;
    private bool hasTalked = false;
    private bool isDialoguePlaying = false;
    private ShopManager shopManager;
    private Transform playerTransform;
    private Collider2D playerCollider;
    private Collider2D customerCollider;

    private void Start()
    {
        HidePreparationHint();

        if (bathhouseDayStoryController == null)
            bathhouseDayStoryController = FindObjectOfType<BathhouseDayStoryController>();

        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();

        FindPlayerForPhysicsLog();
        ConfigureCustomerCollider();

        GameManager gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager != null ? gameManager.currentDay : -1;
        Debug.Log(
            "CustomerTrigger Start. currentDay = " + currentDay +
            ", customer = " + gameObject.name +
            ", exclamationMarkBound = " + (exclamationMark != null) + ".");

        if (exclamationMark != null)
        {
            exclamationMark.SetActive(true);
            Debug.Log("CustomerTrigger exclamation initialized. customer = " + gameObject.name + ", exclamationActive = " + exclamationMark.activeSelf + ".");
        }
        else
        {
            Debug.LogWarning("CustomerTrigger missing exclamationMark reference. customer = " + gameObject.name + ".");
        }

        if (combatTrigger != null)
        {
            combatTrigger.SetActive(false);
            Debug.Log("CustomerTrigger combatTrigger initialized. customer = " + gameObject.name + ", combatTriggerActive = " + combatTrigger.activeSelf + ".");
        }
    }

    private void Update()
    {
        RefreshPlayerNearByDistance();

        if (!Input.GetKeyDown(KeyCode.F))
            return;

        Debug.Log(
            "CustomerTrigger F pressed. customer = " + gameObject.name +
            ", playerNear = " + playerNear +
            ", hasTalked = " + hasTalked +
            ", isDialoguePlaying = " + isDialoguePlaying +
            ", interactionDistance = " + interactionDistance +
            ", exclamationActive = " + GetExclamationActiveState() + ".");

        TryStartCustomerDialogue(false);
    }

    // Trailer helper only: starts the same customer dialogue path without requiring the player to stand in range.
    public bool TrailerTryStartDialogue()
    {
        return TryStartCustomerDialogue(true);
    }

    private bool TryStartCustomerDialogue(bool ignorePlayerRange)
    {
        if ((!ignorePlayerRange && !playerNear) || hasTalked || isDialoguePlaying)
            return false;

        if (ShouldBlockCustomerInteraction())
            return false;

        if (dialogueManager == null)
        {
            Debug.LogError("CustomerTrigger needs a DialogueManager reference.");
            return false;
        }

        DialogueLine[] dialogueLines = GetDialogueLinesForToday();
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("CustomerTrigger has no dialogue lines to play.");
            return false;
        }

        dialogueManager.RemoveDialogueEndListener(OnBeforeCombatDialogueEnd);
        dialogueManager.AddDialogueEndListener(OnBeforeCombatDialogueEnd);
        isDialoguePlaying = true;
        Debug.Log(
            "BeforeCombat dialogue starting. currentDay = " + GameManager.EnsureInstance().currentDay +
            ", customer = " + gameObject.name +
            ", beforeCombatStarted = true" +
            ", exclamationMarkBound = " + (exclamationMark != null) +
            ", exclamationActiveBeforeDialogue = " + GetExclamationActiveState() + ".");
        dialogueManager.StartDialogue(dialogueLines);
        return true;
    }

    private DialogueLine[] GetDialogueLinesForToday()
    {
        if (bathhouseDayStoryController != null)
        {
            GameManager gameManager = GameManager.EnsureInstance();
            int currentDay = gameManager.currentDay;
            int index = currentDay - 1;

            if (bathhouseDayStoryController.beforeCombatDialogues != null &&
                index >= 0 &&
                index < bathhouseDayStoryController.beforeCombatDialogues.Length &&
                bathhouseDayStoryController.beforeCombatDialogues[index] != null &&
                bathhouseDayStoryController.beforeCombatDialogues[index].lines != null &&
                bathhouseDayStoryController.beforeCombatDialogues[index].lines.Length > 0)
            {
                DialogueLine[] todayLines = bathhouseDayStoryController.beforeCombatDialogues[index].lines;
                int linesCount = todayLines.Length;
                Debug.Log("Using CSV BeforeCombat dialogue. currentDay = " + currentDay + ", beforeCombatIndex = " + index + ", lines = " + linesCount + ".");
                LogBeforeCombatLines(currentDay, index, todayLines);
                return todayLines;
            }

            Debug.LogWarning("No CSV BeforeCombat dialogue found. currentDay = " + currentDay + ", beforeCombatIndex = " + index + ". Falling back to CustomerTrigger.lines.");
        }
        else
        {
            Debug.LogWarning("CustomerTrigger has no BathhouseDayStoryController reference. Falling back to CustomerTrigger.lines.");
        }

        if (lines != null && lines.Length > 0)
        {
            Debug.LogWarning("Using fallback CustomerTrigger.lines. lines = " + lines.Length + ".");
            return lines;
        }

        Debug.LogWarning("CustomerTrigger could not find CSV BeforeCombat dialogue or fallback lines.");
        return null;
    }

    private void LogBeforeCombatLines(int currentDay, int dialogueIndex, DialogueLine[] dialogueLines)
    {
        for (int i = 0; i < dialogueLines.Length; i++)
        {
            DialogueLine line = dialogueLines[i];
            if (line == null)
            {
                Debug.LogWarning("BeforeCombat line is null. currentDay=" + currentDay + ", dialogueIndex=" + dialogueIndex + ", lineIndex=" + i + ".");
                continue;
            }

            string portraitName = line.portrait != null ? line.portrait.name : "None";
            Debug.Log(
                "BeforeCombat line loaded: currentDay=" + currentDay +
                ", beforeCombatIndex=" + dialogueIndex +
                ", lineIndex=" + i +
                ", lineOrder=" + line.order +
                ", textId=" + line.textId +
                ", speakerName=" + line.speakerName +
                ", portraitSource=" + line.portraitSourceName +
                ", portraitSprite=" + portraitName +
                ", isLeftPortrait=" + line.isLeftPortrait +
                ", side=" + line.side + ".");

            if (!string.IsNullOrWhiteSpace(line.portraitSourceName) &&
                line.portraitSourceName.ToLowerInvariant() != "none" &&
                line.portrait == null)
            {
                Debug.LogWarning(
                    "BeforeCombat portrait is missing after import. currentDay=" + currentDay +
                    ", beforeCombatIndex=" + dialogueIndex +
                    ", lineIndex=" + i +
                    ", textId=" + line.textId +
                    ", speakerName=" + line.speakerName +
                    ", portraitSource=" + line.portraitSourceName + ".");
            }
        }
    }

    private void OnBeforeCombatDialogueEnd()
    {
        if (dialogueManager != null)
            dialogueManager.RemoveDialogueEndListener(OnBeforeCombatDialogueEnd);

        Debug.Log("CustomerTrigger OnDialogueEnd triggered. customer = " + gameObject.name + ".");
        CompleteCustomerInteraction();
    }

    private void CompleteCustomerInteraction()
    {
        GameManager gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager != null ? gameManager.currentDay : -1;
        string customerName = gameObject.name;
        string exclamationBefore = GetExclamationActiveState();

        isDialoguePlaying = false;
        hasTalked = true;

        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
            Debug.Log(
                "CompleteCustomerInteraction. ExclamationMark SetActive(false). currentDay = " + currentDay +
                ", customer = " + customerName +
                ", exclamationActiveBefore = " + exclamationBefore +
                ", exclamationActiveAfterDialogue = " + exclamationMark.activeSelf + ".");
        }
        else
        {
            Debug.LogWarning("CompleteCustomerInteraction missing exclamationMark reference. customer = " + customerName + ".");
        }

        if (combatTrigger != null)
        {
            combatTrigger.SetActive(true);
            Debug.Log(
                "CompleteCustomerInteraction. combatTrigger SetActive(true). currentDay = " + currentDay +
                ", customer = " + customerName +
                ", combatTriggerActive = " + combatTrigger.activeSelf + ".");

            ShowPreparationHint();
        }
        else
        {
            Debug.LogWarning("CompleteCustomerInteraction missing combatTrigger reference. customer = " + customerName + ".");
        }

        Debug.Log(
            "CompleteCustomerInteraction finished. currentDay = " + currentDay +
            ", customer = " + customerName +
            ", hasTalked = " + hasTalked +
            ", customerWillSetActiveFalse = true.");

        gameObject.SetActive(false);
        Debug.Log("Customer GameObject SetActive(false). customer = " + customerName + ".");
    }

    private void ShowPreparationHint()
    {
        const string hint = "今天的客人已经准备好了。如果担心状态不好，可以先去前台买点道具，准备好了再去工作区开始接待吧。";

        if (preparationHintText != null)
        {
            preparationHintText.text = hint;
            preparationHintText.gameObject.SetActive(true);
            Debug.Log("Preparation hint shown on UI. customer = " + gameObject.name + ".");
            return;
        }

        Debug.Log("Preparation hint text is not assigned. Hint: " + hint);
    }

    private void HidePreparationHint()
    {
        if (preparationHintText != null)
            preparationHintText.gameObject.SetActive(false);
        else
            Debug.Log("Preparation hint text is not assigned at Start. It will be skipped until assigned in Inspector.");
    }

    private string GetExclamationActiveState()
    {
        return exclamationMark != null ? exclamationMark.activeSelf.ToString() : "Missing";
    }

    private bool ShouldBlockCustomerInteraction()
    {
        if (SavePanelController.IsPanelOpen)
        {
            Debug.Log("CustomerTrigger skipped because SavePanel is open. customer = " + gameObject.name + ".");
            return true;
        }

        if (IsShopPanelOpen())
        {
            Debug.Log("CustomerTrigger skipped because ShopPanel is open. customer = " + gameObject.name + ".");
            return true;
        }

        if (ShopTrigger.IsPlayerInAnyShopRange)
        {
            Debug.Log("CustomerTrigger skipped because player is in shop range. customer = " + gameObject.name + ".");
            return true;
        }

        if (IsDialoguePanelOpen())
        {
            Debug.Log("CustomerTrigger skipped because DialoguePanel is already open. customer = " + gameObject.name + ".");
            return true;
        }

        return false;
    }

    private bool IsShopPanelOpen()
    {
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();

        return shopManager != null &&
               shopManager.shopPanel != null &&
               shopManager.shopPanel.activeSelf;
    }

    private bool IsDialoguePanelOpen()
    {
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        return dialogueManager != null &&
               dialogueManager.dialoguePanel != null &&
               dialogueManager.dialoguePanel.activeSelf;
    }

    private void FindPlayerForPhysicsLog()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("CustomerTrigger could not find Player by tag. customer = " + gameObject.name + ".");
            return;
        }

        playerTransform = player.transform;
        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
        playerCollider = player.GetComponent<Collider2D>();

        Debug.Log(
            "CustomerTrigger player physics check. customer = " + gameObject.name +
            ", player = " + player.name +
            ", playerRigidbody2DExists = " + (playerRigidbody != null) +
            ", playerCollider2DExists = " + (playerCollider != null) +
            ", playerLayer = " + LayerMask.LayerToName(player.layer) + ".");

        if (playerRigidbody == null)
            Debug.LogWarning("Player needs a Rigidbody2D for customer blocking. customer = " + gameObject.name + ".");

        if (playerCollider == null)
            Debug.LogWarning("Player needs a Collider2D for customer blocking. customer = " + gameObject.name + ".");
    }

    private void ConfigureCustomerCollider()
    {
        customerCollider = GetComponent<Collider2D>();
        if (customerCollider == null)
        {
            Debug.LogWarning("CustomerTrigger needs a Collider2D on the customer object for blocking. customer = " + gameObject.name + ".");
            return;
        }

        bool wasTrigger = customerCollider.isTrigger;
        Debug.Log(
            "CustomerTrigger customer collider check. customer = " + gameObject.name +
            ", customerCollider2DExists = true" +
            ", customerColliderIsTrigger = " + wasTrigger +
            ", customerLayer = " + LayerMask.LayerToName(gameObject.layer) + ".");

        if (forceCustomerColliderBlocksPlayer && customerCollider.isTrigger)
        {
            customerCollider.isTrigger = false;
            Debug.Log(
                "CustomerTrigger set customer Collider2D IsTrigger to false for blocking. customer = " + gameObject.name +
                ", wasTrigger = " + wasTrigger +
                ", isTriggerNow = " + customerCollider.isTrigger + ".");
        }
    }

    private void RefreshPlayerNearByDistance()
    {
        if (playerTransform == null)
            return;

        float distance = GetDistanceToPlayer();
        bool isNearByDistance = distance <= interactionDistance;

        if (playerNear != isNearByDistance)
        {
            playerNear = isNearByDistance;
            Debug.Log(
                "CustomerTrigger distance interaction state changed. customer = " + gameObject.name +
                ", playerNear = " + playerNear +
                ", distance = " + distance +
                ", interactionDistance = " + interactionDistance + ".");
        }
    }

    private float GetDistanceToPlayer()
    {
        if (customerCollider != null && playerCollider != null)
        {
            ColliderDistance2D colliderDistance = customerCollider.Distance(playerCollider);
            return colliderDistance.distance;
        }

        return Vector2.Distance(transform.position, playerTransform.position);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isPlayer = collision.collider.CompareTag("Player");
        Debug.Log(
            "CustomerTrigger OnCollisionEnter2D. customer = " + gameObject.name +
            ", other = " + collision.collider.name +
            ", isPlayer = " + isPlayer +
            ", customerLayer = " + LayerMask.LayerToName(gameObject.layer) +
            ", otherLayer = " + LayerMask.LayerToName(collision.collider.gameObject.layer) + ".");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            playerNear = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player");
        Debug.Log(
            "CustomerTrigger OnTriggerEnter2D. customer = " + gameObject.name +
            ", other = " + other.name +
            ", isPlayer = " + isPlayer +
            ", customerLayer = " + LayerMask.LayerToName(gameObject.layer) +
            ", otherLayer = " + LayerMask.LayerToName(other.gameObject.layer) + ".");

        if (isPlayer)
            playerNear = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("CustomerTrigger touching: " + other.name);

        if (other.CompareTag("Player"))
            playerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            Debug.Log("Player left CustomerTrigger range.");
        }
    }
}
