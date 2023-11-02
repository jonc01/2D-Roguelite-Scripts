using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo_ResetRun : MonoBehaviour
{
    [Header("- References -")]
    [SerializeField] GameObject inputPrompt;
    [SerializeField] GameObject interactPrompt;

    [Space(10)]
    [Header("- Debug -")]
    [SerializeField] private bool canTakeInput;
    //TEMP - Move to script to send Player to next Tileset
    [SerializeField] private Animator portalAnim;
    [SerializeField] string animName;

    void Start()
    {
        canTakeInput = false;
        ToggleText(false);

        portalAnim.Play(animName);
        portalAnim.SetTrigger("StartPortal");
    }

    void Update()
    {
        if(!canTakeInput) return;
        //Checks if game is already paused, or input is disabled
        if(!GameManager.Instance.inputAllowed) return;

        if(Input.GetButtonDown("Interact"))
        {
            AsyncLevelLoader.asyncLevelLoader.ResetRun();
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(!collider.CompareTag("Player")) return;

        ToggleText(true);
        canTakeInput = true;
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if(!collider.CompareTag("Player")) return;
        ToggleText(false);
        canTakeInput = false;
    }

    private void ToggleText(bool toggle)
    {
        inputPrompt.SetActive(toggle); //Change text to match Player's keybind if rebound
        interactPrompt.SetActive(toggle);
    }
}
