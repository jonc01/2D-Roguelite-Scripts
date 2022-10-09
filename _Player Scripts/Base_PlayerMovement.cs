using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_PlayerMovement : MonoBehaviour
{
    [Header("References/Setup")]
    public Base_Character character;
    public AnimatorManager animator;
    public Base_PlayerCombat combat;
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] bool showGizmos = false;

    [Space(10)]
    //Controls
    public bool allowInput = true; //toggled when player is stunned, rooted, or during a cutscene
    //TODO: not setup yet

    [Header("Current Platform")]
    public int currPlatform;
    private bool updatePlatform;

    [Space(10)]

    //Drop-through platforms
    private GameObject currentOneWayPlatform;
    [SerializeField] private CapsuleCollider2D playerCollider;
    //private BoxCollider2D playerCollider;

    //Stats
    public float moveSpeed = 3f; //default, set by Base_Character
    public float jumpHeight = 8f; //default, set by Base_Character
    public float coyoteTimeThreshold = .1f;
    private float timeSinceLeftGround;
    private bool coyoteAllowed;

    public float horizontal;
    public bool isFacingRight = true;
    public bool isGrounded;
    public bool isCrouching = false;
    public bool canMove = true;
    public bool canAirMove = true;

    //Animation Bools
    public bool jumped;
    public bool isJumping;
    public bool isFalling;
    public bool isLanding;
    private float timeSpentFalling;
    [SerializeField] private float landingAnimThreshold = 0.1f; //How long player needs to fall to play landing animation
    public bool canPlayLanding;

    //Dodge
    public bool canDash = true;
    public bool isDashing;
    private float dashingPower = 20f;
    private float dashingTime = .08f;//0.12f;
    private float dashingCD = 1f;
    private float originalGravity;
    //Coroutine DashCO;

    //Float
    [SerializeField] bool isFloating;
    Coroutine FloatCO;

    void Start()
    {
        playerCollider = GetComponent<CapsuleCollider2D>();
        if(character != null)
        {
            moveSpeed = character.Base_MoveSpeed;
            jumpHeight = character.Base_JumpHeight;
        }

        allowInput = true;
        isCrouching = false;
        canMove = true;
        canAirMove = true;
        jumped = false;
        canDash = true;
        isFloating = false;
        originalGravity = rb.gravityScale; //For Dash and Float
        timeSinceLeftGround = 0;
        updatePlatform = true;
    }

    void Update()
    {
        if (!combat.isAlive) return;

        CheckCoyoteTime();

        VelocityCheck(); //Checks for grounded, falling, jumping, landing

        if (combat.isStunned) return;
        if (isDashing || isFloating) return;

        if (!canMove || !canAirMove) return; //priority less than isDashing, allow immunity while dashing
        
        if(canMove) horizontal = Input.GetAxisRaw("Horizontal");

        Flip();

        //Variable jump heights
        if(Input.GetButtonDown("Jump") && !jumped)
        {
            if(IsGrounded() || coyoteAllowed)
            {
                jumped = true;
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            }
        }
        if(Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        //Crouch - to fall through platforms, just a trigger, not a toggle
        if (IsGrounded())
        {
            if (Input.GetButtonDown("Crouch"))
            {
                //Allow dropping through platforms
                if(currentOneWayPlatform != null) StartCoroutine(DisableCollision());
            }
            CheckPlatform();
        }
        else
        {
            //Not grounded, allow CheckPlatform() to get platform ID once
            updatePlatform = true;
        }
    }

    private void FixedUpdate()
    {
        if (!combat.isAlive || combat.isStunned)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); //The other options didn't work
            return;
        }
        
        //if (combat.isStunned) return; //priority over dash, prevent dash while stunned
        if (isDashing || isFloating) return;

        if (!canMove)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }


        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        //VelocityCheck();
    }

    void CheckCoyoteTime()
    {
        if (IsGrounded())
        {
            timeSinceLeftGround = 0;
            jumped = false;
        }
        else timeSinceLeftGround += Time.deltaTime;

        if (timeSinceLeftGround <= coyoteTimeThreshold)
        {
            coyoteAllowed = true;
        }
        else coyoteAllowed = false;
    }

    #region Drop-through Platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = null;
        }
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();

        Physics2D.IgnoreCollision(playerCollider, platformCollider);
        yield return new WaitForSeconds(.25f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
    #endregion

    void CheckFallAnim()
    {
        timeSpentFalling += Time.deltaTime;

        if (timeSpentFalling > .1f) isFalling = true;

        if (timeSpentFalling >= landingAnimThreshold) canPlayLanding = true;
        else canPlayLanding = false;
    }

    void VelocityCheck()
    {
        if (rb.velocity.y > 0)
        {
            isGrounded = false;
            isJumping = true;
            isFalling = false;
        }

        if (rb.velocity.y == 0)
        {
            isGrounded = true;
            isJumping = false;
            timeSpentFalling = 0;

            if (isFalling)
            {
                //Only play animation if falling longer than .1s
                if (canPlayLanding)
                    StartCoroutine(FalltoLandAnim());
                else isFalling = false;
            }
        }

        if (rb.velocity.y < 0)
        {
            isGrounded = false;
            isJumping = false;
            //isFalling = true;
            canPlayLanding = false;
            CheckFallAnim();
        }
    }

    IEnumerator FalltoLandAnim()
    {
        isLanding = true;
        yield return new WaitForSeconds(.18f); //TODO: test
        isLanding = false;
        isFalling = false;
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.01f, groundLayer);
    }

    void CheckPlatform()
    {
        //Only updating platform after IsGrounded() returns false, then update once
        if (!updatePlatform) return;

        updatePlatform = false;
        int i = Physics2D.OverlapCircle(groundCheck.position, 0.01f, groundLayer).GetInstanceID();
        if(i != currPlatform)
        {
            if (i > 0) currPlatform = i;
        }
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(groundCheck.position, 0.01f);
        }
    }

    private void Flip()
    {
        if(isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public void Move(bool moveRight)
    {
        //move at stats.moveSpeed;
    }
    
    public void StartDash()
    {
        if (!combat.isAlive) return;
        if(canDash) if(canMove || combat.isAttacking || combat.isAirAttacking) StartCoroutine(Dash());
    }
    
    private IEnumerator Dash()
    {
        combat.CancelAttack();

        canDash = false;
        isDashing = true;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCD);
        canDash = true;
    }

    public void Float(float duration = .1f)
    {
        CheckFloatCO();
        FloatCO = StartCoroutine(Floating(duration));
    }

    IEnumerator Floating(float duration)
    {
        isFloating = true;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(0, 0);

        yield return new WaitForSeconds(duration);

        rb.gravityScale = originalGravity;
        isFloating = false;
    }

    void CheckFloatCO()
    {
        if (FloatCO != null) StopCoroutine(FloatCO);
        isFloating = false;
        rb.gravityScale = originalGravity;
    }

    public void StopCO()
    {
        StopAllCoroutines();
    }

    public void ToggleCanMove(bool canMoveToggle)
    {
        canMove = canMoveToggle;
        //canMove check in FixedUpdate()
        //while false, velocity is set to 0
    }

    public void ToggleAirMove(bool canMoveToggle)
    {
        canAirMove = canMoveToggle;
    }
}
