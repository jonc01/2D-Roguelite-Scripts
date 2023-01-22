using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_EnemyController : MonoBehaviour
{
    [Header("=== References/Setup ===")]
    public Base_EnemyMovement movement;
    public Base_EnemyCombat combat;
    public bool isRangedAttack = false;
    [SerializeField] private float CODurationLower = .2f, CODurationUpper = .8f;
    

    [Header("=== Raycasts Reference ===")]
    [SerializeField] public Base_EnemyRaycast raycast;
    [SerializeField] int currPlayerPlatform;

    [Header("=== Debug Variables ===")]
    [SerializeField] bool isIdling;
    [SerializeField] bool isPatrolling;
    [SerializeField] bool playerDetected;

    Coroutine IdleCO;
    Coroutine PatrolCO;

    private void Awake()
    {
        if(movement == null) movement = GetComponent<Base_EnemyMovement>();
        if(combat == null) combat = GetComponent<Base_EnemyCombat>();
    }

    private void Start()
    {
        bool startDir = (Random.value > 0.5f);
        movement.MoveRight(startDir);
    }

    private void Update()
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

        if (!raycast.isGrounded) return;

        MoveCheck();
        LedgeWallCheck();
        ChasePlayer();
        AttackCheckClose();
        AttackCheckFar();
        PlayerToRightCheck();
    }

    private void FixedUpdate()
    {
        PlatformCheck();
    }

    void PlayerToRightCheck()
    {
        //Only update if the player is actively being detected
        if(raycast.playerDetectFront || raycast.playerDetectBack)
            combat.playerToRight = raycast.playerToRight;
    }

    void AttackCheckClose()
    {
        if (!PlatformCheck()) return;
        if (raycast.playerInRangeClose) combat.AttackClose();
    }

    void AttackCheckFar()
    {
        if (!PlatformCheck() && !isRangedAttack) return;
        if (raycast.playerInRangeFar && combat.CanAttackFar()) combat.AttackFar();
    }

    bool PlatformCheck()
    {
        //Updates current player platform, compares to enemy's platform
        currPlayerPlatform = GameManager.Instance.PlayerCurrPlatform;
        if (currPlayerPlatform == raycast.currPlatform) return true;
        else return false;
    }

    void ChasePlayer()
    {
        if (raycast.currPlatform != currPlayerPlatform) return;
        if (raycast.wallDetect || !raycast.ledgeDetect) return; //May not be needed with platform check

        if (raycast.playerDetectFront || raycast.playerDetectBack)
        {
            playerDetected = true;
            StopPatrolling();

            if (isIdling)
            {
                StopIdling();
                movement.canMove = true;
            }
            movement.MoveRight(raycast.playerToRight);
        }
        else playerDetected = false;
    }

    void MoveCheck()
    {
        if (playerDetected) return;

        //LedgeCheck raycast or wallcheck to turn around
        if (raycast.ledgeDetect) //&& movement.canMove)
        {
            //Logic for enemy to decide between Patrolling and Idling
            if (isPatrolling)
            {
                if (movement.isFacingRight) movement.MoveRight(true);
                else movement.MoveRight(false);
            }

            //Switch between Patrolling or Idling, and the duration to run the action
            if (!isPatrolling && !isIdling)
            {
                bool switchDir = (Random.value > .5f);
                bool idleSwitch = (Random.value > .5f);
                float coDuration = (Random.Range(CODurationLower, CODurationUpper));

                if (idleSwitch) StartPatrol(coDuration, switchDir);
                else StartIdle(coDuration, switchDir);
            }
        }
    }

    protected void StartIdle(float duration, bool switchDir, bool knockbackHitB = false)
    {
        if (!isIdling)
            IdleCO = StartCoroutine(Idling(duration, switchDir, knockbackHitB));
    }

    protected void StopIdling()
    {
        if (IdleCO != null)
            StopCoroutine(IdleCO);

        isIdling = false;
    }

    IEnumerator Idling(float duration, bool switchDir, bool knockbackHitB)
    {
        isIdling = true;
        movement.canMove = false;
        movement.DisableMove();
        if (switchDir)
            FlipDir();

        yield return new WaitForSeconds(duration);
        isIdling = false;
        movement.canMove = true;
    }

    protected void StartPatrol(float duration, bool switchDir)
    {
        if (!isPatrolling)
            PatrolCO = StartCoroutine(Patrolling(duration, switchDir));
    }

    protected void StopPatrolling()
    {
        if (PatrolCO != null)
            StopCoroutine(PatrolCO);

        isPatrolling = false;
    }

    IEnumerator Patrolling(float duration, bool switchDir)
    {
        isPatrolling = true;
        if (switchDir)
            FlipDir();

        yield return new WaitForSeconds(duration);
        isPatrolling = false;
    }

    void FlipDir()
    {
        bool dir = !movement.isFacingRight;
        movement.MoveRight(dir);
    }

    void LedgeWallCheck()
    {
        if (combat.isAttacking) return;
        if (!raycast.ledgeDetect || raycast.wallDetect)
            if (movement.rb.velocity.y == 0)
                FlipDir();
    }
}
