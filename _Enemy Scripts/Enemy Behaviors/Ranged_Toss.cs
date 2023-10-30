using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ranged_Toss : Ranged_Horizontal
{
    [Space(10)]
    [Header("= Ranged Toss =")]
    [SerializeField] GameObject tossedProjectile;
    [SerializeField] GameObject detonateIndicator;
    [Space(10)]
    [SerializeField] public float tossForce = 3;
    [SerializeField] public float launchAngle = 45f;

    protected override void Start()
    {
        base.Start();
        canFire = true;
        //damage = combat.attackDamage; //! Manually setting damage
    }

    public override void Attack()
    {
        // base.Attack();
        if(!canFire || combat.isAttacking) return;
        StartCoroutine(RangeTossCO());
    }

    IEnumerator RangeTossCO()
    {
        canFire = false;
        canAttack = false;
        movement.ToggleFlip(false);
        movement.canMove = false;
        combat.isAttacking = true;
        combat.knockbackImmune = true;

        combat.instantiateManager.TextPopups.ShowIndicator(combat.hitEffectsOffset.position);

        yield return new WaitForSeconds(chargeUpAnimDelay);

        combat.animator.PlayManualAnim(0, fullAnimTime);
        PlayIndicatorExplode();

        LaunchProjectile();

        //End of attack bools
        yield return new WaitForSeconds(animEndingTime);
        combat.isAttacking = false;
        combat.knockbackImmune = false;
        movement.canMove = true;
        movement.ToggleFlip(true);

        yield return new WaitForSeconds(attackSpeed);
        canFire = true;
        canAttack = true;
    }

    protected void LaunchProjectile()
    {
        //Set stats
        GameObject projectileObj = Instantiate(tossedProjectile, combat.hitEffectsOffset.position, Quaternion.identity);
        GrenadeController script = projectileObj.GetComponent<GrenadeController>();
        script.damage = damage;

        //Launch towards Player position
        Vector3 playerPos = GetPlayerPosX();
        float distToPlayer = playerPos.x - transform.position.x;

        float toRight = 1f;
        if(distToPlayer < 0) toRight = -1;
        else toRight = 1;

        float absLaunchDist = Mathf.Abs(distToPlayer);
        float absDist = absLaunchDist;
    
        //Setting launch target based on distance to player
        Vector3 launchDirection;
        launchDirection = playerPos - transform.position;

        if(absLaunchDist >= 2.5f) absLaunchDist = 1f;
        else if(absLaunchDist >= 2) absLaunchDist -= 1.3f;
        else if(absLaunchDist >= 1) absLaunchDist -= 1f;
        else if(absLaunchDist >= .75) absLaunchDist -= .8f;

        launchDirection.x = absLaunchDist * toRight;
        launchAngle = 45;
        //Compare y positions to get launch force
        float yPosComparison = playerPos.y - transform.position.y;
        if(yPosComparison >= 1)
        { //Update angles if too far
            if(absDist > 1f) { tossForce = 5; launchAngle += 15f; } 
            else tossForce = 6;
        }
        else if(yPosComparison <= -1)
        {
            if(absDist > 1f) { tossForce = 3; script.TempAllowDropThrough(.8f); }
            else { tossForce = 1; script.TempAllowDropThrough(); }
        }
        else
        {
            tossForce = 3;
        }

        launchDirection.y = Mathf.Tan(Mathf.Deg2Rad * launchAngle);
        script.rb.velocity = tossForce * launchDirection;
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
