using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentInventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] AugmentInventoryDisplay augmentInventoryDisplay;
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
    [SerializeField] private List<AugmentScript> heldAugments;

    void Awake()
    {
        heldAugments = new List<AugmentScript>();
        
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
        combat.maxHP = base_MaxHP;
        combat.defense = base_Defense;
        movement.moveSpeed = base_MoveSpeed;
        combat.attackDamage = base_AttackDamage;
        combat.attackSpeed = base_AttackSpeed;
        combat.knockbackStrength = base_KnockbackStrength;
        combat.kbResist = base_kbResist;
    }

    private void ModifyPlayerStats()
    {
        //Applies augmented/modified stats
        combat.maxHP += modified_MaxHP;
        combat.defense += modified_Defense;
        movement.moveSpeed += modified_MoveSpeed;
        combat.attackDamage += modified_AttackDamage;
        combat.attackSpeed += modified_AttackSpeed;
        combat.knockbackStrength += modified_KnockbackStrength;
        combat.kbResist += modified_kbResist;
    }

#region Augments
    public void UpdateAugments()
    {
        float tempPlayerHP = combat.currentHP; //Store Player HP in case of max health being reduced
        
        //Reset Player stats before re-applying stat boosts
        ResetPlayerStats();

        for(int i=0; i<heldAugments.Count; i++)
        {
            ApplyAugmentStats(heldAugments[i]);
        }

        ModifyPlayerStats();
        
        //Update health
        if(combat.currentHP < tempPlayerHP) combat.currentHP = combat.maxHP;
        else combat.currentHP = tempPlayerHP;

        combat.HealPlayer(0, false);
    }

    public void AddAugment(AugmentScript augment)
    {
        heldAugments.Add(augment);
        if(augmentInventoryDisplay != null) augmentInventoryDisplay.AddAugmentToDisplay(heldAugments);
        UpdateAugments();
    }

    public void RemoveAugment(AugmentScript augment)
    {
        heldAugments.Remove(augment);
        //TODO: need to return to pool
    }

    private void ApplyAugmentStats(AugmentScript augment)
    {
        int statIndex = (int)augment.BuffedStat; //TODO: check typecast
        // int statIndex = augment.DebuffedStat;

        //TODO:
        // Either call the code from the augment directly, or do it here with switch cases?
        switch(statIndex)
        {
            case 0: modified_MaxHP += augment.buffedAmount; break;
            case 1: modified_Defense += augment.buffedAmount; break;
            case 2: modified_MoveSpeed += augment.buffedAmount; break;
            case 3: modified_AttackDamage += augment.buffedAmount; break;
            case 4: modified_AttackSpeed += augment.buffedAmount; break;
            //case 5: crit chance?
            default: break;
        }
    }

#endregion

}
