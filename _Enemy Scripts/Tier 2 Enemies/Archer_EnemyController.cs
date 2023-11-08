using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
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
        // return; //TODO: TESTING //=========1
        if(combat.altAttacking) return; //Check if attack is already started, this prevents Manualflip being called during attack coroutine
        // if (!PlatformCheck() || isRangedAttack) return; //========1
        if (!PlatformCheck() && !isRangedAttack) return;

        if (raycast.playerInRangeClose)
        {
            PlayerDistCheck();
            // combat.altAttacking = true;
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
            // PlayerDistCheck(); //------------------2 , trying to add, should dash from player 
            // combat.altAttacking = true;
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
        combat.ToggleDamageImmune(true);
        yield return new WaitForSeconds(0.1f);
        combat.animator.PlayManualAnim(1, 0.75f); //Vanish
        yield return new WaitForSeconds(0.2f);
        movement.canMove = false;

        //Lunge then flip towards the Player and start Attack
        if(PlayerDistCheck() <= raycast.attackRangeClose) LungeCheck(9);
        else LungeCheck(0);

        yield return new WaitForSeconds(0.55f);
        combat.ToggleDamageImmune(false);
        combat.ToggleHealthbar(true);
        // yield return new WaitForSeconds(.35f);
        
        ManualFlip(raycast.playerToRight);
        yield return new WaitForSeconds(0.05f);
        
        combat.isAttacking = false;
        combat.AttackFar();
    }

    public void LungeCheck(float lungeStrength = 4f, float duration = .3f)
    {
        // movement.ToggleFlip(false);
        
        if(raycast.playerInRangeClose) //Player too close
        {
            if(raycast.backToWall || !raycast.backToLedge)
            {
                //Lunge forwards
                combat.Lunge(!raycast.playerDetectedToRight, lungeStrength, duration);
            }
            else
            {
                //Lunge backwards
                combat.Lunge(raycast.playerDetectedToRight, lungeStrength, duration);
            }
        }else
        {
            combat.Lunge(!raycast.playerDetectedToRight, lungeStrength, duration);
        }
    }

    void ManualFlip(bool faceRight)
    {
        movement.ManualFlip(faceRight);
        movement.ToggleFlip(false);
    }
}
