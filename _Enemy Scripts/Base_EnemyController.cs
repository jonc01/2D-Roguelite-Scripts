using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_EnemyController : MonoBehaviour
{
    [Header("=== References/Setup ===")]
    public Base_EnemyMovement movement;
    public Base_EnemyCombat combat;
    public bool isRangedAttack = false;
    [SerializeField] protected float CODurationLower = .2f, CODurationUpper = .8f;
    [SerializeField] protected Transform playerTransform;

    [Header("=== Raycasts Reference ===")]
    [SerializeField] public Base_EnemyRaycast raycast;
    [SerializeField] protected int currPlayerPlatform;

    [Header("=== Debug Variables ===")]
    [SerializeField] protected bool isIdling;
    [SerializeField] protected bool isPatrolling;
    [SerializeField] protected bool playerDetected;
    [SerializeField] protected bool checkLanding;
    Coroutine IdleCO;
    Coroutine PatrolCO;
    Coroutine LandingCO;

    protected virtual void Awake()
    {
        if(movement == null) movement = GetComponent<Base_EnemyMovement>();
        if(combat == null) combat = GetComponent<Base_EnemyCombat>();
        if(raycast == null) raycast = GetComponentInChildren<Base_EnemyRaycast>();
    }

    protected virtual void Start()
    {
        playerTransform = GameManager.Instance.playerTransform;

        bool startDir = (Random.value > 0.5f);
        // movement.MoveRight(startDir);
        StartIdle(.3f, false);
        checkLanding = false;
    }

    protected virtual void Update()
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

        AttackCheckClose();
        AttackCheckFar();
        // PlayerToRightCheck();
    }

    protected virtual void FixedUpdate()
    {
        playerDetected = raycast.aggroed;
        PlatformCheck();
    }

    // protected void PlayerToRightCheck()
    // {
    //     combat.playerToRight = playerTransform.position.x > transform.position.x;

    //     //Only update if the player is actively being detected
    //     // if(raycast.playerDetectFront || raycast.playerDetectBack)
    //     //     combat.playerToRight = raycast.playerToRight;
    // }

    protected virtual void AttackCheckClose()
    {
        //Only attack the player if they're on the same platform
        if (!PlatformCheck() || combat.altAttacking) return;
        if (raycast.playerInRangeClose) combat.AttackClose();
    }

    protected virtual void AttackCheckFar()
    {
        if (combat.altAttacking) return;
        //Ranged enemies can attack the player on separate platforms if in range
        if (!PlatformCheck() && !isRangedAttack) return;
        if (raycast.playerInRangeFar && combat.CanAttackFar()) combat.AttackFar();
    }

    protected virtual bool PlatformCheck()
    {
        //Updates current player platform, compares to enemy's platform
        currPlayerPlatform = GameManager.Instance.PlayerCurrPlatform;
        if (currPlayerPlatform == raycast.currPlatform) return true;
        else return false;
    }

    protected virtual void ChasePlayer()
    {
        if (!isRangedAttack)
        {
            if (!combat.chasePlayer) return;
            if (!movement.canMove) return;
            if (raycast.currPlatform != currPlayerPlatform) return;
            if (raycast.wallDetect || !raycast.ledgeDetect) return; //May not be needed with platform check
        }
        
        // if (raycast.playerDetectFront || raycast.playerDetectBack) //-
        if (playerDetected)
        {
            // combat.instantiateManager.TextPopups.ShowIndicator(combat.hitEffectsOffset.position);

            StopPatrolling();

            if (isIdling)
            {
                StopIdling();
                movement.canMove = true;
            }
            // movement.MoveRight(raycast.playerDetectedToRight);
            movement.MoveRight(raycast.playerToRight);
        }
    }

    protected void MoveCheck()
    {
        // if(combat.isAttacking || combat.altAttacking) return;
        if(combat.isAttacking || playerDetected) return;

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
                bool switchDir = Random.value > .5f;
                bool idleSwitch = Random.value > .5f;
                float coDuration = Random.Range(CODurationLower, CODurationUpper);

                if (idleSwitch) StartPatrol(coDuration, switchDir);
                else 
                {
                    StartIdle(coDuration, switchDir);
                }
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

    protected IEnumerator Idling(float duration, bool switchDir, bool knockbackHitB)
    {
        isIdling = true;
        movement.canMove = false;
        // movement.DisableMove();
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
        if(!isPatrolling) return;
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

    protected void FlipDir()
    {
        bool dir = !movement.isFacingRight;
        movement.MoveRight(dir);
    }

    protected void LedgeWallCheck()
    {
        if (combat.isAttacking)
        {
            if (!raycast.isGrounded) //TODO: testing, no issues yet
                if(movement.rb.velocity.y != 0) combat.StopAttack();
            return;
        } 
        if (!raycast.isGrounded) return;
        if (!raycast.ledgeDetect || raycast.wallDetect)
            if (movement.rb.velocity.y == 0)
                FlipDir();
    }

    protected void StartLanding()
    {
        if(!checkLanding) return;
        if(LandingCO != null) StopCoroutine(LandingCO);

        //Disable canMove right after landing
        checkLanding = false;
        LandingCO = StartCoroutine(Landing());
    }

    IEnumerator Landing()
    {
        movement.canMove = false;
        yield return new WaitForSeconds(.1f);
        movement.canMove = true;
    }
}
