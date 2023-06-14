using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_PlayerMovement : MonoBehaviour
{
    [Header("References/Setup")]
    public Base_Character character;
    public AnimatorManager animator;
    public Base_PlayerCombat combat;
    [SerializeField] Transform vfxOffset;
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
    [SerializeField] public bool canDropThrough;
    [SerializeField] public bool dropThroughBlocked;
    //[SerializeField] private CapsuleCollider2D playerCollider;
    private BoxCollider2D playerCollider;

    //Stats
    [Header("Movement Stats and Variables")]
    public float moveSpeed = 3f; //default, set by Base_Character
    public float jumpHeight = 7f; //default, set by Base_Character
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
    [SerializeField] private float landingAnimThreshold = 0.25f; //How long player needs to fall to play landing animation
    public bool canPlayLanding;

    //Jump Buffer
    private float jumpBuffer = .15f;
    private float jumpBufferTimer = 0;
    private float jumpVFXCD = .1f;
    private float jumpVFXCDTimer;

    //Double Jump
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private bool doubleJumped;

    //VFX
    private bool playingLandFX;
    private float runTimer; //How long the player has been running, resets on FX play
    private float runFXCD = .3f; //How often the FX can be played //.3f

    //Dodge
    public bool canDash = true;
    public bool isDashing;
    [SerializeField] private float dashingPower = 12f;
    [SerializeField] private float dashingTime = .08f;//0.12f;
    private float dashingCD = 1f;
    private float originalGravity;
    //Coroutine DashCO;

    //Float
    [SerializeField] bool isFloating;
    Coroutine FloatCO;
    Coroutine LandingCO;

    void Start()
    {
        playerCollider = GetComponent<BoxCollider2D>();
        if(character != null)
        {
            moveSpeed = character.Base_MoveSpeed;
            jumpHeight = character.Base_JumpHeight;
        }

        canDoubleJump = false;
        doubleJumped = false;
        allowInput = true;
        isCrouching = false;
        canMove = true;
        canAirMove = true;
        jumped = false;
        canDash = true;
        isFloating = false;
        playingLandFX = false;
        originalGravity = rb.gravityScale; //For Dash and Float
        timeSinceLeftGround = 0;
        jumpVFXCDTimer = 0;
        updatePlatform = true;
    }

    void Update()
    {
        if (!allowInput) return;
        if (!combat.isAlive) return;

        CheckCoyoteTime();
        jumpBufferTimer -= Time.deltaTime;
        jumpVFXCDTimer -= Time.deltaTime;

        VelocityCheck(); //Checks for grounded, falling, jumping, landing

        if (combat.isStunned) return;
        if (combat.isKnockedback) return;
        if (isDashing || isFloating) return;

        //if (combat.isAttacking) return; //Can use this instead of calling canMove toggles in combat script

        if (!canMove || !canAirMove) return; //priority less than isDashing, allow immunity while dashing
        
        if(canMove) horizontal = Input.GetAxisRaw("Horizontal");

        Flip();
        
        //Variable jump heights
        if(Input.GetButtonDown("Jump"))// && !jumped)
        {
            jumpBufferTimer = jumpBuffer;
        }

        //if(Input.GetButtonDown("Jump"))
        if(jumpBufferTimer > 0 && !jumped)
        {
            if(isJumping) return; //Velocity check
            if(IsGrounded() || coyoteAllowed)
            {
                jumped = true;
                //coyoteAllowed = false;
                PlayJumpVFX();
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
                canDoubleJump = true;
                //coyoteAllowed = false;
            }
        }

        //Double Jump
        if (canDoubleJump && !doubleJumped && !coyoteAllowed)
        {
            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferTimer = 0;
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight);// *.9f); //reduced height on second jump
                doubleJumped = true;
                canDoubleJump = false;
                PlayJumpVFX();
            }
        }

        //Variable Jump
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            jumpBufferTimer = 0;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        //Crouch - to fall through platforms, a short toggle to allow player to fall through completely
        if (IsGrounded())
        {
            if (Input.GetButtonDown("Crouch"))
            {
                //Allow dropping through platforms
                if(!canDropThrough || dropThroughBlocked) return;
                if(currentOneWayPlatform != null)
                    StartCoroutine(DisableCollision());
            }
            canDoubleJump = false;
            doubleJumped = false;
            if (updatePlatform) CheckPlatform();
            CheckRunVFX();
        }
        else
        {
            //Not grounded, allow CheckPlatform() to get platform ID once
            runTimer = 0;
            updatePlatform = true;
            canDoubleJump = true;
        }
    }

    private void FixedUpdate()
    {
        if (!combat.isAlive || combat.isStunned)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); //The other options didn't work
            return;
        }

        // //Only update platform while grounded
        // if (IsGrounded()) CheckPlatform();
        // else updatePlatform = true;

        if (combat.isKnockedback) return; //prevent movement inputs when knockedback
        
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

    void PlayJumpVFX()
    {
        if (jumpVFXCDTimer <= 0) InstantiateManager.Instance.VFX.JumpFX(vfxOffset);
        jumpVFXCDTimer = jumpVFXCD;
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
        else
        {
            coyoteAllowed = false;
            jumped = true;
        } 
 
    }

    #region Drop-through Platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*if(collision.gameObject.CompareTag("SolidPlatform"))
        {
            currentOneWayPlatform = null;
        }*/

        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = collision.gameObject;
            canDropThrough = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = null;
            canDropThrough = false;
        }
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();

        Physics2D.IgnoreCollision(playerCollider, platformCollider);
        yield return new WaitForSeconds(.25f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        // if (trigger.GetComponent<BlockDropThrough>()) dropThroughBlocked = true;
        if (trigger.CompareTag("SolidPlatform")) dropThroughBlocked = true;
    }

    private void OnTriggerExit2D(Collider2D trigger)
    {
        // if (trigger.GetComponent<BlockDropThrough>()) dropThroughBlocked = false;
        if (trigger.CompareTag("SolidPlatform")) dropThroughBlocked = false;
    }
    #endregion

    void CheckFallAnim()
    {
        timeSpentFalling += Time.deltaTime;

        if (timeSpentFalling > .2f) isFalling = true;

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
            timeSpentFalling = 0; //Landing animation only plays if the player falls long enough

            if (isFalling)
            {
                //Only play animation if falling longer than .2s
                if (!playingLandFX && canPlayLanding) StartCoroutine(LandingFX());

                if (canPlayLanding) LandingCO = StartCoroutine(FalltoLandAnim());
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

    #region Movement FX
    IEnumerator LandingFX()
    {
        //This needs a cooldown, or it instantiates multiple times
        playingLandFX = true;
        yield return new WaitForSeconds(.05f); //short delay to prevent offset being pushed through ground on land
        PlayJumpVFX(); //Reusing Land/Jump
        yield return new WaitForSeconds(.3f);
        playingLandFX = false;
    }

    void CheckRunVFX()
    {
        if (horizontal != 0) runTimer += Time.deltaTime;
        else runTimer = .25f; //resetting to allow FX

        if (IsGrounded() && runTimer > runFXCD)
        {
            InstantiateManager.Instance.VFX.RunFX(vfxOffset, isFacingRight);
            runTimer = 0;
        }
    }
    #endregion

    IEnumerator FalltoLandAnim()
    {
        isLanding = true;
        yield return new WaitForSeconds(.18f);
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
        //if (!updatePlatform) return;

        int i = Physics2D.OverlapCircle(groundCheck.position, 0.01f, groundLayer).GetInstanceID();
        if (i != currPlatform) currPlatform = i;
        updatePlatform = false;
        // string j = Physics2D.OverlapCircle(groundCheck.position, 0.01f, groundLayer).name;
        // Debug.Log("Current Platform: " + j);
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

        //! - using horizontal from the Input in FixedUpdate
    }
    
    public void StartDash()
    {
        if (!combat.isAlive || combat.isKnockedback) return;
        if (canDash) if(canMove || combat.isAttacking || combat.isAirAttacking) StartCoroutine(Dash());
        if (isDashing) InstantiateManager.Instance.VFX.DashFX(vfxOffset, isFacingRight);
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

    public void TimedDisableMove(float duration)
    {
        ToggleCanMove(false);
        Invoke("EnableMove", duration);
    }

    void EnableMove()
    {
        ToggleCanMove(true);
    }

    public void ToggleCanMove(bool canMoveToggle)
    {
        canMove = canMoveToggle;
        //canMove check in FixedUpdate()
        //while false, velocity is set to 0
    }

    public void StopVelocityX()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public void StopVelocityY()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    public void ToggleAirMove(bool canMoveToggle)
    {
        canAirMove = canMoveToggle;
    }
}
