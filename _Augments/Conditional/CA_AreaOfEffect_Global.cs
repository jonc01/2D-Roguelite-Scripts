using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CA_AreaOfEffect_Global : Base_ConditionalAugments
{
    //Explosion is instantiated at the Enemy position
    [SerializeField] GameObject explosionPrefab;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Activate(Transform objectHitPos)
    {
        if(!CanActivate()) return;

        StartProcCooldown();
        //Instantiate an explosion at the enemy position, prefab applies status effect
        if(explosionPrefab != null)
        {    
            Instantiate(explosionPrefab, objectHitPos.position, explosionPrefab.transform.rotation);
        }
        else
        {
            Activate(); //
        }
    }
}
