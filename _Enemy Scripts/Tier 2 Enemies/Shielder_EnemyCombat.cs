using System;
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
    [SerializeField] float deathDelayFrames = 8;
    float deathDelayTime;

    protected override void Start()
    {
        base.Start();
        deathDelayTime = deathDelayFrames / sampleRate;
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
                player.GetComponent<Base_PlayerCombat>().GetKnockback(!playerToRight, .2f);
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

    public override void TakeDamage(float damageTaken, bool knockback = false, float strength = 8)
    {
        if (movement.isFacingRight == playerToRight)
        {
            InstantiateManager.Instance.TextPopups.ShowBlocked(textPopupOffset.position);
            InstantiateManager.Instance.HitEffects.ShowHitEffect(hitEffectsOffset.position);
            CheckCounterHit();
        }
        else base.TakeDamage(damageTaken, knockback, strength);
    }

    protected override void Die()
    {
        healthBar.gameObject.SetActive(false);
        if (AttackingCO != null) StopCoroutine(AttackingCO);
        movement.rb.simulated = false;
        GetComponent<CircleCollider2D>().enabled = false;

        isAlive = false;
        ScreenShakeListener.Instance.Shake(2);
        //InstantiateManager.Instance.HitEffects.ShowKillEffect(hitEffectsOffset.position);


        StartCoroutine(DelayDeath());
    }

    IEnumerator DelayDeath()
    {
        //Delay screenshake and XP orb spawns to line up with death animation
        yield return new WaitForSeconds(deathDelayTime);

        InstantiateManager.Instance.HitEffects.ShowKillEffect(hitEffectsOffset.position);

        ScreenShakeListener.Instance.Shake(2);
        InstantiateManager.Instance.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);

        //Base_EnemyAnimator checks for isAlive to play Death animation
        if(enemyStageManager != null) enemyStageManager.UpdateEnemyCount();
        //sr.enabled = false;
        Invoke("DeleteObj", 1f); //Wait for fade out to finish
    }

    private void DeleteObj()
    {
        Destroy(gameObject);
    }
}
