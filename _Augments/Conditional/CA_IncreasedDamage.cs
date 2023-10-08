using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_IncreasedDamage : Base_ConditionalAugments
{
    [Header("= CA_IncreaseAttackSpeed =")]
    [SerializeField] float buffedAttackDamage; //combat.attackDamage is set to this value for the duration
    
    protected override void Start()
    {
        base.Start();
        // baseAttackDamage = GameManager.Instance.AugmentInventory.base_AttackDamage;
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
            StartProcCooldown();
            return;
        }
        
        active = true;

        buffedAttackDamage = playerCombat.base_attackDamage + buffAmount;
        
        timerDuration = buffDuration;
        playerCombat.attackDamage = buffedAttackDamage;
    }

    protected virtual void StopBuff()
    {
        active = false;
        playerCombat.attackDamage = playerCombat.base_attackDamage;
    }
}
