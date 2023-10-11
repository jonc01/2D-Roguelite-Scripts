using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_OnHit_OnKill : Base_ConditionalAugments
{
    [Header("- Proc On-Kill with On-Hit -")]
    [SerializeField] float onHitAddProcChance = 0; //additional proc chance, default still applies
    [SerializeField] float newAttackDamage = 1;
    
    protected override void Start()
    {
        base.Start();
        onHitAddProcChance = procChance;
        //Parry has 100% proc, but still rolls normally for OnHit or OnKill augments
    }

    public override void TriggerAugment(Transform objectHitPos, float addProcChance = 0)
    {
        // base.TriggerAugment(objectHitPos, addProcChance);
        //Guaranteed proc
        Activate(objectHitPos);
    }

    protected override void Activate(Transform objectHitPos)
    {
        // base.Activate();
        if(!CanActivate()) return;

        Base_EnemyCombat enemyHit = objectHitPos.GetComponent<Base_EnemyCombat>();
        Transform groundOffset;
        if(enemyHit != null) groundOffset = enemyHit.GetGroundPosition();
        else groundOffset = objectHitPos;

        StartProcCooldown();
        GameManager.Instance.AugmentInventory.OnKill(groundOffset, onHitAddProcChance);   
    }

    public override void SetConditionalAugmentStats()
    {
        base.SetConditionalAugmentStats();
        
        //Only called once the Augment is purchased
        //Called after updating stats from Augments
        playerCombat.base_attackDamage = newAttackDamage;
        playerCombat.attackDamage = newAttackDamage;

        playerCombat.maxHP -= 20;
        playerCombat.HealPlayer(0); //update current health
    }
}
