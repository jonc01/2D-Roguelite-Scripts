using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee_Rush : Base_CombatBehavior
{
    //Rush towards the player 
    [Header("= Melee Rush =")]
    [SerializeField] float rushDuration = 2;
    [SerializeField] float base_moveSpeed;
    [SerializeField] float rushMoveSpeed = 2;

    protected override void Start()
    {
        base.Start();
        base_moveSpeed = movement.moveSpeed;
    }

    public override void Attack()
    {
        if(combat.isAttacking) return;

        StartCoroutine(RushCO());
    }

    IEnumerator RushCO()
    {
        Debug.Log("1 - Speeding up moveSpeed");
        combat.isAttacking = true;
        // movement.moveSpeed = rushMoveSpeed;

        yield return new WaitForSeconds(rushDuration);
        combat.GetKnockback(raycast.playerDetectedToRight, 8);

        Debug.Log("2 - Slowing down to base");
        combat.isAttacking = false;
        // movement.moveSpeed = base_moveSpeed;
    }
}
