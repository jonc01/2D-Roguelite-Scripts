using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbController_Heal : OrbController
{

    [Space(20)]
    [Header("- Heal Orb override -")]
    [SerializeField] Base_PlayerCombat playerCombat;
    [SerializeField] float healAmount = 2;

    // [SerializeField] private bool playerInRange = false;

    protected override void Awake()
    {
        base.Awake();
        // playerInRange = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        playerCombat = GameManager.Instance.PlayerCombat;
    }

    //HitPlayer
    protected override void HitPlayer()
    {
        if(hitDone) return;
        GiveHeal(); //Heal Player

        Invoke("DisableVelocity", .25f);

        animator.Play("PuffOfSmoke");
        Invoke("DestroyObject", 0.67f); //Delay to play animation
    }

    private void GiveHeal()
    {
        if(xpGiven) return;
        xpGiven = true;
        playerCombat.HealPlayer(healAmount);
    }

    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     var playerCollider = collision.GetComponent<Base_PlayerCombat>();
    //     if(playerCollider == null) return;

    //     playerInRange = true;
    //     StartCoroutine(MoveToPlayer());
    // }
}