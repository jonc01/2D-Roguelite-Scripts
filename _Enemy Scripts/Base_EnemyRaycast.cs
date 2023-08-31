using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_EnemyRaycast : MonoBehaviour
{
    [Header("=== Required reference setup ===")]
    [SerializeField] bool debugging = false;
    [SerializeField] Base_EnemyMovement movement;
    //Layers
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    [SerializeField]
    protected Transform ledgeCheck,
        wallPlayerCheck,
        attackCheck,
        groundCheck,
        backToWallCheck;

    [Space]
    [Header("=== Adjustable Variables ===")] //Raycast variables
    [SerializeField] private float ledgeCheckDistance = 0.05f;
    [SerializeField]
    private float
        wallCheckDistance = 0.5f, //negative values - enemies are initialized facing left
        playerCheckDistanceFront = 3f, //Aggro range
        playerCheckDistanceBack = 1.5f;
    public float attackRangeClose = 0.67f; //when to start attacking player, uses a raycast to detect if player is within range
    public float attackRangeFar = 1f;

    [Space]
    [Header("Current Platform")]
    public int currPlatform;
    [SerializeField] private bool updatePlatform;

    [Space]
    [Header("=== Raycast Checks ===")]
    public bool playerToRight;
    [SerializeField]
    public bool
        playerDetectFront,
        playerDetectBack,
        playerInRangeClose,
        playerInRangeFar,
        ledgeDetect,
        wallDetect,
        isGrounded,
        backToWall;

    private void Awake()
    {
        if (movement == null) movement = GetComponentInParent<Base_EnemyMovement>();

        if (ledgeCheck == null) ledgeCheck = transform.Find("ledgeCheck").transform;
        if (wallPlayerCheck == null) wallPlayerCheck = transform.Find("playerWallCheck").transform;
        if (attackCheck == null) attackCheck = transform.Find("attackCheck").transform;
        if (groundCheck == null) groundCheck = transform.Find("groundCheck").transform;

        updatePlatform = true;
    }

    void Update()
    {
        if (!movement.combat.isAlive) return;
        if (debugging) DebugDrawRaycast();

        isGrounded = IsGrounded();
        // movement.canFlip = isGrounded;
        
        //Not grounded, allow CheckPlatform() to get platform ID once
        if (isGrounded) if(updatePlatform) CheckPlatform();
        else updatePlatform = true;

        AttackCheck();
        LedgeWallCheck();
        PlayerDetectCheck();
        UpdatePlayerToRight();
    }

    // void FixedUpdate()
    // {
    //     
    // }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.01f, groundLayer);
    }

    void CheckPlatform()
    {
        //Only updating platform after IsGrounded() returns false, then update once
        // if (!updatePlatform) return;

        int i = Physics2D.OverlapCircle(groundCheck.position, 0.01f, groundLayer).GetInstanceID();
        if (i != currPlatform) currPlatform = i;
        updatePlatform = false;
    }

    void DebugDrawRaycast()
    {
        Vector3 down = transform.TransformDirection(Vector3.down) * ledgeCheckDistance;
        Debug.DrawRay(ledgeCheck.position, down, Color.green);

        Vector3 right = transform.TransformDirection(Vector3.right) * wallCheckDistance;
        //Debug.DrawRay(wallPlayerCheck.position, right, Color.blue);

        Vector3 attackRight = transform.TransformDirection(Vector3.right) * playerCheckDistanceFront;
        Debug.DrawRay(wallPlayerCheck.position, attackRight, Color.cyan);

        Vector3 attackLeft = transform.TransformDirection(Vector3.left) * playerCheckDistanceBack;
        Debug.DrawRay(wallPlayerCheck.position, attackLeft, Color.red);

        Vector3 playerInAttackRangeFar = transform.TransformDirection(Vector3.right) * attackRangeFar;
        Debug.DrawRay(attackCheck.position, playerInAttackRangeFar, Color.yellow);

        Vector3 playerInAttackRangeClose = transform.TransformDirection(Vector3.right) * attackRangeClose;
        Debug.DrawRay(attackCheck.position, playerInAttackRangeClose, Color.magenta);

    }

    void AttackCheck()
    {
        playerInRangeClose = Physics2D.Raycast(attackCheck.position, transform.right, attackRangeClose, playerLayer);
        playerInRangeFar = Physics2D.Raycast(attackCheck.position, transform.right, attackRangeFar, playerLayer);
    }

    void LedgeWallCheck()
    {
        ledgeDetect = Physics2D.Raycast(ledgeCheck.position, Vector2.down, ledgeCheckDistance, groundLayer);
        wallDetect = Physics2D.Raycast(wallPlayerCheck.position, transform.right, wallCheckDistance, groundLayer);

        if(backToWallCheck != null)
        {
            backToWall = Physics2D.Raycast(wallPlayerCheck.position, transform.right, -wallCheckDistance, groundLayer);
        }
    }

    void PlayerDetectCheck()
    {
        playerDetectFront = Physics2D.Raycast(wallPlayerCheck.position, transform.right, playerCheckDistanceFront, playerLayer);
        playerDetectBack = Physics2D.Raycast(wallPlayerCheck.position, -transform.right, playerCheckDistanceBack, playerLayer);
    }

    void UpdatePlayerToRight()
    {
        // Updates 'playerToRight' bool: where the player is in relation to enemy
        if (playerDetectFront)  playerToRight = movement.isFacingRight;
        else if (playerDetectBack)  playerToRight = !movement.isFacingRight;
        //! don't use this for knockback, won't behave correctly if the player is midair or the enemy can't update

        //if (playerDetectFront)
            // if (movement.isFacingRight) // E-> P
            //     playerToRight = true;
            // else if (!movement.isFacingRight) // P <-E
            //      playerToRight = false;
        //else if (playerDetectBack)
            // if (movement.isFacingRight) // P E->
            //     playerToRight = false;
            // else if (!movement.isFacingRight) // <-E P
            //     playerToRight = true;
        
        //! - no else; don't update playerToRight when player jumps above raycast
    }
}
