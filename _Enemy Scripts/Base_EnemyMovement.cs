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

    [Header("Lunge")]
    [SerializeField] bool canLunge;
    [SerializeField] bool isLunging;
    Coroutine LungeCO;

    private void Awake()
    {
        if(character != null)
            moveSpeed = character.Base_MoveSpeed;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        canFlip = true;
        canLunge = true;
        isLunging = false;
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

    public void Lunge(bool facingRight, float strength = 8, float delay = .5f)
    {
        //TODO: repurposing Knockback for Lunge
        KnockbackNullCheckCO();

        canLunge = false;
        isLunging = true;
        ToggleFlip(false);
        float temp = facingRight != true ? -1 : 1; //should lunge in direction facing
        Vector2 direction = new Vector2(temp, rb.velocity.y);
        rb.AddForce(direction * strength, ForceMode2D.Impulse);

        LungeCO = StartCoroutine(KnockbackReset(delay));
    }

    IEnumerator KnockbackReset(float delay, float recoveryDelay = .1f)
    {
        yield return new WaitForSeconds(delay);
        rb.velocity = Vector3.zero;
        canMove = false;
        yield return new WaitForSeconds(recoveryDelay); //delay before allowing move again
        canMove = true;
        ToggleFlip(true);
        isLunging = false;
        canLunge = true;
    }

    void KnockbackNullCheckCO()
    {
        //End Coroutine early, reset variables
        if (LungeCO == null) return;
        StopCoroutine(LungeCO);
        canMove = true;
        ToggleFlip(true);
        isLunging = false;
        canLunge = true;
    }

    #region Flip
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
