using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxPlayerHP = 100;
    public int maxPlayerSP = 50;
    public int attackDamage = 12;
    public int soapSkillDamage = 25;
    public int soapSkillCost = 10;
    public int hamburgerHeal = 35;
    public int drinkRecoverSP = 25;

    [Header("Enemy Stats")]
    public string enemyName = "泥巴怪";
    public int maxEnemyHP = 80;
    public int enemyAttackDamage = 10;
    public int winGold = 20;

    [Header("Flow")]
    public string returnSceneName = "BathhouseMain";
    public bool advanceDayOnWin = false;
    public float enemyTurnDelay = 0.8f;

    [Header("UI Text")]
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerSPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI battleMessageText;
    public TextMeshProUGUI itemCountText;

    [Header("UI Sliders Optional")]
    public Slider playerHPSlider;
    public Slider playerSPSlider;
    public Slider enemyHPSlider;

    [Header("Result Panels Optional")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject commandPanel;

    private int playerHP;
    private int playerSP;
    private int enemyHP;
    private bool battleEnded;
    private bool playerCanAct;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            playerHP = Mathf.Clamp(GameManager.Instance.playerHP, 1, maxPlayerHP);
            playerSP = Mathf.Clamp(GameManager.Instance.playerSP, 0, maxPlayerSP);
        }
        else
        {
            playerHP = maxPlayerHP;
            playerSP = maxPlayerSP;
        }

        enemyHP = maxEnemyHP;
        playerCanAct = true;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (commandPanel != null) commandPanel.SetActive(true);

        RefreshUI("一只" + enemyName + "出现了！");
    }

    public void OnAttackButton()
    {
        if (!CanPlayerAct()) return;
        StartCoroutine(PlayerAction(attackDamage, 0, "小福发动普通搓洗！"));
    }

    public void OnSoapSkillButton()
    {
        if (!CanPlayerAct()) return;

        if (playerSP < soapSkillCost)
        {
            RefreshUI("SP不够，放不出泡泡技能！");
            return;
        }

        StartCoroutine(PlayerAction(soapSkillDamage, soapSkillCost, "小福使用肥皂泡泡重击！"));
    }

    public void OnUseHamburgerButton()
    {
        if (!CanPlayerAct()) return;

        if (GameManager.Instance == null || GameManager.Instance.hpHamburgerCount <= 0)
        {
            RefreshUI("没有回血汉堡了！");
            return;
        }

        GameManager.Instance.hpHamburgerCount--;
        playerHP = Mathf.Min(maxPlayerHP, playerHP + hamburgerHeal);
        StartCoroutine(EnemyTurnAfterMessage("小福吃下汉堡，回复了HP！"));
    }

    public void OnUseDrinkButton()
    {
        if (!CanPlayerAct()) return;

        if (GameManager.Instance == null || GameManager.Instance.spDrinkCount <= 0)
        {
            RefreshUI("没有回蓝饮料了！");
            return;
        }

        GameManager.Instance.spDrinkCount--;
        playerSP = Mathf.Min(maxPlayerSP, playerSP + drinkRecoverSP);
        StartCoroutine(EnemyTurnAfterMessage("小福喝下饮料，回复了SP！"));
    }

    public void OnReturnButton()
    {
        SavePlayerStats();
        SceneManager.LoadScene(returnSceneName);
    }

    private IEnumerator PlayerAction(int damage, int spCost, string message)
    {
        playerCanAct = false;
        playerSP -= spCost;
        enemyHP = Mathf.Max(0, enemyHP - damage);
        RefreshUI(message + "造成 " + damage + " 点伤害！");

        yield return new WaitForSeconds(enemyTurnDelay);

        if (enemyHP <= 0)
        {
            WinBattle();
            yield break;
        }

        EnemyAttack();
    }

    private IEnumerator EnemyTurnAfterMessage(string message)
    {
        playerCanAct = false;
        RefreshUI(message);
        yield return new WaitForSeconds(enemyTurnDelay);
        EnemyAttack();
    }

    private void EnemyAttack()
    {
        if (battleEnded) return;

        playerHP = Mathf.Max(0, playerHP - enemyAttackDamage);
        RefreshUI(enemyName + "反击！小福受到 " + enemyAttackDamage + " 点伤害！");

        if (playerHP <= 0)
        {
            LoseBattle();
            return;
        }

        playerCanAct = true;
    }

    private bool CanPlayerAct()
    {
        return !battleEnded && playerCanAct;
    }

    private void WinBattle()
    {
        battleEnded = true;
        SavePlayerStats();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerGold += winGold;
            if (advanceDayOnWin) GameManager.Instance.currentDay++;
        }

        RefreshUI("净化成功！获得 " + winGold + " 金币。点击返回。 ");
        if (commandPanel != null) commandPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    private void LoseBattle()
    {
        battleEnded = true;
        SavePlayerStats();
        RefreshUI("小福累倒了……点击返回重新准备。 ");
        if (commandPanel != null) commandPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(true);
    }

    private void SavePlayerStats()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.playerHP = playerHP;
        GameManager.Instance.playerSP = playerSP;
    }

    private void RefreshUI(string message)
    {
        if (playerHPText != null) playerHPText.text = "HP：" + playerHP + " / " + maxPlayerHP;
        if (playerSPText != null) playerSPText.text = "SP：" + playerSP + " / " + maxPlayerSP;
        if (enemyHPText != null) enemyHPText.text = enemyName + "：" + enemyHP + " / " + maxEnemyHP;
        if (battleMessageText != null) battleMessageText.text = message;

        if (GameManager.Instance != null && itemCountText != null)
        {
            itemCountText.text = "汉堡 x" + GameManager.Instance.hpHamburgerCount + "  饮料 x" + GameManager.Instance.spDrinkCount;
        }

        if (playerHPSlider != null)
        {
            playerHPSlider.maxValue = maxPlayerHP;
            playerHPSlider.value = playerHP;
        }

        if (playerSPSlider != null)
        {
            playerSPSlider.maxValue = maxPlayerSP;
            playerSPSlider.value = playerSP;
        }

        if (enemyHPSlider != null)
        {
            enemyHPSlider.maxValue = maxEnemyHP;
            enemyHPSlider.value = enemyHP;
        }
    }
}
