using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEBUGGING_MODE : MonoBehaviour
{
    
    [SerializeField] Base_PlayerCombat playerCombat;
    [SerializeField] Transform playerPos;
    [SerializeField] LayerMask enemyLayer;

    [Header("Kill box")]
    [SerializeField] float damage;
    [SerializeField] float hitboxWidth;
    [SerializeField] float hitBoxHeight;
    [SerializeField] bool showGizmos = false;
    

    void Start()
    {
        playerPos = playerCombat.transform;
    }

    void Update()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.BackQuote)) //tilde key
        {
            if(playerCombat == null) return;
            playerCombat.maxHP = 9999;
            playerCombat.HealPlayer(9999);
            playerCombat.kbResist = 9999;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            playerCombat.HealPlayer(20f);
        }

        if(Input.GetKey(KeyCode.RightShift))
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                KillAroundPlayer();
            }
        }
        #endif
    }

    void KillAroundPlayer()
    {
        Collider2D[] hitEnemies = 
            Physics2D.OverlapBoxAll(playerPos.position,
            new Vector2(hitboxWidth, hitBoxHeight), 0, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                // damageable.TakeDamage(damage);
                damageable.TakeDamageStatus(damage, 3);
                ScreenShakeListener.Instance.Shake(3);
                Transform enemyHitOffset = damageable.GetHitPosition();

                // if(statusEffectPrefab != null)
                // {
                //     Instantiate(statusEffectPrefab, enemyHitOffset.position, statusEffectPrefab.transform.rotation, enemy.transform);
                // }
            }
        }
    }

    protected void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.DrawWireCube(transform.position, 
                new Vector3(hitboxWidth,
                hitBoxHeight, 0));
        }
    }
}
