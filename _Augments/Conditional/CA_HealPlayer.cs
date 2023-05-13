using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_HealPlayer : Base_ConditionalAugments
{
    //Heals the Player
    //Add to whichever conditional augment list you want to call this with (Ex: OnKill or OnHit)

    protected override void Activate()
    {
        // base.Activate();
        playerCombat.HealPlayer(buffAmount);
    }
}
