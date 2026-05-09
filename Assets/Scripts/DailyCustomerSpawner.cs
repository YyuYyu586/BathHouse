using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyCustomerSpawner : MonoBehaviour
{
    [Header("把7天的顾客按顺序拖进这个数组")]
    public GameObject[] customers; // 在外面设置大小为 7

    void Start()
    {
        // 1. 游戏一开始，先把所有顾客都隐藏掉
        foreach (GameObject c in customers)
        {
            if (c != null) c.SetActive(false);
        }

        // 2. 获取今天是第几天 
        // （如果你 GameManager 还没挂好，可以先在这里写死 int today = 1; 来测试）
        int today = 1;
        if (GameManager.Instance != null)
        {
            today = GameManager.Instance.currentDay;
        }

        // 3. 只显示当天的那个顾客 (数组下标从0开始，所以是 today - 1)
        if (today >= 1 && today <= customers.Length)
        {
            customers[today - 1].SetActive(true);
        }
    }
}