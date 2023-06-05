using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_PlayerController : MonoBehaviour
{
    [Header("References")]
    public Base_PlayerMovement movement;
    public Base_PlayerCombat combat;

    //TODO: Allow rebinding of keys in settings
    

    void Update()
    {
        //MOVEMENT
        if(movement != null && movement.allowInput)
        {
            //Keeping Jump in Movement Script, to allow variable jump heights.
            /*if (Input.GetButtonDown("Jump"))
            {
                movement.Jump();
            }
            
            if (Input.GetButtonUp("Jump"))
            {
                movement.Jump(true);
            }*/

            if (Input.GetButtonDown("Horizontal"))
            {
                movement.Move(false);
            }
            if (Input.GetButtonDown("Horizontal"))
            {
                movement.Move(true);
            }

            if (Input.GetButtonDown("Dodge"))
            {
                if(movement.canDash) movement.StartDash();
            }
        }

        //COMBAT
        if(combat != null && combat.allowInput)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if(movement.IsGrounded()) combat.Attack1(); //isGrounded
                else combat.Attack2();
            }
        }

        if(combat != null && combat.allowInput)
        {
            if(Input.GetButtonDown("Fire2"))
            {
                if(movement.IsGrounded()) combat.Block();
            }
        }
    }
}
