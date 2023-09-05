using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class Ranged_Static : Base_CombatBehavior
{
    [Header("= Ranged Static =")]
    [SerializeField] protected GameObject projectile;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] public float damage = 4;
    [SerializeField] public bool canFire;
    

    protected override void Start()
    {
        base.Start();
        canFire = true;
    }

    public override void Attack()
    {
        if (!canFire || combat.isAttacking) return;
        // if (!canFire || combat.altAttacking) return;
        StartCoroutine(ShootCO());
    }

    IEnumerator ShootCO()
    {
        combat.isAttacking = true;
        movement.ToggleFlip(false);
        canFire = false;
        canAttack = false;
        movement.canMove = false;
        combat.knockbackImmune = true;
        movement.rb.velocity = Vector3.zero;

        yield return new WaitForSeconds(chargeUpAnimDelay); //Charge up anim

        combat.animator.PlayManualAnim(0, fullAnimTime);

        //Instantiate projectile, set variables from this script
        GameObject projectileObj = Instantiate(projectile, attackPoint.position, transform.rotation);
        ProjectileController script = projectileObj.GetComponent<ProjectileController>();
        script.damage = damage;
        script.playerToRight = raycast.playerToRight; //Knockback direction

        yield return new WaitForSeconds(fullAnimTime - chargeUpAnimDelay);
        combat.knockbackImmune = false;
        combat.isAttacking = false;

        movement.canMove = true;
        yield return new WaitForSeconds(attackSpeed);
        combat.altAttacking = false;
        movement.ToggleFlip(true);
        canFire = true;
        canAttack = true;
    }
}
