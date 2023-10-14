using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burn_DamageOverTime : Base_DamageOverTime
{
    [Header("- Burn -")]
    // [SerializeField] private float extendTimer = 2;
    //
    [SerializeField] private float damageIncreasePerTick = 1;
    [SerializeField] private float damagePerSecond = 1;
    [SerializeField] private float maxDamagePerTick = 10;
    [SerializeField] List<Burn_DamageOverTime> existingBurns;

    protected override void Start()
    {
        existingBurns = new List<Burn_DamageOverTime>();

        base.Start();
        CheckForExistingDoT();
    }

    protected override void CheckForExistingDoT()
    {
        int totalBurns = 0;

        for(int i=0; i<transform.parent.childCount; i++)
        {
            Burn_DamageOverTime currBurn = transform.parent.GetChild(i).GetComponent<Burn_DamageOverTime>();

            //Only longer duration if target is already burning
            if(currBurn != null)
            {
                existingBurns.Add(currBurn);
                totalBurns++;
            }
            if(totalBurns > 1)
            {
                existingBurns[0].ResetTimer(); //Reset local duration

                isActive = false;
            }
        }
    }

    protected override void FixedUpdate()
    {
        // base.FixedUpdate();

        if(isActive)
        {
            overallTimer -= Time.deltaTime;
            if(overallTimer <= 0) //+1 to allow for 1s delay
            {
                isActive = false;
                return;
            }

            tickTimer -= Time.deltaTime; //Countdown timer

            if(tickTimer <= 0) //
            {
                DealDamage();
                tickTimer = tickFrequency; //Reset tick timer
            }
        }
        else
        {
            if(endingStatus) return;
            StartCoroutine(EndStatus());
        }
    }

    protected override void DealDamage()
    {
        float damageDealt = UpdateDamage();

        if(enemyCombat != null) enemyCombat.TakeDamageStatus(damageDealt, damageColorIdx);
        if(playerCombat != null) playerCombat.TakeDamage(damageDealt);
        if(enemyCombat == null && playerCombat == null) isActive = false;
    }

    float UpdateDamage()
    {
        float totalDamage = Mathf.FloorToInt(overallTimer / damagePerSecond) * damageIncreasePerTick;
        existingBurns[0].SetDamagePerTick(totalDamage);

        return totalDamage;
    }

    public void SetDamagePerTick(float addDamagePerTick)
    {
        damagePerTick = addDamagePerTick;
        //Damage cannot exceed max
        if(damagePerTick > maxDamagePerTick)
            damagePerTick = maxDamagePerTick;
    }
}
