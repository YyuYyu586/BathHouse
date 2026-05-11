using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI messageText;

    [Header("Prices")]
    public int WaterLadlePrice = 10;
    public int SoapPrice = 8;
    public int TeaPrice = 30;
    public int TowelPrice = 25;

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
     public void BuyWaterLadle()
   {
    TryBuy(WaterLadlePrice, () => GameManager.Instance.waterLadleCount++, "买到了水瓢！");
   }  

     public void BuySoap()
    {
    TryBuy(SoapPrice, () => GameManager.Instance.soapCount++, "买到了肥皂！");
    }

    public void BuyTea()
   {
    TryBuy(TeaPrice, () => GameManager.Instance.teaCount++, "买到了茶！");
   }

    public void BuyTowel()
    { 
    TryBuy(TowelPrice, () => GameManager.Instance.towelCount++, "买到了毛巾！");
    } 

    private void TryBuy(int price, System.Action onSuccess, string successMessage)
    {
        if (GameManager.Instance.gold >= price)
        {
            GameManager.Instance.gold -= price;
            onSuccess.Invoke();
            RefreshUI(successMessage);
        }
        else
        {
            RefreshUI("金币不足，无法购买！");
        }
    }

    private void RefreshUI(string message)
    {
        if (goldText != null) goldText.text = $"金币: {GameManager.Instance.gold}";
        if (messageText != null) messageText.text = message;
    }

    private void PausePlayer(bool shouldPause)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.enabled = !shouldPause;
        }
    }
}