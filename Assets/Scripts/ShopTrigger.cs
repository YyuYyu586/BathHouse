using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public static bool IsPlayerInAnyShopRange { get { return activeShopRangeCount > 0; } }

    public ShopManager shopManager;
    public GameObject pressFPrompt;

    private static int activeShopRangeCount;

    private bool isPlayerInRange;
    private DialogueManager dialogueManager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetShopRangeState()
    {
        activeShopRangeCount = 0;
    }

    private void Start()
    {
        if (pressFPrompt != null) pressFPrompt.SetActive(false);

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (SavePanelController.IsPanelOpen || IsDialoguePanelOpen() || IsShopPanelOpen())
            {
                return;
            }

            if (shopManager != null)
            {
                shopManager.OpenShop();
            }
            else
            {
                Debug.LogError("ShopTrigger 没有绑定 ShopManager。");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!isPlayerInRange)
            {
                isPlayerInRange = true;
                activeShopRangeCount++;
            }

            if (pressFPrompt != null) pressFPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ClearPlayerInRange();
            if (pressFPrompt != null) pressFPrompt.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ClearPlayerInRange();
    }

    private bool IsShopPanelOpen()
    {
        return shopManager != null && shopManager.shopPanel != null && shopManager.shopPanel.activeSelf;
    }

    private bool IsDialoguePanelOpen()
    {
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        return dialogueManager != null &&
               dialogueManager.dialoguePanel != null &&
               dialogueManager.dialoguePanel.activeSelf;
    }

    private void ClearPlayerInRange()
    {
        if (!isPlayerInRange)
            return;

        isPlayerInRange = false;
        activeShopRangeCount = Mathf.Max(0, activeShopRangeCount - 1);
    }
}
