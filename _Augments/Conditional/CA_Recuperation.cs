using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_Recuperation : Base_ConditionalAugments
{
    [Space(20)]
    [Header("Recuperation (set to buffedAmount)")]
    [SerializeField] float healAmount = 4;

    protected override void Start()
    {
        base.Start();
        healAmount = augmentScript.buffedAmount;
    }

    protected override void Activate()
    {
        // base.Activate();
        playerCombat.HealPlayer(healAmount);
    }
}
