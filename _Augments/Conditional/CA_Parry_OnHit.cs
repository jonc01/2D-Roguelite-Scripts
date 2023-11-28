using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_Parry_OnHit : Base_ConditionalAugments
{
    [Header("- Parry -")]
    [SerializeField] float onHitAddProcChance = 0;
    [SerializeField] bool procOnHit = false;
    [SerializeField] bool procOnKill = false;

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
        // Debug.Log(name + " Augment activated");
        // base.Activate();
        if(!CanActivate()) return;
        StartProcCooldown();

        if(procOnHit)
            GameManager.Instance.AugmentInventory.OnHit(objectHitPos, onHitAddProcChance);

        if(procOnKill)
            GameManager.Instance.AugmentInventory.OnKill(objectHitPos, onHitAddProcChance);

        //TODO: can't get groundPos using getcomponent
        // Transform groundPos = objectHitPos.GetComponent<IDamageable>().GetGroundPosition();
        // GameManager.Instance.AugmentInventory.OnKill(groundPos);
    }
}
