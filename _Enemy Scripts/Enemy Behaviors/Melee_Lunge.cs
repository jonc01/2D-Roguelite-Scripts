using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee_Lunge : Base_CombatBehavior
{
    //If the player is within the range, lunge at player

    [Header("Lunge")]
    [SerializeField] bool allowFlipBeforeAttack = false;
    [SerializeField] bool canLunge;
    [SerializeField] public bool isLunging;

    [SerializeField] bool allowCollision;

    protected override void Start()
    {
        base.Start(); //base script references
        allowCollision = false;
        canLunge = true;
        isLunging = false;
    }

    void FixedUpdate()
    {
        if (!combat.isAlive || !combat.isAttacking) return;
        if (playerHit || !allowCollision) return;
        CheckHit();
    }

    public override void Attack() 
    {
        if (!canLunge || !combat.isAlive) return;

        StartCoroutine(LungeCO());
    }

    IEnumerator LungeCO()
    {
        playerHit = false;
        canLunge = false;
        movement.ToggleFlip(false);
        combat.isAttacking = true;

        //Start Lunge animation
        combat.animator.PlayManualAnim(0, fullAnimTime);
        combat.PlayIndicator();
        
        yield return new WaitForSeconds(chargeUpAnimDelay); //Charge up portion of animation

        if(allowFlipBeforeAttack) combat.FacePlayer();

        allowCollision = true;
        // combat.Lunge(movement.isFacingRight, 8f, .2f);
        combat.GetKnockback(movement.isFacingRight, 8f, .2f);
        
        yield return new WaitForSeconds(animEndingTime);
        allowCollision = false;
        combat.isAttacking = false;

        movement.ToggleFlip(true);
        movement.canMove = true;
        
        yield return new WaitForSeconds(attackSpeed);
        canLunge = true;
    }

    private void CheckHit()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, combat.attackRange, combat.playerLayer);
        foreach (Collider2D player in hitPlayers)
        {
            if (player.GetComponent<Base_PlayerCombat>() != null)
            {
                playerHit = true;
                player.GetComponent<Base_PlayerCombat>().TakeDamage(combat.attackDamage, transform.position.x);
                
                player.GetComponent<Base_PlayerCombat>().GetKnockback(transform.position.x, combat.knockbackStrength);
            }
        }
    }
}
