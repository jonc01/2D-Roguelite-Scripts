using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEBUGGING_MODE : MonoBehaviour
{
    
    [SerializeField] Base_PlayerCombat playerCombat;


    void Start()
    {

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote)) //tilde key
        {
            if(playerCombat == null) return;
            playerCombat.maxHP = 9999;
            playerCombat.HealPlayer(9999);
        }
    }
}
