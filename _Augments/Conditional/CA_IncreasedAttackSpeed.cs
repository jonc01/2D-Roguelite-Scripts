using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_IncreasedAttackSpeed : Base_ConditionalAugments
{
    //TODO: might turn this code into CA_Base_DurationAugments

    [Header("= CA_IncreaseAttackSpeed =")]
    [SerializeField] float baseAttackSpeed; //return to this value after effect ends
    [SerializeField] float buffedAttackSpeed;

    protected override void Start()
    {
        base.Start();
        baseAttackSpeed = GameManager.Instance.AugmentInventory.base_AttackSpeed;
    }

    void FixedUpdate()
    {
        if(!active) return;
        if(durationTimer <= 0) StopBuff();
        Debug.Log("ATTACK SPEED IS WORKING: " + playerCombat.attackSpeed);
        durationTimer -= Time.fixedDeltaTime;
        // Time.deltaTime
    }
    
    protected override void Activate()
    {
        active = true;
        // buffedAttackSpeed = baseAttackSpeed * buffAmountPercent;
        // base.Activate();
        durationTimer = duration; 
        playerCombat.attackSpeed *= buffAmountPercent; //TODO: need to adjust amount, probably increase Tier to Epic or Legendary
        if(playerCombat.attackSpeed <= 0) playerCombat.attackSpeed = .1f;

        //OR
        // playerCombat.attackSpeed -= buffAmount; //0.05 per level
        // if(playerCombat.attackSpeed <= 0) playerCombat.attackSpeed = .1f; //set a hard cap on fastest attack speed
    }

    protected virtual void StopBuff()
    {
        active = false;
        Debug.Log("ATTACKSPEED STOPPED");
        playerCombat.attackSpeed = baseAttackSpeed;
    }
}
