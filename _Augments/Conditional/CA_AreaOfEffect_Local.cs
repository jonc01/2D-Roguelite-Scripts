using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_AreaOfEffect_Local : Base_ConditionalAugments
{
    [SerializeField] public float damage;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] Transform attackPoint;
    [SerializeField] private float hitboxWidth;
    [SerializeField] private float hitBoxHeight;
    [SerializeField] private float knockbackStrength = 4f;
    [SerializeField] private bool showGizmos = false;

    protected override void Start()
    {
        base.Start();
        attackPoint = GameManager.Instance.playerTargetOffset;
    }

    protected override void Activate()
    {
        //Only affects enemies within hitbox range
        Collider2D[] hitEnemies = 
            Physics2D.OverlapBoxAll(attackPoint.position,
            new Vector2(hitboxWidth, hitBoxHeight), 0, enemyLayer);

        //if (damageMultiplier > 1) knockbackStrength = 6; //TODO: set variable defintion in Inspector

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                damageable.TakeDamage(damage, true, knockbackStrength);
                ScreenShakeListener.Instance.Shake(1); //TODO: if Crit
            }
        }
    
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (attackPoint == null) return;

            Gizmos.DrawWireCube(attackPoint.position, 
                new Vector3((hitboxWidth),
                hitBoxHeight, 0));
        }
    }
}
