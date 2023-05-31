using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_AreaOfEffect_Global : Base_ConditionalAugments
{
    [SerializeField] public float damage;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] Transform attackPoint; //Set to enemy location
    [SerializeField] private float hitboxWidth;
    [SerializeField] private float hitBoxHeight;
    [SerializeField] private float knockbackStrength = 4f;
    [SerializeField] private bool showGizmos = false;

    protected override void Start()
    {
        base.Start();
        attackPoint = GameManager.Instance.PlayerTargetOffset;
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
                ScreenShakeListener.Instance.Shake(2);
            }
        }
    
    }
}
