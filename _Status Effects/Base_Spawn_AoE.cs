using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_Spawn_AoE : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] public float damage = 5;
    [SerializeField] public bool statusDamage = true;
    [SerializeField] public float knockbackStrength = 1;
    [SerializeField] public bool canProcOnHit = true;
    [SerializeField] protected GameObject explosionParentOffset;

    [Header("HitBox")]
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected float hitboxWidth = .5f;
    [SerializeField] protected float hitBoxHeight = .5f;
    [SerializeField] private bool showGizmos = false;
    [SerializeField] protected Transform hitboxOffset;
    
    [Header("Animation Setup")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected string animName;
    [SerializeField] protected int hashedAnimName;
    [SerializeField] protected int hashedAnimNameBlank; //-34935967

    [Space(10)]
    [SerializeField] protected float fullAnimTime;
    [SerializeField] protected float animDelayTime;

    
    protected void OnEnable() //TODO: or Start(), OnEnable() if pooling
    {
        if(explosionParentOffset == null) explosionParentOffset = gameObject;
        StartCoroutine(Attack());
    }

    protected virtual IEnumerator Attack()
    {
        anim.Play(hashedAnimName);

        yield return new WaitForSeconds(animDelayTime);
        CheckHit();

        yield return new WaitForSeconds(fullAnimTime - animDelayTime);

        Destroy(explosionParentOffset);
    }

    protected virtual void CheckHit()
    {
        //Only affects enemies within hitbox range
        Collider2D[] hitEnemies = 
            Physics2D.OverlapBoxAll(hitboxOffset.position,
            new Vector2(hitboxWidth, hitBoxHeight), 0, enemyLayer);

        //if (damageMultiplier > 1) knockbackStrength = 6; //TODO: set variable defintion in Inspector

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                if(statusDamage) damageable.TakeDamageStatus(damage, 0);
                else damageable.TakeDamage(damage, true, canProcOnHit, knockbackStrength, transform.position.x);

                ScreenShakeListener.Instance.Shake(1); //TODO: if Crit
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (hitboxOffset == null) return;

            Gizmos.DrawWireCube(hitboxOffset.position, 
                new Vector3((hitboxWidth),
                hitBoxHeight, 0));
        }
    }
}
