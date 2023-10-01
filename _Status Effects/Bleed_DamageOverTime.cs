using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bleed_DamageOverTime : Base_DamageOverTime
{
    [Header("- Bleed -")]
    // [SerializeField] private float extendTimer = 2;
    [SerializeField] private float damageIncreasePerTick = 1;
    [SerializeField] private float maxDamagePerTick = 20;
    [SerializeField] List<Bleed_DamageOverTime> existingBleeds;

    protected override void Start()
    {
        existingBleeds = new List<Bleed_DamageOverTime>();

        base.Start();
        CheckForExistingDoT();
    }

    protected override void CheckForExistingDoT()
    {
        int totalBleeds = 0;

        for(int i=0; i<transform.parent.childCount; i++)
        {
            Bleed_DamageOverTime currBleed = transform.parent.GetChild(i).GetComponent<Bleed_DamageOverTime>();

            //Only longer duration if target is already poisoned
            if(currBleed != null)
            {
                existingBleeds.Add(currBleed);
                totalBleeds++;
            }
            if(totalBleeds > 1)
            {
                existingBleeds[0].ResetTimer(); //Reset local duration
                existingBleeds[0].IncreaseDamagePerTick(damageIncreasePerTick);
                isActive = false;
            }
        }
    }

    public void IncreaseDamagePerTick(float addDamagePerTick)
    {
        damagePerTick += addDamagePerTick;
        //Damage cannot exceed max
        if(damagePerTick > maxDamagePerTick)
            damagePerTick = maxDamagePerTick;
    }
}
