using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_AoE_Explosion : MonoBehaviour
{
    //Explosion object that applies status effects to objects hit

    [Header("Animator")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected int hashedAnimName;
    [SerializeField] protected float animDelay = 0;
    [SerializeField] protected float animDuration;

    [Header("-----")]
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] public float damage;
    [SerializeField] protected bool statusDamage = true; //can't be blocked
    [SerializeField] protected Transform optionalHitBoxOffset;
    [SerializeField] protected float hitboxWidth;
    [SerializeField] protected float hitBoxHeight;
    [SerializeField] protected float knockbackStrength = 4f;
    [SerializeField] protected bool showGizmos = false;

    [Header("= Status Effect =")]
    [SerializeField] protected GameObject statusEffectPrefab;

    [Header("- optional -")]
    [SerializeField] protected bool attachStatusToEnemy = true;
    [SerializeField] protected bool setStatusToGround = true;

    protected virtual void Start()
    {
        if(animator == null) animator = GetComponent<Animator>();
        if(animDuration > 0) animator.Play(hashedAnimName);

        // CheckExplodeHit();
        // StartCoroutine(DelayedExplodeCO());

        if(optionalHitBoxOffset == null) optionalHitBoxOffset = transform;
        
        Invoke("CheckExplodeHit", animDelay);
        Invoke("DeleteObject", animDuration); //delete object after the animation is over
    }

    // protected virtual IEnumerator DelayedExplodeCO()
    // {
    //     yield return new WaitForSeconds(animDelay);
    //     CheckExplodeHit();
    //     // yield return new WaitForSeconds(animDuration - animDelay);
    // }

    protected virtual void CheckExplodeHit()
    {
        // attackPoint = 

        //Only affects enemies within hitbox range
        Collider2D[] hitEnemies = 
            Physics2D.OverlapBoxAll(optionalHitBoxOffset.position,
            new Vector2(hitboxWidth, hitBoxHeight), 0, enemyLayer);

        //if (damageMultiplier > 1) knockbackStrength = 6; //TODO: set variable defintion in Inspector
        
        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable == null) return;
        
            if(statusDamage) damageable.TakeDamageStatus(damage, 0);
            else damageable.TakeDamage(damage, true, false, knockbackStrength, transform.position.x);

            ScreenShakeListener.Instance.Shake(2);
            // Transform enemyHitOffset = damageable.GetPosition();
            Transform enemyHitOffset;
            
            if(setStatusToGround) enemyHitOffset = damageable.GetGroundPosition();
            else enemyHitOffset = damageable.GetHitPosition();

            if(statusEffectPrefab == null) return;
            
            if(attachStatusToEnemy)
            {
                Instantiate(statusEffectPrefab, enemyHitOffset.position, statusEffectPrefab.transform.rotation, enemy.transform);
            }
            else
            {
                Instantiate(statusEffectPrefab, enemyHitOffset.position, enemyHitOffset.transform.rotation);
            }
        }
    }

    protected void DeleteObject()
    {
        Destroy(gameObject);
    }

    protected void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.DrawWireCube(optionalHitBoxOffset.position, 
                new Vector3(hitboxWidth,
                hitBoxHeight, 0));
        }
    }
}
