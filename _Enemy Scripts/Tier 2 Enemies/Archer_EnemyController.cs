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
        if(combat.altAttacking) return; //Check if attack is already started, this prevents Manualflip being called during attack coroutine
        if (!PlatformCheck() || isRangedAttack) return;
        if (raycast.playerInRangeClose)
        {
            PlayerDistCheck();
            combat.altAttacking = true;
            StartCoroutine(LungeAttack());
        }else
        {
            combat.altAttacking = true;
            combat.AttackFar();
        }
    }

    protected override void AttackCheckFar()
    {
        if(combat.altAttacking) return;
        if (!PlatformCheck() && !isRangedAttack) return;
        if (raycast.playerInRangeFar && combat.CanAttackFar())
        {
            combat.altAttacking = true;
            StartCoroutine(LungeAttack());
        }
        // base.AttackCheckFar();
    }

    // -- Reference -- DELETE WHEN DONE
    // protected virtual void AttackCheckClose11()
    // {
    //     //Only attack the player if they're on the same platform
    //     if (!PlatformCheck()) return;
    //     if (raycast.playerInRangeClose) combat.AttackClose();
    // }

    // protected virtual void AttackCheckFar11()
    // {
    //     //Ranged enemies can attack the player on separate platforms if in range
    //     if (!PlatformCheck() && !isRangedAttack) return;
    //     if (raycast.playerInRangeFar && combat.CanAttackFar()) combat.AttackFar();
    // }

    // ---------------------------------

    protected float PlayerDistCheck()
    {
        distanceToPlayer = Mathf.Abs(playerPos.position.x - transform.position.x);
        return distanceToPlayer;
    }

    IEnumerator LungeAttack()
    {
        //Lunge then flip towards the Player and start Attack
        LungeCheck();
        combat.animator.PlayManualAnim(1, 0.75f); //Vanish
        combat.ToggleHealthbar(false);
        yield return new WaitForSeconds(0.75f);
        combat.ToggleHealthbar(true);
        // yield return new WaitForSeconds(.35f);
        
        ManualFlip(raycast.playerToRight);
        yield return new WaitForSeconds(0.1f);
        // ManualFlip(raycast.playerDetectedToRight);
        combat.AttackFar();
    }

    public void LungeCheck(float lungeStrength = 4f, float duration = .3f)
    {
        movement.ToggleFlip(false);

        if(raycast.playerInRangeClose) //Player too close
        {
            if(raycast.backToWall || !raycast.backToLedge)
            {
                //Lunge forwards
                lungeStrength += 4;
                LungeStart(movement.isFacingRight, lungeStrength, duration);
            }
            else
            {
                //Lunge backwards
                lungeStrength += 4;
                LungeStart(!movement.isFacingRight, lungeStrength, duration);
            }
        }
    }

    void LungeStart(bool lungeToRight, float strength = 4f, float duration = .3f)
    {
        // combat.animator.PlayManualAnim(1, 0.75f);
        movement.Lunge(lungeToRight, strength, duration);
    }

    void ManualFlip(bool faceRight)
    {
        movement.ManualFlip(faceRight);
        movement.ToggleFlip(false);
    }
}
