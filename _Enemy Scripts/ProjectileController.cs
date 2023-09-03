using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Variables")]
    public float damage = 5;
    public float knockbackStrength = 1;
    public float speed = 3;
    public bool enemyProj = true;
    [SerializeField] private bool projectileHit;
    [SerializeField] private float hitAnimTime;
    [SerializeField] public bool playerToRight;

    [Header("References")]
    [SerializeField] Animator animator;

    CircleCollider2D collider;

    protected virtual void Start()
    {
        if(animator == null) animator = GetComponent<Animator>();
        projectileHit = false;
        Invoke("DestroyProjectile", 3);
        collider = GetComponent<CircleCollider2D>();
    }

    protected virtual void Update()
    {
        if(projectileHit) { DestroyProjectile(); return; }
        transform.position += transform.right * Time.deltaTime * speed;
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if(enemyProj)
        {
            //Check for player collision
            var target = collision.GetComponent<Base_PlayerCombat>();
            if(target != null)
            {
                //Player parried, flip direction
                target.TakeDamage(damage, transform.position.x, true, 2);
                if(target.parryDeflecting)
                {
                    enemyProj = false;
                    gameObject.layer = LayerMask.NameToLayer("ProjectilePlayer");

                    speed *= -1;
                    return;
                }
                //Player hit
                target.GetKnockback(transform.position.x, knockbackStrength);
                DestroyProjectile();
                return;
            }
        }
        else if(!enemyProj) //Projectile was deflected, switches target to Enemies
        {
            var target = collision.GetComponent<Base_EnemyCombat>();
            if(target != null)
            {
                target.TakeDamageStatus(damage);
                DestroyProjectile();
                target.GetKnockback(!playerToRight, knockbackStrength);
            }
        }
        //It projectile hits wall, still destroy
        DestroyProjectile();
    }

    protected virtual void DestroyProjectile()
    {
        //Stops projectile on hit or when expiring, then plays the hit animation
        projectileHit = true;
        collider.enabled = false;
        animator.Play("Hit_Alt");
        Invoke("DestroyObject", hitAnimTime);
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
