using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_CombatBehavior : MonoBehaviour
{
    //Enemy Classes call this function for different melee behavior.
    protected Base_EnemyCombat combat;
    protected Base_EnemyMovement movement;
    [SerializeField] protected float attackSpeed;

    [Header("Animations")]
    [SerializeField] protected float fullAnimTime;
    [SerializeField] protected float animDelay;

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
        animEndingTime = fullAnimTime - animDelay;
        if(animEndingTime < 0) animEndingTime = (animEndingTime *= -1); //flip value if negative
    }

    public virtual void Attack()
    {
        //This is just here to get overridden
    }
}
