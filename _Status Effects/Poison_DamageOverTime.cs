using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poison_DamageOverTime : Base_DamageOverTime
{
    [Header("- Poison -")]
    [SerializeField] private float extendTimer = 2;
    [SerializeField] List<Poison_DamageOverTime> existingPoisons;

    protected override void Start()
    {
        existingPoisons = new List<Poison_DamageOverTime>();

        base.Start();
        CheckForExistingDoT();
    }

    protected override void CheckForExistingDoT()
    {
        int totalPoisons = 0;

        for(int i=0; i<transform.parent.childCount; i++)
        {
            Poison_DamageOverTime existingPoison = transform.parent.GetChild(i).GetComponent<Poison_DamageOverTime>();

            //Only longer duration if target is already poisoned
            if(existingPoison != null)
            {
                existingPoisons.Add(existingPoison);
                totalPoisons++;
            }
            if(totalPoisons > 1)
            {
                existingPoisons[0].overallTimer += extendTimer;
                isActive = false;
            }
        }
    }

    public override void ExtendTimer(float timeAdded)
    {
        overallTimer += timeAdded;
        //Timer cannot exceed full duration
        // if(overallTimer > duration) overallTimer = duration; //No limit to duration for Poison
    }

    protected override void FixedUpdate()
    {
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
}
