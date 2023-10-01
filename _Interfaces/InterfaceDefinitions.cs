using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    Transform GetHitPosition();
    Transform GetGroundPosition();
    void TakeDamage(float damageTaken, bool knockback = false, bool procOnHit = false, float strength = 8, float xPos = 0);
    void TakeDamageStatus(float damageTaken, int colorIdx);
}

public interface IInteractable
{
    void Interact();
    //void DisplayPrompt();
}