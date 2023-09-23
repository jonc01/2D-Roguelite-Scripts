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
    [Header("Debugging")]
    [SerializeField] protected float timerDuration;
    [SerializeField] protected AugmentScript augmentScript;
    [SerializeField] protected int currentLevel;

    protected virtual void Start()
    {
        playerCombat = GameManager.Instance.PlayerCombat;
        timerDuration = 0;
        active = false;
        if(augmentScript == null) augmentScript = GetComponent<AugmentScript>();
    }

    public virtual void TriggerAugment()
    {
        float rand = Random.Range(0f, 1.0f);
	    if(rand < procChance) Activate();
    }

    protected virtual void Activate()
    {
        Debug.Log(name + " Augment Activated");

        GameManager.Instance.AugmentInventory.augmentInventoryDisplay.ToggleAugmentStatus(augmentScript.displayedIndex, buffDuration);
    }

//

    public virtual void TriggerAugment(Transform objectHitPos)
    {
        float rand = Random.Range(0f, 1.0f);
	    if(rand < procChance) Activate(objectHitPos);
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
}
