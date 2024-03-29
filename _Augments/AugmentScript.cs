using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering;

public class AugmentScript : MonoBehaviour
{
    [Header("Scriptable Object Reference")]
    [SerializeField] public Augment augmentScrObj;

    [Header("Stats from Scriptable Object")]
    public int Tier; //0: Common, 1: Rare, 2: Epic, 3: Legendary, 4: Overcharged, 5: Unstable
    public int AugmentType;
    public Base_ConditionalAugments ConditionalAugmentScript;
    public int AugmentLevel; //Randomized in AugmentSelectMenu, 1-5
    public int MaxLevel = 5;
    public int BuffedStat;
    public string[] BuffedStatName;
    public int increaseType; //Flat, Percent
    public float buffedAmount;
    public float buffedAmountPercent;
    public float buffAmountPerLevel; // -----------------
    public int DebuffedStat;
    public float debuffedAmount; //TODO: might not use, just use a negative value for buffedAmount
    public float debuffedAmountPercent; //TODO: might not use, just use a negative value for buffedAmount
    [Space(10)]
    public bool isConditional;
    public float procChance;
    public int displayedIndex = 0;

    // [Header("Conditional Stats")]
    // public float conditionalBuffedAmount;
    // public float conditionalBuffedAmountPercent;

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
        if(augmentScrObj == null)
        {
            Debug.Log(name + " No AugmentScrObj");
            return;
        } 
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
        Tier = (int)augmentScrObj.Tier;

        //
        baseDescription = augmentScrObj.Description;
        AugmentType = (int)augmentScrObj.AugmentType;
        ConditionalAugmentScript = GetComponent<Base_ConditionalAugments>();
        increaseType = (int)augmentScrObj.IncreaseType;
        BuffedStat = (int)augmentScrObj.BuffedStat;
        BuffedStatName = augmentScrObj.BuffedStat.ToString().Split('_');

        if(increaseType == 0) buffedAmount = augmentScrObj.StatIncrease;
        else buffedAmountPercent = augmentScrObj.StatIncrease;

        buffAmountPerLevel = augmentScrObj.StatIncreasePerLevel;

        baseBuffedAmount = buffedAmount;
        baseBuffedAmountPercent = buffedAmountPercent;

        isConditional = augmentScrObj.isConditional;
        procChance = augmentScrObj.procChance;
        // baseBuffedAmountPercent = 
        // DebuffedStat = augmentScrObj. //TODO: might just use "modifiedStat", then use + or - for changes
        UpdateLevel(AugmentLevel);
        UpdateConditional();
        UpdateDescription();
        // Debug.Log("Augment Stats loaded");
    }
//
    private void UpdateConditional()
    {
        if(ConditionalAugmentScript == null || augmentScrObj == null) return;

        //Update proc chance with current level
        procChance = augmentScrObj.procChance + ((AugmentLevel - 1) * augmentScrObj.procChancePerLevel);

        if(isConditional) ConditionalAugmentScript.UpdateLevelStats();
    }

    public void UpdateLevel(int level)
    {
        if(level > MaxLevel) AugmentLevel = MaxLevel;
        else AugmentLevel = level;

        UpdateStatsToLevel();
    }

#region Augment Added to Inventory
    public void AddConditionalAugment()
    {
        if(ConditionalAugmentScript == null) return;
        ConditionalAugmentScript.SetConditionalAugmentStats();
    }
#endregion

#region Change Stats based on Level
    private void UpdateStatsToLevel()
    {
        ResetStats();

        // if(AugmentLevel >= augmentScrObj.MaxUpgradeLevel) return;
        float scaledStat = (AugmentLevel-1) * buffAmountPerLevel; //Ex: Level 1: +0, Level 2: +1

        //Upgrade stats
        if(increaseType == 0) buffedAmount += scaledStat;
        else buffedAmountPercent = baseBuffedAmountPercent + scaledStat;

        //TODO: not using yet, needs setup
        //TODO: for augments that update multiple stats, might just use an array, loop through array here
        //TODO: if using array^ just use buffedAmount, and use positive or negative values
        if(debuffedAmount != 0) debuffedAmount -= scaledStat;
        if(debuffedAmountPercent != 0f) debuffedAmountPercent += scaledStat * .01f;

        UpdateConditional();
        UpdateDescription();
    }

    public void UpdateDescription(bool random = false)
    {
        if(augmentScrObj == null) return;

        string divider = "";
        float stat;
        string statType = "";// = BuffedStatName;
        //Separates Stat names if a space is needed
        foreach (string statsName in BuffedStatName) {
            statType += statsName.ToString();
            statType += " ";
        }

        //Display if stat is a flat damage boost or percent
        if(increaseType == 0) //[0] Flat
        {
            stat = buffedAmount;
        }
        else if(increaseType == 2) //[2] Conditional
        {
            //Note: this is used by augments that only have a conditional
            stat = buffedAmount;
            // stat = conditionalBuffedAmount;
        }
        else //[1] Percent
        {
            stat = buffedAmountPercent;
            divider = "%";
        }
        
        //Example:
        // Increased Damage and Attack speed
        // +5 Attack
        // +10% Attack Speed

        if(augmentScrObj != null)
        {
            string tempDesc; //Initial description before setting as main
            if(random)
            {
                string buffedDesc = "?" + divider;
                tempDesc = baseDescription.Replace('#'.ToString(), buffedDesc);

                if(isConditional)
                {
                    Description = tempDesc.Replace('$'.ToString(), '?'.ToString());
                }
                else
                {
                    Description = tempDesc;
                }
            }else{
                string buffedDesc = stat.ToString() + divider;
                tempDesc = baseDescription.Replace('#'.ToString(), buffedDesc);

                if(isConditional)
                {
                    float procDesc = procChance*100f;
                    Description = tempDesc.Replace('$'.ToString(), procDesc.ToString("N0")); 
                }
                else
                {
                    Description = tempDesc;
                }
            }
        }
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
