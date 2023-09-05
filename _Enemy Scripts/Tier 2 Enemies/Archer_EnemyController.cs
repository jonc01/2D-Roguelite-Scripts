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
        }
        else
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
            // combat.AttackFar();
        }
        // base.AttackCheckFar();
    }

    protected float PlayerDistCheck()
    {
        distanceToPlayer = Mathf.Abs(playerPos.position.x - transform.position.x);
        return distanceToPlayer;
    }

    IEnumerator LungeAttack()
    {
        combat.isAttacking = true;
        combat.animator.PlayManualAnim(1, 0.75f); //Vanish
        combat.ToggleHealthbar(false);
        combat.ToggleDamageImmune(true);
        yield return new WaitForSeconds(0.2f);

        //Lunge then flip towards the Player and start Attack
        if(PlayerDistCheck() <= raycast.attackRangeClose) LungeCheck(9);
        else LungeCheck(0);
        movement.canMove = false;
        yield return new WaitForSeconds(0.55f);
        combat.ToggleDamageImmune(false);
        combat.ToggleHealthbar(true);
        // yield return new WaitForSeconds(.35f);
        movement.canMove = false;
        movement.rb.velocity = Vector3.zero;
        
        ManualFlip(raycast.playerToRight);
        yield return new WaitForSeconds(0.05f);
        // ManualFlip(raycast.playerDetectedToRight);
        combat.isAttacking = false;
        combat.AttackFar();
    }

    public void LungeCheck(float lungeStrength = 4f, float duration = .3f)
    {
        movement.ToggleFlip(false);
        movement.canMove = false;
        
        if(raycast.playerInRangeClose) //Player too close
        {
            // lungeStrength += 4;
            if(raycast.backToWall || !raycast.backToLedge)
            {
                //Lunge forwards
                LungeStart(movement.isFacingRight, lungeStrength, duration);
            }
            else
            {
                //Lunge backwards
                LungeStart(movement.isFacingRight, lungeStrength, duration);
                // LungeStart(!movement.isFacingRight, lungeStrength, duration);
                // LungeStart(!raycast.playerToRight, lungeStrength, duration);
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
