using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extend_DamageOverTime : Base_DamageOverTime
{
    [Header("- Extend -")]
    [SerializeField] private float extendDuration = .33f; //Increases full duration
    [SerializeField] private float extendTimer = .5f; //Extends timer
    // [SerializeField] List<Base_DamageOverTime> currentDoTs;
    
    protected override void Start()
    {
        // currentDoTs = new List<Base_DamageOverTime>();

        base.Start();
        CheckForExistingDoT();
    }

    protected override void CheckForExistingDoT()
    {
        for(int i=0; i<transform.parent.childCount; i++)
        {
            Base_DamageOverTime existingDoT = transform.parent.GetChild(i).GetComponent<Base_DamageOverTime>();

            if(existingDoT != null)
            {
                //Extend base duration of current status
                existingDoT.AddToBaseDuration(extendDuration);
                existingDoT.ExtendTimer(extendTimer);
            }
        }

    }
}
