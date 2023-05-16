using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_ConditionalAugments : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] public float procChance = 1.0f;
    [SerializeField] public float duration;
    [SerializeField] protected bool active;
    [SerializeField] public float buffAmount; //TODO: might need to update this to the AugmentScript values
    [SerializeField] public float buffAmountPercent;
    [SerializeField] protected Base_PlayerCombat playerCombat;
    [Header("Debugging")]
    [SerializeField] protected float durationTimer;
    [SerializeField] protected AugmentScript augmentScript;
    [SerializeField] protected int currentLevel;

    protected virtual void Start()
    {
        playerCombat = GameManager.Instance.PlayerCombat;
        durationTimer = 0;
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
        Debug.Log("Augment Activated");
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
