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

    public int waterLadleHeal = 35;
    public int teaRecoverSP = 25;

    [Header("Enemy Stats")]
    public string enemyName = "泥巴怪";
    public int maxEnemyHP = 80;
    public int enemyAttackDamage = 10;
    public int winGold = 20;

    [Header("Flow")]
    public string returnSceneName = "BathhouseMain";
    public bool advanceDayOnWin = true;
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
        battleEnded = false;

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
            RefreshUI("SP不够，放不出肥皂泡泡！");
            return;
        }

        StartCoroutine(PlayerAction(soapSkillDamage, soapSkillCost, "小福使用肥皂泡泡重击！"));
    }

    public void OnUseWaterLadleButton()
    {
        if (!CanPlayerAct()) return;

        if (GameManager.Instance == null || GameManager.Instance.waterLadleCount <= 0)
        {
            RefreshUI("没有水瓢了！");
            return;
        }

        GameManager.Instance.waterLadleCount--;
        playerHP = Mathf.Min(maxPlayerHP, playerHP + waterLadleHeal);
        StartCoroutine(EnemyTurnAfterMessage("小福使用水瓢，回复了 HP！"));
    }

    public void OnUseTeaButton()
    {
        if (!CanPlayerAct()) return;

        if (GameManager.Instance == null || GameManager.Instance.teaCount <= 0)
        {
            RefreshUI("没有茶了！");
            return;
        }

        GameManager.Instance.teaCount--;
        playerSP = Mathf.Min(maxPlayerSP, playerSP + teaRecoverSP);
        StartCoroutine(EnemyTurnAfterMessage("小福喝下热茶，回复了 SP！"));
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

            if (advanceDayOnWin)
            {
                GameManager.Instance.GoNextDay();
            }
        }

        RefreshUI("净化成功！获得 " + winGold + " 金币。点击返回。");

        if (commandPanel != null) commandPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    private void LoseBattle()
    {
        battleEnded = true;
        SavePlayerStats();

        RefreshUI("小福累倒了……点击返回重新准备。");

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
        if (playerHPText != null)
            playerHPText.text = "HP：" + playerHP + " / " + maxPlayerHP;

        if (playerSPText != null)
            playerSPText.text = "SP：" + playerSP + " / " + maxPlayerSP;

        if (enemyHPText != null)
            enemyHPText.text = enemyName + "：" + enemyHP + " / " + maxEnemyHP;

        if (battleMessageText != null)
            battleMessageText.text = message;

        if (GameManager.Instance != null && itemCountText != null)
        {
            itemCountText.text =
                "水瓢 x" + GameManager.Instance.waterLadleCount +
                "  茶 x" + GameManager.Instance.teaCount;
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