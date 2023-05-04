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
    public int BuffedStat;
    public float buffedAmount;
    public float buffedAmountPercent;
    public int DebuffedStat;
    public float debuffedAmount;
    public float debuffedAmountPercent;
    [Header("Icons")]
    [SerializeField] public Sprite Icon_Image;

    [Header("Display - Set by reference")]
    [SerializeField] public string Name;
    [Multiline(10)]
    [SerializeField] public string Description;


    void Awake()
    {
        // if(Description == null) Description = GetComponentInChildren<TextMeshProUGUI>();
        if(augmentScrObj == null) return;
        AugmentLevel = augmentScrObj.MinLevel; //Min level is determined by its tier;
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
        Description = augmentScrObj.Description;
        BuffedStat = (int)augmentScrObj.BuffedStat;
        buffedAmount = augmentScrObj.StatIncrease;
        Debug.Log("Augment Stats loaded");
        // DebuffedStat = augmentScrObj. //TODO: might just use "modifiedStat", then use + or - for changes
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
        // if(AugmentLevel >= augmentScrObj.MaxUpgradeLevel) return;
        int scaledLevel = (AugmentLevel - 1);
        //Upgrade stats
        if(buffedAmount != 0) buffedAmount += scaledLevel;
        if(buffedAmountPercent != 0f) buffedAmountPercent += scaledLevel * .02f;
        if(debuffedAmount != 0) debuffedAmount -= scaledLevel;
        if(debuffedAmountPercent != 0f) debuffedAmountPercent += scaledLevel * .02f;

        //TODO: update description
    }

#endregion

}
