using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI detailText;

    [Header("Prices")]
    public int soapPrice = 15;
    public int teaPrice = 15;
    public int waterLadlePrice = 60;
    public int towelPrice = 100;

    private string selectedItem = "";

    private void Start()
    {
        ApplyP0Prices();
        BindBackButtons();
        CloseShop();
    }

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        selectedItem = "";
        RefreshUI("欢迎来到商店！");
        ShowDetail("请选择想购买的商品。");
        PausePlayer(true);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        PausePlayer(false);
    }

    public void SelectSoap()
    {
        selectedItem = "soap";
        ShowDetail("肥皂\n价格:15 金币\n效果:战斗中恢复 30 HP。");
    }

    public void SelectTea()
    {
        selectedItem = "tea";
        ShowDetail("花茶\n价格:15 金币\n效果:战斗中恢复 20 SP。");
    }

    public void SelectWaterLadle()
    {
        selectedItem = "waterLadle";
        ShowDetail("【质变】高级证书\n价格:60 金币\n被动效果:每天战斗结束后，自动恢复 40% HP 和 SP。");
    }

    public void SelectTowel()
    {
        selectedItem = "towel";
        ShowDetail("【终极】黄金搓澡巾\n价格:100 金币\n决战道具:第七天 Boss 战中使用，可让 Boss 眩晕一回合。");
    }

    public void ConfirmBuy()
    {
        if (selectedItem == "")
        {
            RefreshUI("请先选择一个商品。");
            return;
        }

        if (selectedItem == "soap")
        {
            TryBuy(soapPrice, () => GameManager.Instance.soapCount++, "买到了肥皂！");
        }
        else if (selectedItem == "tea")
        {
            TryBuy(teaPrice, () => GameManager.Instance.teaCount++, "买到了花茶！");
        }
        else if (selectedItem == "waterLadle")
        {
            if (GameManager.Instance.hasWaterLadle)
            {
                RefreshUI("你已经买过【质变】高级证书了。");
                return;
            }

            TryBuy(waterLadlePrice, () => GameManager.Instance.hasWaterLadle = true, "获得【质变】高级证书！");
        }
        else if (selectedItem == "towel")
        {
            if (GameManager.Instance.hasGoldenTowel)
            {
                RefreshUI("你已经买过黄金搓澡巾了。");
                return;
            }

            TryBuy(towelPrice, () => GameManager.Instance.hasGoldenTowel = true, "获得【终极】黄金搓澡巾！");
        }
    }

    public void CancelSelection()
    {
        selectedItem = "";
        ShowDetail("请选择一个商品。");
        RefreshUI("已取消选择。");
    }

    public void ExitShop()
    {
        CloseShop();
    }

    private void ApplyP0Prices()
    {
        soapPrice = 15;
        teaPrice = 15;
        Debug.Log("ShopManager P0 prices applied. soapPrice=" + soapPrice + ", teaPrice=" + teaPrice + ".");
    }

    private void BindBackButtons()
    {
        if (shopPanel == null)
            return;

        Button[] buttons = shopPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            string buttonName = button.gameObject.name.ToLowerInvariant();
            if (buttonName == "back" ||
                buttonName == "close" ||
                buttonName == "exit" ||
                buttonName == "return" ||
                button.gameObject.name == "返回")
            {
                button.onClick.AddListener(CloseShop);
                Debug.Log("ShopManager bound close action to button: " + button.gameObject.name);
            }
        }
    }

    private void TryBuy(int price, System.Action onSuccess, string successMessage)
    {
        if (GameManager.Instance.playerGold >= price)
        {
            GameManager.Instance.playerGold -= price;
            onSuccess.Invoke();
            RefreshUI(successMessage);
            Debug.Log("Shop purchase success. item=" + selectedItem + ", price=" + price + ", gold=" + GameManager.Instance.playerGold + ", soapCount=" + GameManager.Instance.soapCount + ", teaCount=" + GameManager.Instance.teaCount + ".");
        }
        else
        {
            RefreshUI("金币不足，无法购买！");
            Debug.Log("Shop purchase failed. item=" + selectedItem + ", price=" + price + ", gold=" + GameManager.Instance.playerGold + ".");
        }
    }

    private void RefreshUI(string message)
    {
        if (goldText != null)
            goldText.text = "金币：" + GameManager.Instance.playerGold;

        if (messageText != null)
            messageText.text = message;
    }

    private void ShowDetail(string text)
    {
        if (detailText != null)
            detailText.text = text;
    }

    private void PausePlayer(bool shouldPause)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = !shouldPause;
    }
}
