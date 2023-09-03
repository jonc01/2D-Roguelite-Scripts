using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDrone_EnemyController : Base_EnemyController
{
    [Space(20)]
    [Header("- Bomb Drone Override -")]
    [SerializeField] protected float attackRange = .3f;
    [SerializeField] public float damage = 4;

    protected override void Update()
    {
        if (!combat.isAlive)
        {
            StopIdling();
            StopPatrolling();
            return;
        }

        if (combat.isStunned || combat.isKnockedback)
        {
            StopPatrolling();
            return;
        }

        // if (!raycast.isGrounded || combat.isSpawning) return;
        if (!raycast.isGrounded || combat.isSpawning) { 
            movement.canMove = false; 
            checkLanding = true; 
            return;
        }
        StartLanding();

        MoveCheck();
        LedgeWallCheck();
        ChasePlayer();
        // AttackCheckClose();
        // AttackCheckFar();
        AttackCheck();
        // PlayerToRightCheck();
    }
    
    protected void AttackCheck()
    {
        float dist = Vector3.Distance(playerTransform.position, transform.position);
        if(dist <= attackRange)
        {
            combat.AttackFar();
        }
    }
}
