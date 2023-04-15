using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentInventory : MonoBehaviour
{
    [Header("References")]
    // [SerializeField] AugmentManager augmentManager;
    [SerializeField] Base_PlayerCombat combat;
    [SerializeField] Base_PlayerMovement movement;
    [Header("====== BASE STATS ======")]
    [Header("=== Base Attack Stats ===")]
    public float base_AttackDamage;
    public float base_AttackSpeed;
    public float base_KnockbackStrength;

    [Header("=== Base Defense Stats ===")]
    public float base_MaxHP;
    public float base_Defense;
    public float base_kbResist;

    [Header("=== Base Movement Stats ===")]
    public float base_MoveSpeed;
    [Space(10)]
    [Header("====== MODIFIED STATS ======")]
    [Header("=== Base Attack Stats ===")]
    public float modified_AttackDamage;
    public float modified_AttackSpeed;
    public float modified_KnockbackStrength;

    [Header("=== Base Defense Stats ===")]
    public float modified_MaxHP;
    public float modified_Defense;
    public float modified_kbResist;

    [Header("=== Base Movement Stats ===")]
    public float modified_MoveSpeed;

    [Header("Augments")]
    [SerializeField] private List<GameObject> heldAugments;

    void Awake()
    {
        heldAugments = new List<GameObject>();
        
        if(combat == null) combat = GetComponentInParent<Base_PlayerCombat>();
        if(movement == null) movement = GetComponentInParent<Base_PlayerMovement>();
        GetBasePlayerStats();
    }


    public void GetBasePlayerStats()
    {
        if(combat == null) return;

        base_AttackDamage = combat.attackDamage;
        base_AttackSpeed = combat.attackSpeed;
        base_KnockbackStrength = combat.knockbackStrength;

        base_MaxHP = combat.maxHP;
        base_Defense = combat.defense;
        base_kbResist = combat.kbResist;

        if(movement == null) return;

        base_MoveSpeed = movement.moveSpeed;
    }

    private void ResetPlayerStats() //OnDeath or on augment wipe
    {
        //Resets certain player stats before adding augments
        combat.attackDamage = base_AttackDamage;
        combat.attackSpeed = base_AttackSpeed;
        combat.knockbackStrength = base_KnockbackStrength;
        combat.maxHP = base_MaxHP;
        combat.defense = base_Defense;
        combat.kbResist = base_kbResist;
        movement.moveSpeed = base_MoveSpeed;
    }

#region Augments
    public void UpdateAugments()
    {
        ResetPlayerStats();
        // for(int i=0; i<augmentManager.activeSlots; i++)
        // {
        //     //augmentManager.Slots[i]. ... //TODO: update player stats
        // }

        
    }

    private void PickUpAugment(AugmentScript augment)
    {
        int statIndex = augment.BuffedStat;
        // int statIndex = augment.DebuffedStat;
        // switch(statIndex)
        // {
        //     case 0: combat.maxHP += augment.
        // }

        //TODO:
        // Either call the code from the augment directly, or do it here with switch cases?
        // 0 = damaage

        switch(statIndex)
        {
            case 0: combat.attackDamage += augment.buffedAmount; break;
            case 1: combat.maxHP += augment.buffedAmount; break;
        }
    }

////////////////
//TODO: might just use augmentManager instead of here

//     public void AddAugment(Augment aug)
//     {
//         //Get current Augment index

//         // UI_AugmentDisplay
//     }

//     public void RemoveAugment(Augment aug)
//     {

//     }
// //Private calls
//     private void NewAugment()
//     {
        
//     }

#endregion

}
