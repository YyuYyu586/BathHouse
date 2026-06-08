using TMPro;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private static InteractableObject currentInteractable;

    public string interactPrompt = "\u6309 F \u8c03\u67e5";
    public string objectName;

    [TextArea(2, 4)]
    public string inspectText;

    public DialogueManager dialogueManager;
    public GameObject pressPrompt;

    private bool canInteract;
    private bool isInspecting;
    private bool warnedMissingPrompt;

    private void Start()
    {
        ValidateSetup();

        if (currentInteractable == null)
            SetPressPromptVisible(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F))
            return;

        Debug.Log(
            "[InteractableObject] F pressed. objectName=" + objectName +
            ", gameObject=" + gameObject.name +
            ", canInteract=" + canInteract +
            ", isCurrent=" + (currentInteractable == this) +
            ", isInspecting=" + isInspecting + ".");

        if (!canInteract || currentInteractable != this || isInspecting)
            return;

        if (dialogueManager == null)
        {
            Debug.LogWarning("InteractableObject missing DialogueManager. object = " + gameObject.name + ".");
            return;
        }

        if (string.IsNullOrWhiteSpace(inspectText))
        {
            Debug.LogWarning("InteractableObject inspectText is empty. object = " + gameObject.name + ".");
            return;
        }

        if (dialogueManager.dialoguePanel != null && dialogueManager.dialoguePanel.activeSelf)
        {
            Debug.Log("InteractableObject skipped because DialogueManager is already open. object = " + gameObject.name + ".");
            return;
        }

        StartInspectionDialogue();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool foundPlayer = other.CompareTag("Player");
        LogTriggerEvent("OnTriggerEnter2D", other, foundPlayer);

        if (!foundPlayer)
            return;

        canInteract = true;
        SetCurrentInteractable();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        bool foundPlayer = other.CompareTag("Player");
        LogTriggerEvent("OnTriggerStay2D", other, foundPlayer);

        if (!foundPlayer)
            return;

        canInteract = true;
        SetCurrentInteractable();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bool foundPlayer = other.CompareTag("Player");
        LogTriggerEvent("OnTriggerExit2D", other, foundPlayer);

        if (!foundPlayer)
            return;

        canInteract = false;

        if (currentInteractable == this)
        {
            currentInteractable = null;
            Debug.Log("[InteractableObject] currentInteractable cleared by trigger exit. objectName=" + objectName + ", gameObject=" + gameObject.name + ".");

            if (!isInspecting)
                SetPressPromptVisible(false);
        }
    }

    private void StartInspectionDialogue()
    {
        string speaker = string.IsNullOrWhiteSpace(objectName) ? gameObject.name : objectName;
        string safeInspectText = inspectText ?? "";
        string preview = GetPreview(safeInspectText, 20);

        DialogueLine line = new DialogueLine
        {
            textId = "Inspect_" + gameObject.name,
            speakerName = speaker,
            portraitSourceName = "",
            side = "",
            portrait = null,
            isLeftPortrait = true,
            text = safeInspectText
        };

        isInspecting = true;
        SetPressPromptVisible(false);
        dialogueManager.RemoveDialogueEndListener(OnInspectionDialogueEnd);
        dialogueManager.AddDialogueEndListener(OnInspectionDialogueEnd);

        Debug.Log(
            "[InteractableObject] starting inspection dialogue. objectName=" + objectName +
            ", gameObject=" + gameObject.name +
            ", inspectPreview=" + preview +
            ", calledDialogueManager=true.");

        dialogueManager.StartDialogue(new[] { line });
    }

    private void OnInspectionDialogueEnd()
    {
        if (dialogueManager != null)
            dialogueManager.RemoveDialogueEndListener(OnInspectionDialogueEnd);

        isInspecting = false;
        Debug.Log("[InteractableObject] inspection dialogue ended. objectName=" + objectName + ", gameObject=" + gameObject.name + ", canInteract=" + canInteract + ".");

        if (canInteract && currentInteractable == this)
            SetPressPromptVisible(true);
    }

    private void OnDisable()
    {
        if (currentInteractable == this)
        {
            currentInteractable = null;
            SetPressPromptVisible(false);
            Debug.Log("[InteractableObject] currentInteractable cleared by disable. objectName=" + objectName + ", gameObject=" + gameObject.name + ".");
        }
    }

    private void SetCurrentInteractable()
    {
        if (currentInteractable != this)
        {
            string previousName = currentInteractable != null ? currentInteractable.gameObject.name : "None";
            currentInteractable = this;
            Debug.Log(
                "[InteractableObject] currentInteractable set. previous=" + previousName +
                ", objectName=" + objectName +
                ", gameObject=" + gameObject.name + ".");
        }

        SetPressPromptVisible(true);
    }

    private void SetPressPromptVisible(bool visible)
    {
        if (visible && currentInteractable != this)
        {
            Debug.Log(
                "[InteractableObject] skipped pressPrompt SetActive(true) because this is not current. objectName=" + objectName +
                ", gameObject=" + gameObject.name + ".");
            return;
        }

        if (!visible && currentInteractable != null && currentInteractable != this)
        {
            Debug.Log(
                "[InteractableObject] skipped pressPrompt SetActive(false) because another object is current. objectName=" + objectName +
                ", gameObject=" + gameObject.name +
                ", current=" + currentInteractable.gameObject.name + ".");
            return;
        }

        if (pressPrompt == null)
        {
            if (!warnedMissingPrompt)
            {
                Debug.LogWarning("InteractableObject missing pressPrompt. object = " + gameObject.name + ".");
                warnedMissingPrompt = true;
            }

            return;
        }

        TextMeshProUGUI promptText = pressPrompt.GetComponentInChildren<TextMeshProUGUI>(true);
        if (promptText != null)
            promptText.text = interactPrompt;

        pressPrompt.SetActive(visible);
        Debug.Log(
            "[InteractableObject] pressPrompt SetActive(" + visible + "). objectName=" + objectName +
            ", gameObject=" + gameObject.name +
            ", canInteract=" + canInteract +
            ", isCurrent=" + (currentInteractable == this) + ".");
    }

    private void LogTriggerEvent(string eventName, Collider2D other, bool foundPlayer)
    {
        Debug.Log(
            "[InteractableObject] " + eventName +
            ". objectName=" + objectName +
            ", gameObject=" + gameObject.name +
            ", other.name=" + other.name +
            ", other.tag=" + other.tag +
            ", other.layer=" + LayerMask.LayerToName(other.gameObject.layer) +
            ", foundPlayer=" + foundPlayer +
            ", canInteract=" + canInteract +
            ", isCurrent=" + (currentInteractable == this) + ".");
    }

    private void ValidateSetup()
    {
        if (dialogueManager == null)
            Debug.LogWarning("InteractableObject missing DialogueManager. object = " + gameObject.name + ".");

        if (pressPrompt == null)
            Debug.LogWarning("InteractableObject missing pressPrompt. object = " + gameObject.name + ".");

        if (string.IsNullOrWhiteSpace(inspectText))
            Debug.LogWarning("InteractableObject inspectText is empty. object = " + gameObject.name + ".");

        Collider2D triggerCollider = GetComponent<Collider2D>();
        Collider2D parentCollider = transform.parent != null ? transform.parent.GetComponent<Collider2D>() : null;

        string currentColliderIsTrigger = triggerCollider != null ? triggerCollider.isTrigger.ToString() : "Missing";
        string parentColliderExists = (parentCollider != null).ToString();
        string parentColliderIsTrigger = parentCollider != null ? parentCollider.isTrigger.ToString() : "Missing";

        Debug.Log(
            "[InteractableObject] collider setup. objectName=" + objectName +
            ", gameObject=" + gameObject.name +
            ", currentColliderIsTrigger=" + currentColliderIsTrigger +
            ", hasParentCollider2D=" + parentColliderExists +
            ", parentColliderIsTrigger=" + parentColliderIsTrigger +
            ", pressPromptBound=" + (pressPrompt != null) + ".");

        if (triggerCollider == null || !triggerCollider.isTrigger)
        {
            Debug.LogWarning(
                "InteractableObject should be attached to InspectTrigger, and InspectTrigger Collider2D must have Is Trigger = true. objectName=" +
                objectName + ", gameObject=" + gameObject.name + ".");
        }
    }

    private string GetPreview(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text.Length <= maxLength ? text : text.Substring(0, maxLength);
    }
}
