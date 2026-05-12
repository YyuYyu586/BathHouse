using UnityEngine;
using UnityEngine.SceneManagement;

// Opens the combat scene when the player presses F inside this trigger.
public class CombatTrigger : MonoBehaviour
{
    private bool playerInRange;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene("CombatScene");
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
