using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class Archer_EnemyController : Base_EnemyController
{
    [Header("= Archer =")]
    [SerializeField] float lungeDist = .3f;
    public float distanceToPlayer;
    // public bool backToWall;
    public bool attackClose;
    public bool attackMain;
    public bool playerToRight;
    public bool playerInFront;


    protected override void AttackCheckClose()
    {
        if (!PlatformCheck()) return;
        if (raycast.playerInRangeClose)
        {
            StartCoroutine(LungeAttack());
        } 
    }

    IEnumerator LungeAttack()
    {
        LungeCheck();
        Debug.Log("LUNGING");
        yield return new WaitForSeconds(.2f);
        Debug.Log("shooting after");
        combat.AttackFar();
    }

    public void LungeCheck(float lungeStrength = 4f, float duration = .3f)
    {
        // Increased lunge strength to catch Player if too far
        if(distanceToPlayer > 1.6f) lungeStrength = distanceToPlayer*1.8f;
        movement.ToggleFlip(false);
        
        if(raycast.backToWall)
        { //Back to wall and Player is close behind/under the Boss
            if(attackClose || attackMain)
            {
                lungeStrength += 2f;
                //Lunge away then flip to Attack
                LungeStart(movement.isFacingRight, lungeStrength, duration);
            }else{
                lungeStrength += 2f;
                LungeStart(playerToRight, lungeStrength, duration);
            }
        }
        else
        {
            if(!playerInFront)
            { //Player is behind Boss
                //Player is either close or in range
                if(attackClose || attackMain)
                {
                    lungeStrength += 2f;
                    LungeStart(!playerToRight, lungeStrength, duration);
                }
                else{
                    LungeStart(playerToRight, lungeStrength, duration);
                }
            }
            else
            { //Player is in front
                if(attackClose)
                {
                    // lungeStrength += 1f;

                    //Player is too close, lunge backwards, lungeStrength based on Attack
                    // LungeStart(!playerToRight, lungeStrength, duration);
                }
                //Player is out of normal attack range, lunge forward
                else if(!attackMain && !attackClose)
                {
                    // LungeStart(playerToRight, lungeStrength, duration);
                }
                // else attackMain, don't move
            }
        }
        ManualFlip(playerToRight);
    }

    void LungeStart(bool lungeToRight, float strength = 4f, float duration = .3f)
    {
        movement.Lunge(lungeToRight, strength, duration);
    }

    void ManualFlip(bool faceRight)
    {
        movement.ManualFlip(faceRight);
        movement.ToggleFlip(false);
    }
}
