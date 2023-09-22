using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranged_Global : Ranged_Horizontal
{
    [Space(10)]
    [Header("= Ranged Global =")]
    //Set Projectile Damage and Speed from here, projectile prefab is set at instantiation
    [SerializeField] GameObject globalProjectile;
    [SerializeField] GameObject detonateIndicator;

    protected override void Start()
    {
        base.Start();
        canFire = true;
        //damage = combat.attackDamage; //! Manually setting damage
    }

    public override void Attack()
    {
        // base.Attack();
        if (!canFire || combat.isAttacking) return;
        StartCoroutine(GlobalShootCO());
    }

    IEnumerator GlobalShootCO()
    {
        canFire = false;
        canAttack = false;
        movement.ToggleFlip(false);
        movement.canMove = false;
        combat.isAttacking = true;
        combat.knockbackImmune = true;


        combat.PlayIndicator(); //---------------------------
        Vector3 playerPos = GetPlayerPosX(); //Only get player position here
        
        yield return new WaitForSeconds(chargeUpAnimDelay); //Charge up portion of animation

        combat.animator.PlayManualAnim(0, fullAnimTime);

        PlayIndicatorExplode();

        //Instantiate projectile, set variables from this script
        GameObject projectileObj = Instantiate(globalProjectile, playerPos, transform.rotation);
        CastExplosion script = projectileObj.GetComponent<CastExplosion>();
        script.damage = damage;


        yield return new WaitForSeconds(animEndingTime);
        combat.isAttacking = false;
        combat.knockbackImmune = false;
        movement.canMove = true;
        movement.ToggleFlip(true);

        yield return new WaitForSeconds(attackSpeed);
        canFire = true;
        canAttack = true;
    }

    private Vector3 GetPlayerPosX()
    {
        //Get Player position and cast explosion for Initial explosion
        Vector3 playerPos = GameManager.Instance.playerTransform.position;
        float playerX = playerPos.x;
        float playerY = playerPos.y;

        Vector3 castPos = new Vector3(playerX, playerY, 0);
        return castPos;
    }

    void PlayIndicatorExplode()
    {
        if(detonateIndicator == null) return;
        detonateIndicator.SetActive(true);
    }
}
