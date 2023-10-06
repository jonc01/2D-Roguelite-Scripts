using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_IncreasedDamage : Base_ConditionalAugments
{
    [Header("= CA_IncreaseAttackSpeed =")]
    [SerializeField] float baseAttackDamage; //return to this value after effect ends
    [SerializeField] float buffedAttackDamage; //combat.attackSpeed is set to this value for the duration
    
    protected override void Start()
    {
        base.Start();
        baseAttackDamage = GameManager.Instance.AugmentInventory.base_AttackDamage;
    }

    void FixedUpdate()
    {
        if(!active) return;
        if(timerDuration <= 0) StopBuff();
        timerDuration -= Time.fixedDeltaTime;
        // Time.deltaTime
    }

    protected override void Activate()
    {
        base.Activate(); //sets values
        if(active)
        {
            timerDuration = buffDuration; //Refresh duration
            return;
        }
        
        active = true;

        buffedAttackDamage = baseAttackDamage + buffAmount;
        
        timerDuration = buffDuration;
        playerCombat.attackDamage = buffedAttackDamage;
        
    }

    protected virtual void StopBuff()
    {
        active = false;
        playerCombat.attackDamage = baseAttackDamage;
    }
}
