using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_ConditionalAugments : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] public float procChance = 1.0f; //default 100%, 0.0 - 1.0f
    [SerializeField] public float buffDuration;
    [SerializeField] protected bool active;
    [SerializeField] public float buffAmount; //TODO: need to update this to the AugmentScript values
    [SerializeField] public float buffAmountPercent;
    [SerializeField] protected Base_PlayerCombat playerCombat;

    [Header("Proc Cooldown")]
    [SerializeField] protected float procCooldown = 0;

    [Header("Debugging")]
    [SerializeField] protected float timerDuration;
    [SerializeField] protected AugmentScript augmentScript;
    [SerializeField] protected int currentLevel;
    [SerializeField] private float procCooldownTimer;

    protected virtual void Start()
    {
        playerCombat = GameManager.Instance.PlayerCombat;
        timerDuration = 0;
        active = false;
        if(augmentScript == null) augmentScript = GetComponent<AugmentScript>();
        if(augmentScript != null) procChance = augmentScript.procChance;

        procCooldownTimer = 0;
    }

    protected virtual void Update()
    {
        if(procCooldownTimer > 0) procCooldownTimer -= Time.deltaTime;
    }

    public bool CanActivate()
    {
        return procCooldownTimer <= 0;
    }

    public void StartProcCooldown()
    {
        procCooldownTimer = procCooldown;
    }

    public virtual void TriggerAugment(float addProcChance = 0)
    {
        float rand = Random.Range(0f, 1.0f);
	    if(rand < (procChance + addProcChance))
            Activate();
    }
    
    public virtual void TriggerAugment(Transform objectHitPos, float addProcChance = 0)
    {
        float rand = Random.Range(0f, 1.0f);
	    // if(rand < procChance) Activate(objectHitPos);
        if(rand < (procChance + addProcChance))
            Activate(objectHitPos);
    }

    protected virtual void Activate()
    {
        // Debug.Log(name + " Augment Activated");
        if(!CanActivate()) return;
        StartProcCooldown();

        GameManager.Instance.AugmentInventory.augmentInventoryDisplay.ToggleAugmentStatus(augmentScript.displayedIndex, buffDuration);
    }

    protected virtual void Activate(Transform objectHitPos)
    {
        //Placeholder for other scripts to override
        Activate();
    }

    public virtual void UpdateLevelStats()
    {
        //Call from AugmentScript
        // if(!LevelChanged()) return; //only update if needed
        if(augmentScript == null) return;
        buffAmount = augmentScript.buffedAmount;
        buffAmountPercent = augmentScript.buffedAmountPercent * .01f; //Convert to %
    }

    public virtual void SetConditionalAugmentStats()
    {
        //placeholder for override
    }
}
