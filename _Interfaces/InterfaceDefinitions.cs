using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damageTaken, bool knockback = false, float strength = 8);
}

public interface IInteractable
{
    void Interact();
    //void DisplayPrompt();
}