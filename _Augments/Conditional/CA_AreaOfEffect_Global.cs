using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CA_AreaOfEffect_Global : Base_ConditionalAugments
{
    //Explosion is instantiated at the Enemy position
    [SerializeField] GameObject explosionPrefab;

    [Header("- optional override -")]
    [SerializeField] protected bool usePlayerDamage = false;
    [Header("- Debug - set at Start()")]
    [SerializeField] Base_AoE_Explosion explosionPrefabScript;

    protected override void Start()
    {
        base.Start();
        explosionPrefabScript = explosionPrefab.GetComponent<Base_AoE_Explosion>();
    }

    protected override void Activate(Transform objectHitPos)
    {
        if(!CanActivate()) return;

        StartProcCooldown();
        //Instantiate an explosion at the enemy position, prefab applies status effect
        if(explosionPrefab != null)
        {
            if(usePlayerDamage)
            {
                explosionPrefabScript.damage = playerCombat.GetMultipliedDamage();
                Instantiate(explosionPrefab, objectHitPos.position, explosionPrefab.transform.rotation);
            }
            else
            {
                Instantiate(explosionPrefab, objectHitPos.position, explosionPrefab.transform.rotation);
            }
        }
        else
        {
            Activate(); //
        }
    }
}
