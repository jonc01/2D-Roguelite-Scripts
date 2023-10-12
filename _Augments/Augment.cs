using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;

[CreateAssetMenu(menuName = "Scriptable Objects/Augment")]
public class Augment : ScriptableObject
{
    public int AugmentID;
    // public Color32 TierColor;

    public enum _Tier { Common, Rare, Epic, Legendary, Unstable };
    // public int Tier = 0; //0: Common, 1: Rare, 2: Epic, 3: Legendary, 4: Overcharged, 5: Unstable
    public _Tier Tier; //0: Common, 1: Rare, 2: Epic, 3: Legendary, 4: Overcharged, 5: Unstable
    // public int MaxLevel = 5; 

    public enum _AugmentType { 
        Normal, OnKill, OnDamageTaken, 
        OnHit, OnParry, OnRoomClear };
    public _AugmentType AugmentType;

    // public enum _MaxLevel { Level1 = 1, Level2 = 2, Level3 = 3, Level4 = 5, Level5 = 5};
    // public _MaxLevel MaxLevel;
    public int MaxLevel = 5;

    public Sprite AugmentIcon;
    public string Name;
    [Multiline(10)]
    public string Description;

    [Header("= Buffed Stats =")]
    public float StatIncrease;
    public float StatIncreasePerLevel;
    public enum _IncreaseType { Flat, Percent, Misc };
    public _IncreaseType IncreaseType;
    public enum _BuffedStat { 
        Health, Defense, Move_Speed, 
        Attack, Attack_Speed, 
        Crit_Chance, Crit_Multiplier };
    public _BuffedStat BuffedStat;
    
    [Header("= Debuffed Stats =")]
    public float StatDecrease;
    public enum _DecreaseType { Flat, Percent };
    public _DecreaseType DecreaseType;
    public enum _DebuffedStat { 
        Health, Defense, MoveSpeed, 
        AttackDamage, AttackSpeed, 
        CritChance, CritMultiplier };
    public _DebuffedStat DebuffedStat;

    [Header("= Conditional Augments =")]
    public bool isConditional;
    // [Range(0.01f, 1.0f)]
    [Header("[0.01  -  1.0]")]
    public float procChance;
    // [Range(0.01f, 1.0f)]
    public float procChancePerLevel;

    // [Space(10)]
    // [Header("== Conditional Augments (Only AugmentType != 0) ==")]
    // public float conditionalBuffAmount;



    //Example:
        //FlatIncrease: 2
        //BuffedStat: 3
    //BuffedStat[3] += 2;
    //Translated: +2 to AttackDamage



}
