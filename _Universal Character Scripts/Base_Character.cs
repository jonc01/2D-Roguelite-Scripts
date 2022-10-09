using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Character")]
public class Base_Character : ScriptableObject
{
    public string Name;

    public enum CharType { Friendly, Enemy, Neutral }
    [SerializeField] CharType Type;

    public enum CharTier { T0, T1, T2, T3, T4, Boss }
    [SerializeField] CharTier Tier;

    [Header("= Base Stats =")]
    public float Base_MaxHP = 50;
    public float Base_MoveSpeed = 3;
    public float Base_JumpHeight = 8;
    public float Base_AttackDamage = 5;
    public float Base_AttackSpeed = 1f;
    public float Base_Defense = 0;
    public float Base_CritChance;
    public float Base_CritMultiplier;
    public float Base_AttackRange = .3f;
    public float Base_KnockbackStrength = 2f;
}
