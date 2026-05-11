using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI messageText;

    [Header("Prices")]
    public int hamburgerPrice = 10;
    public int drinkPrice = 8;
    public int passiveCertPrice = 30;
    public int bossItemPrice = 25;

    private void Start()
    {
        CloseShop();
    }

    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        RefreshUI("欢迎光临鼠鼠澡堂小卖部！");
        PausePlayer(true);
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        PausePlayer(false);
    }

    public void BuyHamburger()
    {
        TryBuy(hamburgerPrice, () => GameManager.Instance.hpHamburgerCount++, "买到了回血汉堡！");
    }

    public void BuyDrink()
    {
        TryBuy(drinkPrice, () => GameManager.Instance.spDrinkCount++, "买到了回蓝饮料！");
    }

    public void BuyPassiveCert()
    {
        if (GameManager.Instance.hasPassiveCert)
        {
            RefreshUI("你已经买过这个证书了。");
            return;
        }

        TryBuy(passiveCertPrice, () => GameManager.Instance.hasPassiveCert = true, "获得：40%回复证书！");
    }

    public void BuyBossItem()
    {
        if (GameManager.Instance.hasBossItem)
        {
            RefreshUI("Boss道具已经买过了。");
            return;
        }

        TryBuy(bossItemPrice, () => GameManager.Instance.hasBossItem = true, "获得：Boss专用道具！");
    }

    private void TryBuy(int price, System.Action onSuccess, string successMessage)
    {
        if (GameManager.Instance == null)
        {
            RefreshUI("错误：场景里没有 GameManager。");
            return;
        }

        if (GameManager.Instance.playerGold < price)
        {
            RefreshUI("金币不够，先去搓澡打工吧！");
            return;
        }

        GameManager.Instance.playerGold -= price;
        onSuccess?.Invoke();
        RefreshUI(successMessage);
    }

    private void RefreshUI(string message)
    {
        if (GameManager.Instance != null && goldText != null)
        {
            goldText.text = "金币：" + GameManager.Instance.playerGold;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    private void PausePlayer(bool pause)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.enabled = !pause;
    }
}
