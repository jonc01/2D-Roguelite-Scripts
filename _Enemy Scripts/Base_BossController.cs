using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_BossController : MonoBehaviour
{
    [Header("=== References/Setup ===")]
    [SerializeField] bool DEBUGGING = false;
    public Base_BossMovement movement;
    public Base_BossCombat combat;

    [Header("=== Player Transform and Raycast Reference ===")]
    [SerializeField] protected float playerDetectRange = .5f;
    [SerializeField] public Base_EnemyPlayerDetect playerDetect;

    protected virtual void Awake()
    {
        if(movement == null) GetComponent<Base_BossMovement>();
        if(combat == null) GetComponent<Base_BossCombat>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        if (!combat.isAlive) return;

        if (combat.isStunned)// || combat.isKnockedback)
        {
            // StopPatrolling();
            return;
        }

        // if (!raycast.isGrounded || combat.isSpawning) return;
        if (combat.isSpawning) return;

        #if UNITY_EDITOR
        if(DEBUGGING) DebugRaycast();
        #endif

        ChasePlayer();
        if (combat.isAttacking) return;

        // LedgeWallCheck();
        AttackCheck();
        PlayerToRightCheck();
    }

    protected void PlayerToRightCheck()
    {
        //Only update if the player is actively being detected
        combat.playerToRight = playerDetect.playerToRight;
    }

    protected virtual void AttackCheck()
    {
        combat.playerInFront = playerDetect.playerDetectFront;
        if(!playerDetect.playerDetectFront)
        {
            combat.backToWall = true;
        }
        else
        {
            combat.backToWall = playerDetect.wallDetect;
            if(!playerDetect.wallDetect)
            {
                //Player too close, Use Melee or dash backwards before attacking
                combat.attackClose = playerDetect.PlayerDistTooClose();
                combat.attackMain = playerDetect.PlayerDistMain();
            }
        }
        combat.Attack();
    }

    protected virtual void ChasePlayer()
    {
        if (!movement.canMove) return;
        // if (raycast.wallDetect || !raycast.ledgeDetect) return;
        //May not be needed with platform check
        if(Mathf.Abs(playerDetect.player.position.x - transform.position.x) >= playerDetectRange)
            movement.MoveRight(playerDetect.playerToRight);
    }

    protected virtual void FlipDir()
    {
        bool dir = !movement.isFacingRight;
        movement.MoveRight(dir);
    }

    protected void LedgeWallCheck()
    {
        // if (combat.isAttacking) return;
        
        //TODO: instead of flipping, can use for jump/moving logic
        // if (!playerDetect.ledgeDetect || playerDetect.wallDetect)
        //     if (movement.rb.velocity.y == 0)
        //         FlipDir();
    }

    void DebugRaycast()
    {
        Vector3 playerCheckRange = transform.TransformDirection(Vector3.right) * playerDetectRange;
        Debug.DrawRay(transform.position, playerCheckRange, Color.cyan);
    }
}
