using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;

[CreateAssetMenu(menuName = "Scriptable Objects/Augment")]
public class Augment : ScriptableObject
{
    public int AugmentID;
    // public Color32 TierColor;
    public int Tier = 0; //0: Common, 1: Rare, 2: Epic, 3: Legendary, 4: Overcharged, 5: Unstable
    public int MinLevel = 1; //TODO: might not use this system
    public Sprite AugmentIcon;
    public string Name;
    [Multiline(10)]
    public string Description;
    // public enum Tier { Common, Rare, Epic, Legendary, Blessed, Cursed };
    [Header("= Buffed Stats =")]
    public float FlatIncrease;
    public float PercentIncrease;

    public enum BuffedStat { 
        Health, Defense, MoveSpeed, 
        AttackDamage, AttackSpeed, CritChance, CritMultiplier};
    //TODO: figure out how to buff the specified player stat
    
    [Header("= Debuffed Stats =")]
    public float FlatDecrease;
    public float PercentDecrease;
    public enum DebuffedStat { 
        Health, Defense, MoveSpeed, 
        AttackDamage, AttackSpeed, CritChance, CritMultiplier };

    //Example:
        //FlatIncrease: 2
        //BuffedStat: 3
    //BuffedStat[3] += 2;
    //Translated: +2 to AttackDamage



}
