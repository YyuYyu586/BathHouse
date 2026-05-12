using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏全局数据")]
    public int currentDay = 1;
    public int playerGold = 20;
    public int playerHP = 100;
    public int playerSP = 50;

    [Header("商店道具")]
    public int waterLadleCount = 0;
    public int soapCount = 0;
    public int teaCount = 0;
    public int towelCount = 0;
    public bool hasWaterLadle = false;
    public bool hasGoldenTowel = false;

    private void Awake()
    {
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

    public void GoNextDay()
    {
        AdvanceDay();
    }

    public void AdvanceDay()
    {
        currentDay++;
        playerHP = 100;
        playerSP = 50;
    }
}
