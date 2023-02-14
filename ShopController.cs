using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [SerializeField] GameObject interactPrompt;
    [SerializeField] GameObject inputPrompt;
    private bool canTakeInput;

    void Start()
    {
        ToggleText(false);
        canTakeInput = false;
    }

    void Update()
    {
        if(!canTakeInput) return;

        if(Input.GetButtonDown("Interact"))
        {
            //shop.OpenShop(); //TODO: add after Shops are added
            Debug.Log("Test Shop open");
            ToggleText(false);
        }
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
