using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;

[CreateAssetMenu(menuName = "Scriptable Objects/Augment")]
public class Augment : ScriptableObject
{
    public int AugmentID;
    // public Color32 TierColor;

    public enum _Tier { Common, Rare, Epic, Legendary, Overcharged, Unstable };
    // public int Tier = 0; //0: Common, 1: Rare, 2: Epic, 3: Legendary, 4: Overcharged, 5: Unstable
    public _Tier Tier; //0: Common, 1: Rare, 2: Epic, 3: Legendary, 4: Overcharged, 5: Unstable
    public int MinLevel = 1; //TODO: might not use this system
    public Sprite AugmentIcon;
    public string Name;
    [Multiline(10)]
    public string Description;

    [Header("= Buffed Stats =")]
    public float StatIncrease;
    public enum _IncreaseType { Flat, Percent };
    public _IncreaseType IncreaseType;
    public enum _BuffedStat { 
        Health, Defense, MoveSpeed, 
        AttackDamage, AttackSpeed, 
        CritChance, CritMultiplier };
    public _BuffedStat BuffedStat;
    //TODO: figure out how to buff the specified player stat//needs testing, might work
    
    [Header("= Debuffed Stats =")]
    public float StatDecrease;
    public enum _DecreaseType { Flat, Percent };
    public _DecreaseType DecreaseType;
    public enum _DebuffedStat { 
        Health, Defense, MoveSpeed, 
        AttackDamage, AttackSpeed, 
        CritChance, CritMultiplier };
    public _DebuffedStat DebuffedStat;

    //Example:
        //FlatIncrease: 2
        //BuffedStat: 3
    //BuffedStat[3] += 2;
    //Translated: +2 to AttackDamage



}
