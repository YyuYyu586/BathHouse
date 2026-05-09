using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏全局数据")]
    public int currentDay = 1;      // 当前天数 (1-7)
    public int playerGold = 20;     // 玩家金币
    public int playerHP = 100;      // 玩家当前HP
    public int playerSP = 50;       // 玩家当前SP

    [Header("背包/道具数据")]
    public int hpHamburgerCount = 0;     // 回血汉堡数量
    public int spDrinkCount = 0;    // 回蓝饮料数量
    public bool hasPassiveCert = false; // 是否买了 40% 回复证书
    public bool hasBossItem = false;    // 是否买了打Boss道具

    private void Awake()
    {
        // 经典的单例模式，保证切换场景数据不丢失
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}