using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [SerializeField] GameObject interactPrompt;
    [SerializeField] GameObject inputPrompt;
    [SerializeField] GameObject shopWindow;
    private bool canTakeInput;

    void Start()
    {
        ToggleText(false);
        canTakeInput = false;
        OpenShop(false);
    }

    void Update()
    {
        if(!canTakeInput) return;
        //Checks if game is already paused, or input is disabled
        if(!GameManager.Instance.inputAllowed) return; 

        if(Input.GetButtonDown("Interact"))
        {
            OpenShop();
            ToggleText(false);
        }
        
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OpenShop(false);
            ToggleText(true);
        }
    }

    private void OpenShop(bool open = true)
    {
        if(shopWindow == null) return;
        shopWindow.SetActive(open);
    }

    private void ToggleText(bool toggle)
    {
        interactPrompt.SetActive(toggle);
        inputPrompt.SetActive(toggle); //Change text to match Player's keybind if rebound
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
}
