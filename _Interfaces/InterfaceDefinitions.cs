using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    Transform GetPosition();
    void TakeDamage(float damageTaken, bool knockback = false, float strength = 8, float xPos = 0);
    void TakeDamageStatus(float damageTaken);
}

public interface IInteractable
{
    void Interact();
    //void DisplayPrompt();
}