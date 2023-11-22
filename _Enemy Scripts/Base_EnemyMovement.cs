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
    [SerializeField] public bool canFlip;
    public bool isFacingRight = true;
    // bool isLunging;

    //TESTING //TODO: 
    // bool _isKnockedback = false;
    //

    Coroutine LungingCO;


    private void Awake()
    {
        if(character != null)
            moveSpeed = character.Base_MoveSpeed;

        // isLunging = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if(combat == null) combat = GetComponent<Base_EnemyCombat>();
        canFlip = true;
    }

    void Update()
    {
        if (!combat.isAlive) return;
        if (combat.isKnockedback || combat.isLunging) return; // || _isKnockedback) return; //_isKnockedback here to make sure enemy doesn't flip when pushed from knockback
        Flip();
    }

    private void FixedUpdate()
    {
        if (!combat.isAlive || combat.isStunned || !canMove)
        {
            // if(!combat.isKnockedback)// || combat.isLunging)// || _isKnockedback)
            if(!combat.isKnockedback)
                DisableMove();

            return;
        }
    }


    public void MoveRight(bool moveRight)
    {
        // if (!canMove) return; //1
        if (!canMove || combat.isKnockedback) return; //2

        if (moveRight) rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        else rb.velocity = new Vector2(-moveSpeed, rb.velocity.y); 
        
        Flip();
    }

    public virtual void Jump()
    {
        float jumpStrength = 4f;
        Vector2 jumpDir = new Vector2(rb.velocity.x, jumpStrength);
        rb.AddForce(jumpDir, ForceMode2D.Impulse);
    }

    // public virtual void Lunge(bool lungeToRight, float strength = 4, float duration = .3f)
    // {
    //     canMove = false;
    //     //Reversed Knockback, moving towards player instead of backwards
    //     combat.GetKnockback(!lungeToRight, strength, duration);
    // }

    #region Flip
    void Flip(bool overrideFlip = false)
    {
        if (!canFlip && !overrideFlip) return;
        if(isFacingRight && rb.velocity.x < 0 || !isFacingRight && rb.velocity.x > 0)
        {
            isFacingRight = !isFacingRight;
           
            if(isFacingRight) transform.localRotation = Quaternion.Euler(0, 0, 0);
            else transform.localRotation = Quaternion.Euler(0, 180, 0);

            HealthBarFlip();
        }
    }

    void HealthBarFlip()
    {
        if(combat.healthbarTransform == null) return;
        //Flipping healthbar so it remains in correct orientation as character sprite flips
        if (isFacingRight) combat.healthbarTransform.localRotation = Quaternion.Euler(0, 0, 0);
        else combat.healthbarTransform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    public void ManualFlip(bool faceRight)
    {
        isFacingRight = faceRight;
        if(isFacingRight) transform.localRotation = Quaternion.Euler(0, 0, 0);
        else transform.localRotation = Quaternion.Euler(0, 180, 0);
        HealthBarFlip();
    }
    #endregion

    #region Toggles
    public void DisableMove() //controlled when canMove is false
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public void ToggleFlip(bool toggle)
    {
        canFlip = toggle;
    }
    #endregion
}
