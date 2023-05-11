using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentScript : MonoBehaviour
{
    [Header("Scriptable Object Reference")]
    [SerializeField] public Augment augmentScrObj;

    [Header("Stats from Scriptable Object")]
    public int Tier; //0: Common, 1: Rare, 2: Epic, 3: Legendary, 4: Overcharged, 5: Unstable
    public int AugmentLevel; //Randomized in AugmentSelectMenu, 1-5
    public int MaxLevel = 5;
    public int BuffedStat;
    public float buffedAmount;
    public float buffedAmountPercent;
    public int DebuffedStat;
    public float debuffedAmount; //TODO: might not use, just use a negative value for buffedAmount
    public float debuffedAmountPercent; //TODO: might not use, just use a negative value for buffedAmount
    [Header("Icons")]
    [SerializeField] public Sprite Icon_Image;

    [Header("Display - Set by reference")]
    [SerializeField] public string Name;
    [Multiline(10)]
    [SerializeField] public string Description;

    // Base amounts (before level stats)
    [Header("Debugging")]
    [SerializeField] private string baseDescription;
    [SerializeField] private float baseBuffedAmount;
    [SerializeField] private float baseBuffedAmountPercent;


    void Awake()
    {
        // if(Description == null) Description = GetComponentInChildren<TextMeshProUGUI>();
        if(augmentScrObj == null) return;
        MaxLevel = augmentScrObj.MaxLevel; //Max level is determined by its tier;
        GetAugmentVariables();
    }

    void Start()
    {

    }

    private void GetAugmentVariables()
    {
        if(augmentScrObj == null) { Debug.Log("No Augment Scriptable Object referenced!"); return; }
        Name = augmentScrObj.Name;
        Icon_Image = augmentScrObj.AugmentIcon;
        baseDescription = augmentScrObj.Description;
        BuffedStat = (int)augmentScrObj.BuffedStat;
        buffedAmount = augmentScrObj.StatIncrease;
        Debug.Log("Augment Stats loaded");
        baseBuffedAmount = buffedAmount;
        // baseBuffedAmountPercent = 
        // DebuffedStat = augmentScrObj. //TODO: might just use "modifiedStat", then use + or - for changes
        UpdateDescription();
    }
//
 

    public void UpdateLevel(int level)
    {
        AugmentLevel = level;
        UpdateStatsToLevel();
    }


#region Change Stats based on Level
    private void UpdateStatsToLevel()
    {
        ResetStats();

        // if(AugmentLevel >= augmentScrObj.MaxUpgradeLevel) return;
        int scaledLevel = (AugmentLevel - 1);
        //Upgrade stats
        if(buffedAmount != 0) buffedAmount += scaledLevel;
        if(buffedAmountPercent != 0f) { buffedAmountPercent += scaledLevel * .02f; Debug.Log("buffedAmountPercent not setup!");}
        if(debuffedAmount != 0) debuffedAmount -= scaledLevel;
        if(debuffedAmountPercent != 0f) debuffedAmountPercent += scaledLevel * .02f;

        UpdateDescription();
    }

    private void UpdateDescription()
    {
        Description = buffedAmount.ToString() + " " + baseDescription;
    }

    private void ResetStats()
    {
        buffedAmount = baseBuffedAmount;
        buffedAmountPercent = baseBuffedAmountPercent;
        debuffedAmount = 0;
        debuffedAmountPercent = 0;
    }

#endregion

}
