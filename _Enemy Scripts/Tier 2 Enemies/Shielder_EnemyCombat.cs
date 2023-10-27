using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shielder_EnemyCombat : Base_EnemyCombat
{
    [Header("Shielder Only")]
    [SerializeField] Transform attackPoint2;
    [SerializeField] float hitBoxLength = 1.47f; //1.47f
    [SerializeField] float hitBoxHeight = .48f; //.48f

    [Header("= Die() Delay =")]
    [SerializeField] float deathDelayTime;

    protected override void Start()
    {
        base.Start();

        //Defaults
        // fullAttackAnimTime = 1.4167f;
        // attackDelayTime = 0.834f;
        //deathDelayTime = 0.667f;
    }

    public virtual void CheckCounterHit()
    {
        Collider2D[] hitPlayers =
            Physics2D.OverlapBoxAll(attackPoint2.position,
            new Vector2(hitBoxLength, hitBoxHeight), 0, playerLayer);

        foreach (Collider2D player in hitPlayers)
        {
            if (player.GetComponent<Base_PlayerCombat>() != null)
            {

                player.GetComponent<Base_PlayerCombat>().GetKnockback(transform.position.x, .2f);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (attackPoint == null && attackPoint2 == null) return;

        Gizmos.DrawWireCube(attackPoint2.position,
            new Vector3((hitBoxLength),
            hitBoxHeight, 0));

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public override void TakeDamage(float damageTaken, bool knockback = false, bool procOnHit = false, float strength = 8, float xPos = 0)
    {
        if (!isAlive || isSpawning) return;
        // if (movement.isFacingRight == playerToRight) //facing player, Shield blocks damage and knockback
        if ((transform.position.x < xPos && movement.isFacingRight) 
        || (transform.position.x > xPos && !movement.isFacingRight))
        {
            if(playAudioClips != null) playAudioClips.PlayBlockedAudio();
            if(instantiateManager != null)
            {
                instantiateManager.TextPopups.ShowBlocked(textPopupOffset.position);
                instantiateManager.HitEffects.ShowHitEffect(hitEffectsOffset.position);
            }
            // base.TakeDamage(0, knockback, 0); //TODO: if blocked damage is 0, may just reduce flip during attackCO
            CheckCounterHit();
        }
        else base.TakeDamage(damageTaken, knockback, procOnHit, strength, xPos);
    }

    public override void TakeDamageStatus(float damageTaken, int colorIdx)
    {
        //Ignore Block
        base.TakeDamageStatus(damageTaken, colorIdx);
    }

    protected override void Die()
    {
        healthBar.gameObject.SetActive(false);
        if (AttackingCO != null) StopCoroutine(AttackingCO);
        movement.rb.simulated = false;
        GetComponent<CapsuleCollider2D>().enabled = false;

        playAudioClips.PlayDeathSound();

        isAlive = false;

        if(augmentInventory != null) augmentInventory.OnKill(GetGroundPosition());
        if(screenShakeListener != null) screenShakeListener.Shake(2);
        //InstantiateManager.Instance.HitEffects.ShowKillEffect(hitEffectsOffset.position);

        if(instantiateManager != null)
        {
            instantiateManager.HitEffects.ShowKillEffect(hitEffectsOffset.position);
            instantiateManager.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);
        }

        StartCoroutine(DelayDeath());
    }

    IEnumerator DelayDeath()
    {
        //Delay screenshake and XP orb spawns to line up with death animation
        yield return new WaitForSeconds(deathDelayTime);

        if(screenShakeListener != null) screenShakeListener.Shake(2);

        // if(instantiateManager != null) //Moved to Die(), delay not needed
        // {
        //     instantiateManager.HitEffects.ShowKillEffect(hitEffectsOffset.position);
        //     instantiateManager.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);
        // }

        //Base_EnemyAnimator checks for isAlive to play Death animation
        if(enemyWaveManager != null) enemyWaveManager.UpdateEnemyCount();
        //sr.enabled = false;
        Invoke("DeleteObj", 1f); //Wait for fade out to finish
    }

    // private void DeleteObj() //Using base
    // {
    //     Destroy(gameObject);
    // }
}
