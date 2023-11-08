using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSelect : MonoBehaviour
{
    [Header("Default button for Keyboard navigation")]
    public bool enable = false;
    public Button primaryButton;
    public Button backButton;

    void Start()
    {
        if(!enable) return;
        primaryButton.Select();
    }

    private void OnEnable()
    {
        if(!enable) return;
        primaryButton.Select(); //workaround with multiple menus being toggled
    }

    private void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            if(backButton == null) return;
            //Escape input calls Back button input 
            backButton.onClick.Invoke();
        }
    }
}
