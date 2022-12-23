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
    protected float animEndingTime;

    [Header("Debugging")]
    public bool playerHit;
    public bool canAttack;

    protected virtual void Start()
    {
        if (combat == null) combat = GetComponent<Base_EnemyCombat>();
        if (movement == null) movement = GetComponent<Base_EnemyMovement>();
        playerHit = false;
        animEndingTime = fullAnimTime - animDelay;
    }

    public virtual void Attack()
    {

    }
}
