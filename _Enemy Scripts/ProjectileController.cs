using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Variables")]
    public float damage = 5;
    public float knockbackStrength = 1;
    public float speed = 3;
    [SerializeField] private bool projectileHit;
    [SerializeField] private float hitAnimTime;
    [SerializeField] public bool playerToRight;

    [Header("References")]
    [SerializeField] Animator animator;

    CircleCollider2D collider;

    void Start()
    {
        if(animator == null) animator = GetComponent<Animator>();
        projectileHit = false;
        Invoke("DestroyProjectile", 3);
        collider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if(projectileHit) { DestroyProjectile(); return; }
        transform.position += transform.right * Time.deltaTime * speed;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        animator.Play("Hit_Alt");

        //Check for player collision
        var target = collision.GetComponent<Base_PlayerCombat>();
        if(target != null){ target.TakeDamage(damage, transform.position.x, true, 2); target.GetKnockback(transform.position.x, knockbackStrength); }

        //It projectile hits wall, still destroy
        DestroyProjectile();
    }

    private void DestroyProjectile()
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
