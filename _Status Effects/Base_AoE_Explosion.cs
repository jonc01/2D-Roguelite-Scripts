using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_AoE_Explosion : MonoBehaviour
{
    //Explosion object that applies status effects to objects hit

    [Header("Animator")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected int hashedAnimName;
    [SerializeField] protected float animDuration;

    [Header("-----")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] public float damage;
    [SerializeField] protected float hitboxWidth;
    [SerializeField] protected float hitBoxHeight;
    [SerializeField] protected float knockbackStrength = 4f;
    [SerializeField] protected bool showGizmos = false;

    [Header("= Status Effect =")]
    [SerializeField] GameObject statusEffectPrefab;

    protected virtual void Start()
    {
        if(animator == null) animator = GetComponent<Animator>();
        animator.Play(hashedAnimName);
        Explode();
        Invoke("DeleteObject", animDuration); //delete object after the animation is over
    }

    protected virtual void Explode()
    {
        // attackPoint = 

        //Only affects enemies within hitbox range
        Collider2D[] hitEnemies = 
            Physics2D.OverlapBoxAll(transform.position,
            new Vector2(hitboxWidth, hitBoxHeight), 0, enemyLayer);

        //if (damageMultiplier > 1) knockbackStrength = 6; //TODO: set variable defintion in Inspector
        

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                damageable.TakeDamage(damage, true, knockbackStrength);
                ScreenShakeListener.Instance.Shake(2);
                // Transform enemyHitOffset = damageable.GetPosition();
                Transform enemyHitOffset = damageable.GetGroundPosition();

                if(statusEffectPrefab != null)
                {
                    Instantiate(statusEffectPrefab, enemyHitOffset.position, statusEffectPrefab.transform.rotation, enemy.transform);
                }
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
            Gizmos.DrawWireCube(transform.position, 
                new Vector3((hitboxWidth),
                hitBoxHeight, 0));
        }
    }
}
