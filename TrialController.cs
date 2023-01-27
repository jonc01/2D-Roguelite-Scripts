using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialController : MonoBehaviour
{
    [SerializeField] GameObject interactPrompt;
    [SerializeField] GameObject inputPrompt;
    EnemyStageManager stageManager;
    private bool canTakeInput;
    private bool trialStarted;

    void Start()
    {
        stageManager = GetComponentInParent<EnemyStageManager>();
        ToggleText(false);
        canTakeInput = false;
        trialStarted = false;
    }

    void Update()
    {
        if(!canTakeInput || trialStarted) return;

        if(Input.GetButtonDown("Interact")) //if(Input.GetKeyDown(KeyCode.E))
        {
            stageManager.StartTrial();
            ToggleText(false);
            trialStarted = true;

            stageManager.roomManager.DoorManager.OpenAllDoors(false);
        }
    }

    private void ToggleText(bool toggle)
    {
        interactPrompt.SetActive(toggle);
        inputPrompt.SetActive(toggle); //Change text to match Player's keybind if rebound
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(!collider.CompareTag("Player") || trialStarted) return;
        ToggleText(true);
        canTakeInput = true;
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if(!collider.CompareTag("Player") || trialStarted) return;
        ToggleText(false);
        canTakeInput = false;
    }
}
