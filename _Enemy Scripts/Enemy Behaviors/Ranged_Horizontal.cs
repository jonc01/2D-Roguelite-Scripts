using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranged_Horizontal : Base_CombatBehavior
{
    //If player is in range, fire a projectile

    [Header("Ranged Projectile")]
    [SerializeField] GameObject projectile;
    [SerializeField] Transform attackPoint;
    //Set Projectile Damage and Speed from here, projectile prefab is set at instantiation
    [SerializeField] public float damage = 4;
    [SerializeField] public float speed = 3; //default 3
    [SerializeField] public bool canFire;


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
        canAttack = false;
        movement.ToggleFlip(false);
        movement.canMove = false;
        combat.isAttacking = true;

        combat.animator.PlayManualAnim(0, fullAnimTime);

        yield return new WaitForSeconds(animDelay); //Charge up anim

        //Instantiate projectile, set variables from this script
        GameObject projectileObj = Instantiate(projectile, attackPoint.position, transform.rotation);
        ProjectileController script = projectileObj.GetComponent<ProjectileController>();
        script.damage = damage;
        script.speed = speed;
        script.playerToRight = combat.playerToRight; //Knockback direction

        yield return new WaitForSeconds(animEndingTime);
        combat.isAttacking = false;
        movement.canMove = true;
        movement.ToggleFlip(true);

        yield return new WaitForSeconds(attackSpeed);
        canFire = true;
        canAttack = true;
    }
}
