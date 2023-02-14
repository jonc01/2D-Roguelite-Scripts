using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_EnemyPlayerDetect : MonoBehaviour
{
    //Alternative to Base_EnemyRaycast, using trigger colliders instead of raycasts
    //This allows for player detection at large ranges on the Y axis
    //TODO: could also just take Player transform...

    [Header("=== Required Reference Setup ===")]
    [SerializeField] bool debugging = false;
    [SerializeField] Base_BossMovement movement;
    [SerializeField] Base_BossCombat combat;
    //Layers
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    [SerializeField] public Transform player;
    [SerializeField] public TriggerDetection[] playerDetected;

    [SerializeField]
    protected Transform ledgeCheck,
        wallPlayerCheck,
        attackCheck,
        groundCheck;
    
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
    [SerializeField] private bool usePlatformLogic;
    public int currPlatform;
    [SerializeField] private bool updatePlatform;

    [Space]
    [Header("=== Player Transform Checks ===")]
    public bool playerToRight;
    public bool 
        playerDetectFront;

    [Space]
    [Header("=== Raycast Checks ===")]
    [SerializeField]
    public bool
        ledgeDetect,
        wallDetect,
        isGrounded;

    private void Awake()
    {
        if (movement == null) movement = GetComponentInParent<Base_BossMovement>();
        if (combat == null) combat = GetComponentInParent<Base_BossCombat>();

        if (ledgeCheck == null) ledgeCheck = transform.Find("ledgeCheck").transform;
        if (wallPlayerCheck == null) wallPlayerCheck = transform.Find("playerWallCheck").transform;
        if (attackCheck == null) attackCheck = transform.Find("attackCheck").transform;
        if (groundCheck == null) groundCheck = transform.Find("groundCheck").transform;

        updatePlatform = true;
        if(player == null) player = GameManager.Instance.Player;
    }

    void Start(){

    }

    void OnEnable()
    {
        if(player == null) player = GameManager.Instance.Player;
    }

    void Update()
    {
        if (!combat.isAlive) return;
        if (debugging) DebugDrawRaycast();

        isGrounded = IsGrounded();
        
        //Not grounded, allow CheckPlatform() to get platform ID once
        if(usePlatformLogic)
        {
            if (isGrounded) if(updatePlatform) CheckPlatform();
            else updatePlatform = true;
        }

        //Player Transform
        AttackCheck();
        PlayerDetectCheck();
        UpdatePlayerToRight();
        
        //Raycasts
        LedgeWallCheck();
    }

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

        //This is just here for visual debugging, doesn't use a raycast
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
        //TODO: replace these with triggers
        if(playerDetected.Length == 0) return;
        //Trigger colliders
        // playerInRangeClose = playerDetected[0].objectDetected;
        // playerInRangeFar = playerDetected[1].objectDetected;
    }

    public bool CheckPlayerDetect(int triggerIndex)
    {
        if(triggerIndex < playerDetected.Length)
            return playerDetected[triggerIndex].objectDetected;
        else return false; //index does not exist
    }

    void LedgeWallCheck()
    {
        //TODO: also might not be needed, not patrolling, just chasing player
        ledgeDetect = Physics2D.Raycast(ledgeCheck.position, Vector2.down, ledgeCheckDistance, groundLayer);
        wallDetect = Physics2D.Raycast(wallPlayerCheck.position, transform.right, wallCheckDistance, groundLayer);
    }

    void PlayerDetectCheck()
    {
        //TODO: replace these with triggers
        // playerDetectFront = Physics2D.Raycast(wallPlayerCheck.position, transform.right, playerCheckDistanceFront, playerLayer);
        // playerDetectBack = Physics2D.Raycast(wallPlayerCheck.position, -transform.right, playerCheckDistanceBack, playerLayer);
        // if(player.position.x < transform.position.x) playerDetectFront = true;
        // else playerDetectFront = false;
        if(playerToRight == movement.isFacingRight) playerDetectFront = true;
    }

    void UpdatePlayerToRight()
    {
        // Updates 'playerToRight' bool: where the player is in relation to enemy
        // if (playerDetectFront) playerToRight = movement.isFacingRight;
        // else playerToRight = !movement.isFacingRight;

        if(player.position.x < transform.position.x) playerToRight = false;
        else playerToRight = true;

        //no else; don't want to update playerToRight when player jumps above raycast
    }
}
