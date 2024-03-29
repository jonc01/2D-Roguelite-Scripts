using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer_EnemyController : Base_EnemyController
{
    [Header("= Archer =")]
    [SerializeField] float lungeDist = .3f;
    public float distanceToPlayer;
    // public bool backToWall;
    // public bool attackClose;
    // public bool attackMain;
    Transform playerPos;
    [SerializeField] public bool lunging;
    
    protected override void Start()
    {
        base.Start();
        playerPos = GameManager.Instance.playerTransform;
        lunging = false;
    }

    protected override void AttackCheckClose()
    {
        if(combat.altAttacking) return; //Check if attack is already started, this prevents Manualflip being called during attack coroutine
        if (!PlatformCheck() && !isRangedAttack) return;

        if (raycast.playerInRangeClose)
        {
            PlayerDistCheck();
            StartCoroutine(LungeAttack());
        }
    }

    protected override void AttackCheckFar()
    {
        if(combat.altAttacking) return;
        //Player isn't on the same platform, and enemy doesn't have range
        if (!PlatformCheck() && !isRangedAttack) return;

        if (raycast.playerInRangeFar && combat.CanAttackFar())
        {
            PlayerDistCheck();
            StartCoroutine(LungeAttack());
        }
    }

    protected float PlayerDistCheck()
    {
        distanceToPlayer = Mathf.Abs(playerPos.position.x - transform.position.x);
        return distanceToPlayer;
    }

    IEnumerator LungeAttack()
    {
        if(combat.instantiateManager != null)
            combat.instantiateManager.TextPopups.ShowIndicator(combat.hitEffectsOffset.position);

        combat.altAttacking = true;
        combat.chasePlayer = false;
        combat.isAttacking = true;
        combat.ToggleHealthbar(false);

        yield return new WaitForSeconds(0.1f);

        bool playerInfront = raycast.playerDetectedToRight;
        if(!raycast.backToLedge) playerInfront = false;
        // playerDetected = false;
        
        combat.animator.PlayManualAnim(1, 0.75f); //Vanish
        combat.ToggleDamageImmune(true);

        yield return new WaitForSeconds(0.2f);
        combat.ToggleDamageImmune(false);
        movement.canMove = false;

        //Lunge then flip towards the Player and start Attack
        if(PlayerDistCheck() <= raycast.attackRangeClose) LungeCheck(playerInfront, 9);
        else LungeCheck(playerInfront, 0);

        yield return new WaitForSeconds(0.55f);
        combat.ToggleHealthbar(true);
        // yield return new WaitForSeconds(.35f);
        
        ManualFlip(raycast.playerToRight);
        yield return new WaitForSeconds(0.05f);
        
        combat.isAttacking = false;
        combat.AttackFar();
    }

    public void LungeCheck(bool playerInfront, float lungeStrength = 4f, float duration = .3f)
    {
        // movement.ToggleFlip(false);

        if(raycast.playerInRangeClose) //Player too close
        {
            if(raycast.backToWall || !raycast.backToLedge)
            {
                //Lunge forwards
                combat.Lunge(!playerInfront, lungeStrength, duration);
            }
            else
            {
                //Lunge backwards
                combat.Lunge(playerInfront, lungeStrength, duration);
            }
        }else
        {
            combat.Lunge(!playerInfront, lungeStrength, duration);
        }
    }

    void ManualFlip(bool faceRight)
    {
        movement.ManualFlip(faceRight);
        movement.ToggleFlip(false);
    }
}
