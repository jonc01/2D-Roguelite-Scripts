using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger_EnemyController : Base_EnemyController
{
    [Space(20)]
    [Header("= Dagger =")]
    [SerializeField] float lungeDist = .2f;

    protected override void AttackCheckClose()
    {
        if(combat.altAttacking) return; //Check if attack is already started, this prevents Manualflip being called during attack coroutine
        // if (!PlatformCheck() || isRangedAttack) return;
        if (!PlatformCheck()) return;

        // if (raycast.playerInRangeClose)
        // {
        //     combat.altAttacking = true;
        //     // StartCoroutine(LungeAttack());
        // }
        // else
        // {
        //     combat.altAttacking = true;
        //     combat.AttackFar();
        // }

        if (raycast.playerInRangeClose)
        {
            combat.altAttacking = true;
            Debug.Log("player close");
            combat.AttackClose();
        }
    }

    
}
