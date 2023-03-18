using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Inventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Base_PlayerCombat combat;
    [SerializeField] Base_PlayerMovement movement;

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

    [Header("Augments")]
    [SerializeField] private List<GameObject> AugmentsOwned;

    void Awake()
    {
        AugmentsOwned = new List<GameObject>();
        
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

#region Augments
//Public calls
    public void AddAugment(GameObject obj)
    {
        //Get current Augment index
        //Add to UI bar, also a list, place in next open visual slot
    }

    public void RemoveAugment(GameObject obj)
    {

    }
//Private calls
    private void NewAugment()
    {
        
    }

#endregion

}
