using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColossalBoss_Controller : Base_BossController
{



    protected override void AttackCheck()
    {
        // base.AttackCheck();
        //TODO: only keep CheckPlayerDetect if using trigger colliders to initiate attacks
        if (combat.currAttackIndex == 1 || playerDetect.CheckPlayerDetect(combat.currAttackIndex))
            combat.Attack(combat.currAttackIndex);
    }

    protected override void ChasePlayer()
    {
        base.ChasePlayer();
        
    }

    protected override void FlipDir()
    {
        base.FlipDir();
        //TODO: Add delay to flip, play animation

        //Base code
        // bool dir = !movement.isFacingRight;
        // movement.MoveRight(dir);
    }

    // void LedgeWallCheck()
    // {
    //     if (combat.isAttacking) return;
    //     if (!raycast.ledgeDetect || raycast.wallDetect)
    //         //Spin/float towards player
    // }
}
