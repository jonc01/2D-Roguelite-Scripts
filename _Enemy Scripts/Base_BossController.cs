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
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float distToPlayer;
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
        if (!combat.isAlive || combat.isSpawning) return;

        // if (combat.isStunned) return;

        #if UNITY_EDITOR
        if(DEBUGGING) DebugRaycast();
        #endif

        ChasePlayer();
        PlayerToRightCheck();

        // if (!combat.canAttack) return;
        // LedgeWallCheck();
        AttackCheck();
    }

    protected void PlayerToRightCheck()
    {
        //Only update if the player is actively being detected
        combat.playerToRight = playerDetect.playerToRight;
    }

    protected float PlayerDistCheck()
    {
        distToPlayer = Mathf.Abs(playerDetect.player.position.x - transform.position.x);
        return distToPlayer;
    }

    protected virtual void AttackCheck()
    {
        combat.Attack();

        //These variables determine lunge distance and direction during attacks
        combat.playerInFront = playerDetect.playerDetectFront;
        combat.attackClose = playerDetect.PlayerDistTooClose();
        combat.attackMain = playerDetect.PlayerDistMain();
        combat.backToWall = playerDetect.wallDetectBack; //Wall detect only checks behind boss
        combat.faceToWall = playerDetect.wallDetectFront;

        // if(!combat.isAttacking) return;
        combat.distanceToPlayer = PlayerDistCheck();
    }

    protected virtual void ChasePlayer()
    {
        if (!movement.canMove) return;
        // if (raycast.wallDetect || !raycast.ledgeDetect) return;
        //May not be needed with platform check
        if(PlayerDistCheck() >= playerDetectRange)
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
