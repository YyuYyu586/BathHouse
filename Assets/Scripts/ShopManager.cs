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

    [Header("Purchase Feedback")]
    public PurchaseFeedbackPanelController purchaseFeedback;
    public Sprite soapIcon;
    public Sprite teaIcon;
    public Sprite waterLadleIcon;
    public Sprite towelIcon;

    [Header("Price Texts")]
    public TextMeshProUGUI soapPriceText;
    public TextMeshProUGUI teaPriceText;
    public TextMeshProUGUI waterLadlePriceText;
    public TextMeshProUGUI towelPriceText;
    public Color unaffordablePriceColor = Color.red;

    [Header("Prices")]
    public int soapPrice = 15;
    public int teaPrice = 15;
    public int waterLadlePrice = 60;
    public int towelPrice = 100;

    private string selectedItem = "";
    private Color soapPriceDefaultColor = Color.white;
    private Color teaPriceDefaultColor = Color.white;
    private Color waterLadlePriceDefaultColor = Color.white;
    private Color towelPriceDefaultColor = Color.white;
    private bool priceDefaultColorsCaptured;
    private bool warnedMissingGameManager;

    private void Start()
    {
        ConfigureRichText();
        ResolveOptionalReferences();
        ApplyP0Prices();
        BindBackButtons();
        CloseShop();
        RefreshPriceTexts();
    }

    public void OpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopManager cannot open shop because shopPanel is not assigned.");
            return;
        }

        shopPanel.SetActive(true);

        selectedItem = "";
        RefreshUI("欢迎来到商店！");
        ShowDetail("请选择想购买的商品。");
        RefreshPriceTexts();
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
        ShowDetail("肥皂\n价格:15 金币\n效果:战斗中按 H 使用，恢复 30 点 HP。");
    }

    public void SelectTea()
    {
        selectedItem = "tea";
        ShowDetail("花茶\n价格:15 金币\n效果:战斗中按 J 使用，恢复 20 点 SP。");
    }

    public void SelectWaterLadle()
    {
        selectedItem = "waterLadle";
        ShowDetail("【质变】高级证书\n价格:60 金币\n说明:象征熟练搓澡工的道具，目前主要用于展示。");
    }

    public void SelectTowel()
    {
        selectedItem = "towel";
        ShowDetail("【终极】黄金搓澡巾\n价格:100 金币\n说明:传说中的高级搓澡巾，最终战前可以购买作为通关纪念。");
    }

    public void ConfirmBuy()
    {
        if (selectedItem == "")
        {
            RefreshUI("请先选择一个商品。");
            return;
        }

        GameManager gameManager = GetGameManager("ConfirmBuy");
        if (gameManager == null)
        {
            ShowShopDataUnavailableMessage();
            return;
        }

        if (selectedItem == "soap")
        {
            if (TryBuy(soapPrice, () => gameManager.soapCount++, "买到了肥皂！"))
                ShowPurchaseFeedback(soapIcon, "你购买到了肥皂 ×1");
        }
        else if (selectedItem == "tea")
        {
            if (TryBuy(teaPrice, () => gameManager.teaCount++, "买到了花茶！"))
                ShowPurchaseFeedback(teaIcon, "你购买到了花茶 ×1");
        }
        else if (selectedItem == "waterLadle")
        {
            if (gameManager.hasWaterLadle)
            {
                RefreshUI("已经拥有这个道具了。");
                RefreshPriceTexts();
                return;
            }

            if (TryBuy(waterLadlePrice, () => gameManager.hasWaterLadle = true, "获得【质变】高级证书！"))
                ShowPurchaseFeedback(waterLadleIcon, "你购买到了水瓢");
        }
        else if (selectedItem == "towel")
        {
            if (gameManager.hasGoldenTowel)
            {
                RefreshUI("已经拥有这个道具了。");
                RefreshPriceTexts();
                return;
            }

            if (TryBuy(towelPrice, () => gameManager.hasGoldenTowel = true, "获得【终极】黄金搓澡巾！"))
                ShowPurchaseFeedback(towelIcon, "你购买到了黄金搓澡巾");
        }
    }

    public void CancelSelection()
    {
        selectedItem = "";
        ShowDetail("请选择一个商品。");
        RefreshUI("已取消选择。");
        RefreshPriceTexts();
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

    private bool TryBuy(int price, System.Action onSuccess, string successMessage)
    {
        GameManager gameManager = GetGameManager("TryBuy");
        if (gameManager == null)
        {
            ShowShopDataUnavailableMessage();
            return false;
        }

        if (gameManager.playerGold >= price)
        {
            gameManager.playerGold -= price;
            onSuccess.Invoke();
            RefreshUI(successMessage);
            RefreshPriceTexts();
            Debug.Log("Shop purchase success. item=" + selectedItem + ", price=" + price + ", gold=" + gameManager.playerGold + ", soapCount=" + gameManager.soapCount + ", teaCount=" + gameManager.teaCount + ".");
            return true;
        }

        RefreshUI("金币不足");
        RefreshPriceTexts();
        Debug.Log("Shop purchase failed. item=" + selectedItem + ", price=" + price + ", gold=" + gameManager.playerGold + ".");
        return false;
    }

    private void RefreshUI(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (goldText == null)
            return;

        GameManager gameManager = GetGameManager("RefreshUI");
        if (gameManager != null)
            goldText.text = "金币：" + gameManager.playerGold;
    }

    private void ShowDetail(string text)
    {
        if (detailText != null)
            detailText.text = text;
    }

    private void ConfigureRichText()
    {
        if (messageText != null)
            messageText.richText = true;

        if (detailText != null)
            detailText.richText = true;
    }

    private GameManager GetGameManager(string context)
    {
        if (GameManager.Instance != null)
            return GameManager.Instance;

        if (!warnedMissingGameManager)
        {
            warnedMissingGameManager = true;
            Debug.LogWarning("ShopManager " + context + " skipped because GameManager.Instance is missing.");
        }

        return null;
    }

    private void ShowShopDataUnavailableMessage()
    {
        if (messageText != null)
            messageText.text = "商店数据暂时不可用。";
    }

    private void ResolveOptionalReferences()
    {
        if (shopPanel == null)
            return;

        if (purchaseFeedback == null)
            purchaseFeedback = shopPanel.GetComponentInChildren<PurchaseFeedbackPanelController>(true);

        Transform shopRoot = shopPanel.transform;
        ResolveItemIcons(shopRoot);
        soapPriceText = ResolvePriceText(soapPriceText, shopRoot, "Soap");
        teaPriceText = ResolvePriceText(teaPriceText, shopRoot, "Tea");
        waterLadlePriceText = ResolvePriceText(waterLadlePriceText, shopRoot, "Spoon");
        towelPriceText = ResolvePriceText(towelPriceText, shopRoot, "Towel");

        CapturePriceDefaultColors();
    }

    private void ResolveItemIcons(Transform shopRoot)
    {
        if (soapIcon == null)
            soapIcon = ResolveItemIcon(shopRoot, "Soap");

        if (teaIcon == null)
            teaIcon = ResolveItemIcon(shopRoot, "Tea");

        if (waterLadleIcon == null)
            waterLadleIcon = ResolveItemIcon(shopRoot, "Spoon");

        if (towelIcon == null)
            towelIcon = ResolveItemIcon(shopRoot, "Towel");
    }

    private Sprite ResolveItemIcon(Transform shopRoot, string itemObjectName)
    {
        Transform itemTransform = FindChildRecursive(shopRoot, itemObjectName);
        if (itemTransform == null)
            return null;

        Image image = itemTransform.GetComponent<Image>();
        return image != null ? image.sprite : null;
    }

    private TextMeshProUGUI ResolvePriceText(TextMeshProUGUI current, Transform shopRoot, string itemObjectName)
    {
        if (current != null)
            return current;

        Transform itemTransform = FindChildRecursive(shopRoot, itemObjectName);
        if (itemTransform == null)
            return null;

        return itemTransform.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null)
            return null;

        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform match = FindChildRecursive(child, childName);
            if (match != null)
                return match;
        }

        return null;
    }

    private void CapturePriceDefaultColors()
    {
        if (priceDefaultColorsCaptured)
            return;

        if (soapPriceText != null)
            soapPriceDefaultColor = soapPriceText.color;

        if (teaPriceText != null)
            teaPriceDefaultColor = teaPriceText.color;

        if (waterLadlePriceText != null)
            waterLadlePriceDefaultColor = waterLadlePriceText.color;

        if (towelPriceText != null)
            towelPriceDefaultColor = towelPriceText.color;

        priceDefaultColorsCaptured = true;
    }

    private void RefreshPriceTexts()
    {
        ResolveOptionalReferences();

        GameManager gameManager = GetGameManager("RefreshPriceTexts");
        if (gameManager == null)
            return;

        int gold = gameManager.playerGold;
        RefreshPriceText(soapPriceText, soapPrice, soapPriceDefaultColor, gold);
        RefreshPriceText(teaPriceText, teaPrice, teaPriceDefaultColor, gold);
        RefreshPriceText(waterLadlePriceText, waterLadlePrice, waterLadlePriceDefaultColor, gold);
        RefreshPriceText(towelPriceText, towelPrice, towelPriceDefaultColor, gold);
    }

    private void RefreshPriceText(TextMeshProUGUI priceText, int price, Color defaultColor, int gold)
    {
        if (priceText == null)
            return;

        priceText.text = price.ToString();
        priceText.color = gold >= price ? defaultColor : unaffordablePriceColor;
    }

    private void ShowPurchaseFeedback(Sprite icon, string message)
    {
        if (purchaseFeedback == null)
        {
            Debug.LogWarning("ShopManager purchaseFeedback is not assigned. Purchase feedback will not be shown.");
            return;
        }

        purchaseFeedback.Show(icon, message);
    }

    private void PausePlayer(bool shouldPause)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = !shouldPause;
    }
}
