using Packages.Rider.Editor.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Melee : MonoBehaviour
{
    //Enemy Classes call this function for different melee behavior.
    [SerializeField] Base_EnemyCombat combat;

    private void Start()
    {
        if(combat == null)
            combat = GetComponent<Base_EnemyCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DashAttack() 
    {
        //Dash towards player dealing contact damage
        //combat.movement.Dash();
    }

    
}
