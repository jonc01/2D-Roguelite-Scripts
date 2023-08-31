using System.Collections;
using System.Collections.Generic;
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
        StartCoroutine(ShootCO());
    }

    IEnumerator ShootCO()
    {
        canFire = false;
        canAttack = false;
        movement.ToggleFlip(false);
        movement.canMove = false;
        combat.isAttacking = true;
        combat.knockbackImmune = true;

        combat.animator.PlayManualAnim(0, fullAnimTime);

        yield return new WaitForSeconds(chargeUpAnimDelay); //Charge up anim

        //Instantiate projectile, set variables from this script
        GameObject projectileObj = Instantiate(projectile, attackPoint.position, transform.rotation);
        ProjectileController script = projectileObj.GetComponent<ProjectileController>();
        script.damage = damage;
        script.playerToRight = combat.playerToRight; //Knockback direction

        yield return new WaitForSeconds(fullAnimTime - chargeUpAnimDelay);
        combat.isAttacking = false;
        combat.knockbackImmune = false;
        movement.canMove = true;
        movement.ToggleFlip(true);

        yield return new WaitForSeconds(attackSpeed);
        canFire = true;
        canAttack = true;
    }
}
