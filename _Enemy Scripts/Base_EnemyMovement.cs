using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_EnemyMovement : MonoBehaviour
{
    [Header("References/Setup")]
    public Base_Character character;
    [SerializeField] public Rigidbody2D rb;
    public Base_EnemyCombat combat;

    public float moveSpeed;

    [Header("State Variables")]
    //public bool isGrounded; //Use raycast.IsGrounded() instead
    public bool canMove = true;
    [SerializeField] bool canFlip;
    public bool isFacingRight = true;

    private void Awake()
    {
        if(character != null)
            moveSpeed = character.Base_MoveSpeed;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        canFlip = true;
    }

    void Update()
    {
        if (!combat.isAlive) return;
        if (combat.isKnockedback) return;
        Flip();
    }

    private void FixedUpdate()
    {
        if (!combat.isAlive || combat.isStunned || !canMove)
        {
            if(!combat.isKnockedback)
                DisableMove();

            return;
        }
    }

    void Flip()
    {
        if (!canFlip) return;
        if(isFacingRight && rb.velocity.x < 0 || !isFacingRight && rb.velocity.x > 0)
        {
            isFacingRight = !isFacingRight;
           
            if(isFacingRight) transform.localRotation = Quaternion.Euler(0, 0, 0);
            else transform.localRotation = Quaternion.Euler(0, 180, 0);

            ManualFlip();
        }
    }

    void ManualFlip()
    {
        //Flipping healthbar so it remains in correct orientation as character sprite flips
        if (isFacingRight) combat.healthbarTransform.localRotation = Quaternion.Euler(0, 0, 0);
        else combat.healthbarTransform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    public void MoveRight(bool moveRight)
    {
        if (!canMove) return;

        if (moveRight)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            //Flip();
        }
        else
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
            //Flip();
        }
        Flip();
    }

    public void DisableMove()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public void ToggleFlip(bool toggle)
    {
        canFlip = toggle;
    }
}
