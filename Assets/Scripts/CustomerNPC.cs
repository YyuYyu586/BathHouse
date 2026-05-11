using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CustomerNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;

    public DialogueLine[] lines;

    private bool playerNear = false;

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.F))
        {
            dialogueManager.StartDialogue(lines);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}
