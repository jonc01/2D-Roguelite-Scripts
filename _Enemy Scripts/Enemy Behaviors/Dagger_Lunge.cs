using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger_Lunge : Base_CombatBehavior
{
    
    [Header("= LungeAttack =")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator anim;
    // [SerializeField] bool canLunge;
    [SerializeField] int currAttack;
    [SerializeField] float attackResetDelay = 2f;
    [SerializeField] float attackResetTimer;
    [Space(10)]
    [SerializeField] float[] fullAnimTimes;
    [SerializeField] float[] animDelayTimes;

    [Space(10)]
    [Header("= Lunge HitBox =")]
    [SerializeField] bool showGizmos = false;
    [SerializeField] Transform[] attackPoint;
    [SerializeField] float[] hitboxWidth;
    [SerializeField] float[] hitboxHeight;

    protected override void Start()
    {
        base.Start(); //base script references
        // canLunge = true;
        if(anim == null) anim = GetComponentInChildren<Animator>();
        if(rb == null) rb = GetComponent<Rigidbody2D>();

        attackResetTimer = 0;
        currAttack = 0;
    }

    public override void Attack()
    {
        // if(!canLunge || !combat.isAlive) return;
        if(!combat.isAlive) return;
        if (combat.isAttacking || combat.altAttacking) return;
        
        if(currAttack == 0)
        {
            StartCoroutine(LungeAttackCO());
        }else{
            StartCoroutine(SwipeAttackCO());
        }
    }

    void Update()
    {
        //Timer keeps track of when to reset attack counter to first attack
        if(attackResetTimer > 0)
        {
            attackResetTimer -= Time.deltaTime;
            if(attackResetTimer <= 0) currAttack = 0;
        }
    }

    IEnumerator LungeAttackCO()
    {
        combat.isAttacking = true;
        combat.altAttacking = true;
        combat.chasePlayer = false;

        // combat.knockbackImmune = true;

        yield return new WaitForSeconds(chargeUpAnimDelay);

        movement.ToggleFlip(false);

        // combat.Lunge(movement.isFacingRight, 8);
        combat.GetKnockback(movement.isFacingRight, 8); //TODO: no idea why Lunge doesn't work
        yield return new WaitForSeconds(.1f); //Lunge start before animation

        //Attack 1
        combat.animator.PlayManualAnim(0, fullAnimTimes[0]);
        yield return new WaitForSeconds(animDelayTimes[0] - .1f);
        movement.canMove = false;
        CheckHit(0);

        yield return new WaitForSeconds(fullAnimTimes[0] - animDelayTimes[0]);

        movement.ToggleFlip(true);

        //Attack 2
        combat.animator.PlayManualAnim(1, fullAnimTimes[1]);
        yield return new WaitForSeconds(animDelayTimes[1]);
        CheckHit(1);

        yield return new WaitForSeconds(fullAnimTimes[1] - animDelayTimes[1]);

        yield return new WaitForSeconds(attackSpeed);

        combat.isAttacking = false;
        combat.altAttacking = false;
        movement.canMove = true;
        combat.chasePlayer = true;

        currAttack = 1;
        attackResetTimer = attackResetDelay; //Start time to reset attack combo if delayed
        // combat.knockbackImmune = false;
    }

    IEnumerator SwipeAttackCO()
    {
        combat.isAttacking = true;
        combat.altAttacking = true;
        movement.canMove = false;
        combat.chasePlayer = false;

        movement.ToggleFlip(false);
        combat.animator.PlayManualAnim(2, fullAnimTimes[2]);
        yield return new WaitForSeconds(animDelayTimes[2]);
        CheckHit(2);
        yield return new WaitForSeconds(fullAnimTimes[2] - animDelayTimes[2]);

        yield return new WaitForSeconds(attackSpeed);
        movement.ToggleFlip(true);

        combat.isAttacking = false;
        combat.altAttacking = false;
        movement.canMove = true;
        combat.chasePlayer = true;

        attackResetTimer = 0; //Timer no longer needed
        currAttack = 0;
    }

    private void CheckHit(int index)
    {
        // Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, combat.attackRange, combat.playerLayer);

        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackPoint[index].position, new Vector2(hitboxWidth[index], hitboxHeight[index]), 0, combat.playerLayer);

        foreach (Collider2D player in hitPlayers)
        {
            if (player.GetComponent<Base_PlayerCombat>() != null)
            {
                playerHit = true;
                player.GetComponent<Base_PlayerCombat>().TakeDamage(combat.attackDamage, transform.position.x);
                //TODO: pass xPos to hit the Player and get < or > to get knockback direction
                // player.GetComponent<Base_PlayerCombat>().GetKnockback(!combat.playerToRight, combat.knockbackStrength);
                player.GetComponent<Base_PlayerCombat>().GetKnockback(transform.position.x, combat.knockbackStrength);
            }
        }
    }

    void OnDrawGizmos()
    {
        if(showGizmos)
        {
            if(attackPoint == null) return;

            Gizmos.color = Color.red;

            for(int i=0; i<hitboxHeight.Length; i++)
            {
                Gizmos.DrawWireCube(attackPoint[i].position, new Vector3(hitboxWidth[i], hitboxHeight[i], 0));
            }

        }
    }
}
