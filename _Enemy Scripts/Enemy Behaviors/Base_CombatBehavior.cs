using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_CombatBehavior : MonoBehaviour
{
    //Enemy Classes call this function for different melee behavior.
    protected Base_EnemyCombat combat;
    protected Base_EnemyMovement movement;
    protected Base_EnemyRaycast raycast;
    [SerializeField] protected float attackSpeed;

    [Header("Animations")]
    [SerializeField] protected float fullAnimTime;
    [SerializeField] protected float chargeUpAnimDelay;

    [Header("Debugging")]
    [SerializeField] protected float animEndingTime;

    [Header("Debugging")]
    public bool playerHit;
    public bool canAttack;
    [Header("Status")]
    public bool knockbackImmune;

    protected virtual void Start()
    {
        if (combat == null) combat = GetComponent<Base_EnemyCombat>();
        if (movement == null) movement = GetComponent<Base_EnemyMovement>();
        playerHit = false;
        canAttack = true;
        animEndingTime = fullAnimTime - chargeUpAnimDelay;
        if (animEndingTime < 0) animEndingTime = (animEndingTime *= -1); //flip value if negative
        if (raycast == null) raycast = GetComponentInChildren<Base_EnemyRaycast>();
    }

    public virtual void Attack()
    {
        //Placeholder to get overridden
    }
}
