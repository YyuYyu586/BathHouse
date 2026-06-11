using UnityEngine;
using UnityEngine.SceneManagement;

// Opens the combat scene when the player presses F inside this trigger.
public class CombatTrigger : MonoBehaviour
{
    public static event System.Action<CombatTrigger> CombatTriggered;

    [SerializeField] private bool blockDayOneCombat = true;
    [SerializeField] private string combatSceneName = "CombatScene";
    [SerializeField] private float uiCloseInputCooldown = 0.15f;

    private bool playerInRange;
    private DialogueManager dialogueManager;
    private ShopManager shopManager;
    private bool wasBlockedByUi;
    private float blockCombatInputUntil;

    private void Update()
    {
        bool blockedByUi = ShouldBlockCombatEntry();

        if (wasBlockedByUi && !blockedByUi)
            blockCombatInputUntil = Time.unscaledTime + uiCloseInputCooldown;

        wasBlockedByUi = blockedByUi;

        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (blockedByUi)
                return;

            if (Time.unscaledTime < blockCombatInputUntil)
                return;

            GameManager gameManager = GameManager.EnsureInstance();

            if (gameManager.currentGameMode == GameMode.MainStory && blockDayOneCombat && gameManager.currentDay <= 1)
            {
                Debug.Log("Day 1 is story only. Advancing to Day 2 combat for the demo loop.");
                gameManager.AdvanceDay();
            }

            CombatTriggered?.Invoke(this);
            SceneManager.LoadScene(combatSceneName);
        }
    }

    private bool ShouldBlockCombatEntry()
    {
        if (SavePanelController.IsPanelOpen)
            return true;

        if (IsDialoguePanelOpen())
            return true;

        if (IsShopPanelOpen())
            return true;

        return false;
    }

    private bool IsDialoguePanelOpen()
    {
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        return dialogueManager != null &&
               dialogueManager.dialoguePanel != null &&
               dialogueManager.dialoguePanel.activeSelf;
    }

    private bool IsShopPanelOpen()
    {
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();

        return shopManager != null &&
               shopManager.shopPanel != null &&
               shopManager.shopPanel.activeSelf;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
