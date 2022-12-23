using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranged_Horizontal : Base_CombatBehavior
{
    //If player is in range, fire a projectile

    [Header("Ranged Projectile")]
    [SerializeField] GameObject projectile;
    [SerializeField] Transform attackPoint;
    [SerializeField] public float damage = 4;
    [SerializeField] public float speed = 3; //default 3
    [SerializeField] bool canFire;


    protected override void Start()
    {
        base.Start();
        canFire = true;
        //damage = combat.attackDamage; //! Manually setting damage
    }


    public override void Attack()
    {
        if (!canFire || combat.isAttacking) return;
        StartCoroutine(ShootCO());
    }

    IEnumerator ShootCO()
    {
        canFire = false;
        movement.ToggleFlip(false);
        movement.canMove = false;
        combat.isAttacking = true;

        combat.animator.PlayManualAnim(0, fullAnimTime);

        yield return new WaitForSeconds(animDelay); //Charge up anim

        GameObject projectileObj = Instantiate(projectile, attackPoint.position, transform.rotation);
        ProjectileController script = projectileObj.GetComponent<ProjectileController>();
        script.damage = damage;
        script.speed = speed;
        script.playerToRight = combat.playerToRight; //Knockback direction

        yield return new WaitForSeconds(animEndingTime);

        yield return new WaitForSeconds(attackSpeed);
        canFire = true;
        movement.ToggleFlip(true);
        movement.canMove = false;
        combat.isAttacking = false;
    }
}
