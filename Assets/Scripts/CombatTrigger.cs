using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatTrigger : MonoBehaviour
{
    private bool isPlayerInRange;

    void Update()
    {
        // 踩在地毯上按 F 进入战斗
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // 真正去干活！加载战斗场景
            SceneManager.LoadScene("CombatScene");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("按 F 键进入搓澡间开始战斗！");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}