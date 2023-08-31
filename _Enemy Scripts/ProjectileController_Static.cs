using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController_Static : ProjectileController
{
    [Header("= Static Projectile = Setup")]
    [SerializeField] Animator anim;
    [SerializeField] string animName;
    [SerializeField] int hashedAnim;
    [Space(10)]
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRangeX;
    [SerializeField] float attackRangeY;
    [Space(10)]
    [Header("Animation Times")]
    [SerializeField] float fullAnimTime;
    [SerializeField] float animDelay;

    protected override void Start()
    {
        //overriding base
        //StartCoroutine(AttackCO());
    }

    protected override void Update()
    {
        
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //override
    }

    void OnEnable()
    {
        StartCoroutine(AttackCO());
    }

    IEnumerator AttackCO()
    {
        anim.Play(hashedAnim);
        yield return new WaitForSeconds(animDelay);
        CheckHit();

        yield return new WaitForSeconds(fullAnimTime - animDelay);
        DeleteObj();
    }

    void CheckHit()
    {
        Vector3 hitboxSize = new Vector3 (attackRangeX, attackRangeY, 0);
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackPoint.position, hitboxSize, 0f, playerLayer);
        
        foreach (Collider2D player in hitPlayers)
        {
            if (player.GetComponent<Base_PlayerCombat>() != null)
            {
                var playerObj = player.GetComponent<Base_PlayerCombat>();
                playerObj.TakeDamage(damage);
                if (knockbackStrength > 0) playerObj.GetKnockback(transform.position.x, knockbackStrength);
            }
        }
    }

    private void DeleteObj()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 0));
    }
}
