using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_BossMovement : MonoBehaviour
{
    [Header("References/Setup")]
    public Base_Character character;
    [SerializeField] public Rigidbody2D rb;
    public Base_BossCombat combat;

    public float moveSpeed;

    [Header("State Variables")]
    //public bool isGrounded; //Use raycast.IsGrounded() instead
    public bool canMove = true;
    [SerializeField] public bool canFlip;
    public bool isFacingRight = true;
    bool isLunging;
    Coroutine LungingCO;

    private void Awake()
    {
        if(character != null)
            moveSpeed = character.Base_MoveSpeed;

        isLunging = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if(combat == null) combat = GetComponent<Base_BossCombat>();
        canFlip = true;
    }

    void Update()
    {
        if (!combat.isAlive) return;
        if (isLunging) return;
        Flip();
    }

    private void FixedUpdate()
    {
        if (!combat.isAlive || combat.isStunned || !canMove)
        {
            if(isLunging) DisableMove();
            return;
        }
    }


    public void MoveRight(bool moveRight)
    {
        if (!canMove) return;

        if (moveRight) rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        else rb.velocity = new Vector2(-moveSpeed, rb.velocity.y); 
        
        Flip();
    }

    public virtual void Lunge(bool lungeToRight, float strength = 4, float duration = .3f)
    {
        canMove = false;
        //Reversed Knockback, moving towards player instead of backwards
        GetKnockback(!lungeToRight, strength, duration);
    }

    // public void LungeAlt(bool lungeToRight, float strength = 8, float duration = .2f)
    // {
    //     canMove = false;
    //     //Needs to use '!' to lunge towards player
    //     GetKnockback(!lungeToRight, 8f, .2f); //Re-using GetKnockback
    // }

    public virtual void GetKnockback(bool playerToRight, float strength = 8, float duration = .5f, bool manualReset = true)
    {
        KnockbackNullCheckCO();

        if (strength <= 0) return;

        ToggleFlip(false);

        float temp = playerToRight != true ? 1 : -1; //get knocked back in opposite direction of player
        Vector2 direction = new Vector2(temp, rb.velocity.y);
        rb.AddForce(direction * strength, ForceMode2D.Impulse);

        if(!manualReset) LungingCO = StartCoroutine(KnockbackReset(duration));
    }

    IEnumerator KnockbackReset(float duration, float recoveryDelay = .1f)
    {
        yield return new WaitForSeconds(duration);
        rb.velocity = Vector3.zero;
        canMove = false;
        yield return new WaitForSeconds(recoveryDelay); //delay before allowing move again
        canMove = true;
        ToggleFlip(true);
    }

    void KnockbackNullCheckCO()
    {
        if (LungingCO == null) return;
        StopCoroutine(LungingCO);
        canMove = true;
        ToggleFlip(true);
    }

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
        //Boss has static healthbar
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
    public void DisableMove()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public void ToggleFlip(bool toggle)
    {
        canFlip = toggle;
    }
    #endregion
}
