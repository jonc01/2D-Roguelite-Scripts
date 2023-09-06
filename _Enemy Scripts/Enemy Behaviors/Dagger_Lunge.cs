using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger_Lunge : Base_CombatBehavior
{
    
    [Header("= LungeAttack =")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator anim;
    [SerializeField] bool canLunge;
    [SerializeField] int currAttack;
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
        canLunge = true;
        if(anim == null) anim = GetComponentInChildren<Animator>();
        if(rb == null) rb = GetComponent<Rigidbody2D>();

        currAttack = 0;
    }

    public override void Attack()
    {
        if(!canLunge || !combat.isAlive) return;

        if(combat.isAttacking) return;
        if(currAttack == 0)
        {
            StartCoroutine(LungeAttackCO());
        }else{
            StartCoroutine(SwipeAttackCO());
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            // TESTLUNGE();
            combat.Lunge(movement.isFacingRight, 8);
        }
    }

    IEnumerator LungeAttackCO()
    {
        Debug.Log("starting lunge Dagger");
        canLunge = false;
        combat.isAttacking = true;
        combat.altAttacking = true;

        // combat.knockbackImmune = true;

        yield return new WaitForSeconds(chargeUpAnimDelay);

        //Attack 1
        combat.animator.PlayManualAnim(0, fullAnimTimes[0]);
        // StartLunge(raycast.playerToRight);
        combat.Lunge(movement.isFacingRight);
        yield return new WaitForSeconds(AnimDelayTime(0));
        CheckHit(0);

        yield return new WaitForSeconds(GetFullAnimTime(0) - AnimDelayTime(0));

        //Attack 2
        combat.animator.PlayManualAnim(1, fullAnimTimes[1]);
        // StartLunge(raycast.playerToRight); //- might not use lunge, since no delay
        combat.Lunge(movement.isFacingRight);
        yield return new WaitForSeconds(AnimDelayTime(1));
        CheckHit(1);

        yield return new WaitForSeconds(GetFullAnimTime(1) - AnimDelayTime(1));

        yield return new WaitForSeconds(attackSpeed);

        canLunge = true;
        combat.isAttacking = false;
        combat.altAttacking = false;
        currAttack = 1;
        // combat.knockbackImmune = false;
    }

    IEnumerator SwipeAttackCO()
    {
        combat.isAttacking = true;

        combat.animator.PlayManualAnim(2, fullAnimTimes[2]);
        yield return new WaitForSeconds(AnimDelayTime(2));
        CheckHit(2);
        yield return new WaitForSeconds(GetFullAnimTime(2) - AnimDelayTime(2));

        yield return new WaitForSeconds(attackSpeed);

        combat.isAttacking = false;
        currAttack = 0;
    }

    // void PlayAnimation(int index)
    // {
    //     combat.animator.PlayManualAnim(index, fullAnimTimes[index]);
    // }

    float GetFullAnimTime(int index)
    {
        return fullAnimTimes[index];
    }

    float AnimDelayTime(int index)
    {
        return animDelayTimes[index];
    }

    private void CheckHit(int index)
    {
        // Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, combat.attackRange, combat.playerLayer);

        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackPoint[index].position, new Vector2(hitboxWidth[index], hitboxHeight[index]), combat.playerLayer);

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
