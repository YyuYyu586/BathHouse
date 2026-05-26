using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyUIController : MonoBehaviour
{
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI coinText;
    public float refreshInterval = 0.2f;

    private float refreshTimer;
    private bool warnedMissingGameManager;
    private bool warnedMissingDayText;
    private bool warnedMissingCoinText;

    private void Awake()
    {
        AutoBindTextReferences();
        DisableGraphicRaycasts();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        refreshTimer += Time.deltaTime;
        if (refreshTimer < refreshInterval)
            return;

        refreshTimer = 0f;
        Refresh();
    }

    public void Refresh()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            if (!warnedMissingGameManager)
            {
                warnedMissingGameManager = true;
                Debug.LogWarning("DailyUIController could not find GameManager.Instance.");
            }

            return;
        }

        if (dayText != null)
        {
            dayText.text = "DAY " + gameManager.currentDay;
        }
        else if (!warnedMissingDayText)
        {
            warnedMissingDayText = true;
            Debug.LogWarning("DailyUIController dayText is not assigned on " + gameObject.name + ".");
        }

        if (coinText != null)
        {
            coinText.text = "\u91D1\u5E01\uFF1A" + gameManager.playerGold;
        }
        else if (!warnedMissingCoinText)
        {
            warnedMissingCoinText = true;
            Debug.LogWarning("DailyUIController coinText is not assigned on " + gameObject.name + ".");
        }
    }

    private void AutoBindTextReferences()
    {
        if (dayText == null)
            dayText = FindTextByName("DayText", "CurrentDayText", "Day");

        if (coinText == null)
            coinText = FindTextByName("CoinText", "GoldText", "MoneyText", "Coin");
    }

    private TextMeshProUGUI FindTextByName(params string[] names)
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < names.Length; i++)
        {
            for (int j = 0; j < texts.Length; j++)
            {
                if (texts[j] != null && texts[j].gameObject.name == names[i])
                    return texts[j];
            }
        }

        return null;
    }

    private void DisableGraphicRaycasts()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] != null)
                graphics[i].raycastTarget = false;
        }
    }
}
