using UnityEngine;
using UnityEngine.SceneManagement;

// Opens the combat scene when the player presses F inside this trigger.
public class CombatTrigger : MonoBehaviour
{
    [SerializeField] private bool blockDayOneCombat = true;
    [SerializeField] private string combatSceneName = "CombatScene";

    private bool playerInRange;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            GameManager gameManager = GameManager.EnsureInstance();

            if (blockDayOneCombat && gameManager.currentDay <= 1)
            {
                Debug.Log("Day 1 is story only. Advancing to Day 2 combat for the demo loop.");
                gameManager.AdvanceDay();
            }

            SceneManager.LoadScene(combatSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
