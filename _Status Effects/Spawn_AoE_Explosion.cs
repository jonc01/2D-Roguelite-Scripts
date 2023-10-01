using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_AoE_Explosion : Base_AoE_Explosion
{
    [Header("= Spawn Only =")]
    [SerializeField] Transform hitboxOffset;

    protected override void Explode()
    {
        // base.Explode();
        Collider2D[] hitEnemies = 
            Physics2D.OverlapBoxAll(hitboxOffset.position,
            new Vector2(hitboxWidth, hitBoxHeight), 0, enemyLayer);

        foreach(Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                // damageable.TakeDamage(damage, true, knockbackStrength);
                // ScreenShakeListener.Instance.Shake(2);
                
                // Transform enemyHitOffset = damageable.GetPosition();
                Transform enemyHitOffset = damageable.GetGroundPosition();

                if(statusEffectPrefab != null)
                {
                    Instantiate(statusEffectPrefab, enemyHitOffset.position, statusEffectPrefab.transform.rotation, enemy.transform);
                }
            }
        }
    }
}
