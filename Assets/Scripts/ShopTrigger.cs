using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public ShopManager shopManager;
    public GameObject pressFPrompt;

    private bool isPlayerInRange;

    private void Start()
    {
        if (pressFPrompt != null) pressFPrompt.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
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
            isPlayerInRange = true;
            if (pressFPrompt != null) pressFPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (pressFPrompt != null) pressFPrompt.SetActive(false);
        }
    }
}
