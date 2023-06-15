using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuEventSystemSwap : MonoBehaviour
{
    //The start Selected button enabled Keyboard menu navigation
    [SerializeField] Button startSelectedButton;

    void OnEnable()
    {
        // eventSystem.firstSelectedGameObject = startSelectedButton;
        startSelectedButton.Select();
    }
}
