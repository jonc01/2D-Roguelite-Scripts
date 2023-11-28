using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_IncreasedAttackSpeed : Base_ConditionalAugments
{
    //TODO: might turn this code into CA_Base_DurationAugments

    [Header("= CA_IncreaseAttackSpeed =")]
    [SerializeField] float baseAttackSpeed; //return to this value after effect ends
    [SerializeField] float buffedAttackSpeed; //combat.attackSpeed is set to this value for the duration

    protected override void Start()
    {
        base.Start();
        baseAttackSpeed = GameManager.Instance.AugmentInventory.base_AttackSpeed;
    }

    void FixedUpdate()
    {
        if(!active) return;
        if(timerDuration <= 0) StopBuff();
        timerDuration -= Time.fixedDeltaTime;
        // Time.deltaTime
    }

    public override void UpdateLevelStats()
    {
        base.UpdateLevelStats();
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

        //Calculating the buffedAttackSpeed, need to reset in case of augment level change
        
        buffedAttackSpeed = baseAttackSpeed - (baseAttackSpeed * buffAmountPercent); //percent is calculated in AugmentScript
        
        timerDuration = buffDuration;
        playerCombat.attackSpeed = buffedAttackSpeed; //TODO: need to adjust amount, probably increase Tier to Epic or Legendary
        

        //TODO: !! should be setting to buffedAmount after percent, is currently stacking the buff to then reach 0 attack speed delay

        if(playerCombat.attackSpeed <= 0) playerCombat.attackSpeed = .1f;

        //OR
        // playerCombat.attackSpeed -= buffAmount; //0.05 per level
        // if(playerCombat.attackSpeed <= 0) playerCombat.attackSpeed = .1f; //set a hard cap on fastest attack speed
    }

    protected virtual void StopBuff()
    {
        active = false;
        // Debug.Log("ATTACKSPEED STOPPED");
        playerCombat.attackSpeed = baseAttackSpeed;
    }
}
