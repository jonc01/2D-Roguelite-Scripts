using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger_EnemyController : Base_EnemyController
{
    // [Space(20)]
    // [Header("= Dagger =")]
    // [SerializeField] float lungeDist = .2f;

    protected override void AttackCheckClose()
    {
        if(combat.altAttacking) return; //Check if attack is already started, this prevents Manualflip being called during attack coroutine
        if (!PlatformCheck()) return;

    
        if (raycast.playerInRangeClose)
        {
            combat.AttackClose();
        }
    }

    protected override void AttackCheckFar()
    {
        if(combat.altAttacking) return;
        if(!combat.CanAttackFar()) return; //Check if a far attack behavior is null
        //Player isn't on the same platform, and enemy doesn't have range
        if (!PlatformCheck() && !isRangedAttack) return;
    }
    
}
