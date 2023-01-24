using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_EnemyCombat : MonoBehaviour, IDamageable
{
    [Header("Attack Behavior Setup")]
    [SerializeField] public Base_CombatBehavior AttackCloseBehavior;
    [SerializeField] public Base_CombatBehavior AttackFarBehavior;
    public bool canAttackFar;

    [Header("References/Setup")]
    public LayerMask playerLayer;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected Transform textPopupOffset;
    [SerializeField] public Transform hitEffectsOffset;
    [SerializeField] protected Transform bottomOffset;
    //public Collider collider;
    [SerializeField] private Material mWhiteFlash;
    private Material mDefault;
    private Base_EnemyController enemyController;

    [Space(10)]
    [SerializeField] public bool DEBUGMODE = false;

    [Header("= Required Manual Animations Setup =")]
    public float attackAnimDelayFrames = .1f;
    public float attackAnimTotalFrames = 1f;
    public float sampleRate = 12f;
    [Header("*Calculated at Start()* Animation Results")]
    [SerializeField] float fullAttackAnimTime;
    [SerializeField] float attackDelayTime;

    [Space(10)]

    [Header("Start() Reference Initialization")]
    public Base_EnemyMovement movement;
    public Base_EnemyAnimator animator;
    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected HealthBar healthBar;
    public Transform healthbarTransform;
    [SerializeField] protected EnemyStageManager enemyStageManager;

    [Space(10)]

    //HealthBar

    //Stats
    [Header("=== STATS (char optional) ===")]
    [Header("--- Health ---")]
    public Base_Character character;
    [SerializeField] float maxHP;
    [SerializeField] float currentHP;
    [SerializeField] float defense = 0;
    [SerializeField] protected int totalXPOrbs = 3;
    
    [Header("--- Attack ---")]
    [SerializeField] public float attackDamage;
    [SerializeField] float attackSpeed;
    [SerializeField] float attackEndDelay = 0;
    [SerializeField] float startAttackDelay = 0;
    public float attackRange;
    [SerializeField] public float knockbackStrength = 0; //4 is moderate
    [Space(10)]
    float timeSinceAttack;
    //float critChance;
    //float critMultiplier;

    [Header("--- Status ---")]
    //Bools
    [SerializeField] public bool isAlive;
    [SerializeField] public bool isSpawning;
    public bool isStunned;
    public bool isKnockedback;
    public bool isAttacking;
    public bool playerToRight;
    public float kbResist = 0;
    Coroutine StunnedCO;
    Coroutine KnockbackCO;
    protected Coroutine AttackingCO;


    protected virtual void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        mDefault = sr.material;
        animator = GetComponentInChildren<Base_EnemyAnimator>();
        //collider = GetComponent<BoxCollider2D>();
        //playerLayer = GameObject.FindGameObjectWithTag("Player").GetComponent<LayerMask>();
        if (movement == null) movement = GetComponent<Base_EnemyMovement>();
        if (enemyController == null) enemyController = GetComponent<Base_EnemyController>();
        //Initiating base stats before modifiers

        if (character != null)
        {
            defense = character.Base_Defense;
            attackDamage = character.Base_AttackDamage;
            attackSpeed = character.Base_AttackSpeed;
            attackRange = character.Base_AttackRange;
            maxHP = character.Base_MaxHP;
        }

        currentHP = maxHP;
        isAlive = true;
        isStunned = false;
        isKnockedback = false;
        if (healthBar == null) healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetHealth(maxHP);
            healthbarTransform = healthBar.GetComponent<Transform>();
        }

        fullAttackAnimTime = attackAnimTotalFrames / sampleRate;
        attackDelayTime = attackAnimDelayFrames / sampleRate;

    }

    protected virtual void Start()
    {
        if (DEBUGMODE)
        {
            maxHP *= 100;
            currentHP = maxHP;
        }
        

        isAttacking = false;
        //Must be in Start(), because of player scene loading.
        //Awake() might work during actual build with player scene always being active before enemy scenes.
        enemyStageManager = transform.parent.parent.GetComponent<EnemyStageManager>();
    }

    protected virtual void OnEnable()
    {
        //Manual set, duration of SpawnIndicator SpawnIn
        //Toggle enemy before spawning in
        StartCoroutine(SpawnCO(.75f));
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isAlive || isSpawning) return; //Stops all updates if dead
        timeSinceAttack += Time.deltaTime;
        if (isStunned) return;

        AttackMoveCheck();
    }

    protected virtual IEnumerator SpawnCO(float delay)
    {
        isSpawning = true; //This prevents the enemy from attacking and taking damage
        healthBar.gameObject.SetActive(false);
        if(bottomOffset != null)
            InstantiateManager.Instance.Indicator.ChargeUp(bottomOffset.position, transform, 1);
        //Toggle SR off
        sr.enabled = false;
        yield return new WaitForSeconds(0.1667f); //TODO: appear delay
        sr.enabled = true;
        HitFlash(delay);
        healthBar.gameObject.SetActive(true);
        //Toggle SR on
        yield return new WaitForSeconds(0.583f);
        isSpawning = false;
    }

    protected virtual void AttackMoveCheck()
    {
        float delay = attackSpeed + fullAttackAnimTime;
        if (timeSinceAttack <= delay) //air attacks not affected
        {
            movement.canMove = false;
        }
        else movement.canMove = true;
    }

    public virtual void Attack()
    {
        if (!isAlive || isAttacking || isSpawning) return;
        if (timeSinceAttack > attackSpeed) //TODO: repeat for AttackClose/Far
        {
            timeSinceAttack = 0;
            StopAttack();
            AttackingCO = StartCoroutine(Attacking());
        }
    }

    #region Attack Behavior Overrides

    public virtual void AttackClose()
    {
        if (!isAlive || isAttacking || isSpawning) return;
        //If no alternate behavior added, use default Attack()
        if (AttackCloseBehavior == null) Attack();

        if (timeSinceAttack > attackSpeed)
        {
            timeSinceAttack = 0;
            AttackCloseBehavior.Attack();
        }
    }

    public virtual void AttackFar()
    {
        if (!isAlive || isAttacking || isSpawning) return;
        if (AttackFarBehavior == null) return;

        if (timeSinceAttack > attackSpeed)
        {
            timeSinceAttack = 0;
            AttackFarBehavior.Attack();
        }
    }

    public virtual bool CanAttackFar()
    {
        return AttackFarBehavior.canAttack;
    }

    #endregion

    protected virtual IEnumerator Attacking()
    {
        //Allow flip for a little longer
        isAttacking = true;
        movement.canMove = false;
        movement.ToggleFlip(false);
        
        animator.PlayAttackAnim(fullAttackAnimTime);

        yield return new WaitForSeconds(startAttackDelay);
        FacePlayer(); //Flip to faceplayer before attacking
        
        yield return new WaitForSeconds(attackDelayTime - startAttackDelay);
        CheckHit();
        yield return new WaitForSeconds(fullAttackAnimTime - attackDelayTime);
        isAttacking = false;
        yield return new WaitForSeconds(attackEndDelay);
        movement.canMove = true;
        movement.ToggleFlip(true);
    }

    void FacePlayer()
    {
        //Player behind enemy
        if (enemyController.raycast.playerDetectBack)
        {
            movement.ManualFlip(!movement.isFacingRight);
        }
    }

    public virtual void CheckHit()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D player in hitPlayers)
        {
            if (player.GetComponent<Base_PlayerCombat>() != null)
            {
                player.GetComponent<Base_PlayerCombat>().TakeDamage(attackDamage);
                player.GetComponent<Base_PlayerCombat>().GetKnockback(!playerToRight, knockbackStrength);
                //knockback
                //if (AttackFarBehavior != null) AttackFarBehavior.playerHit = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public virtual void GetStunned(float stunDuration = .5f)
    {
        if (!isAlive || isSpawning) return;

        //TODO: 
        //StopAllCoroutines(); //This could allow stun locks, depending on how often player can apply stun
        StopAttack();
        StunnedCO = StartCoroutine(Stunned(stunDuration));
    }

    protected virtual IEnumerator Stunned(float stunDuration)
    {
        isStunned = true;
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    public virtual void GetKnockback(bool playerToRight, float strength = 8, float delay = .1f)
    {
        KnockbackNullCheckCO();

        if (kbResist > 0) strength -= kbResist;
        if (strength <= 0) return;

        isKnockedback = true;
        movement.ToggleFlip(false);

        float temp = playerToRight != true ? 1 : -1; //get knocked back in opposite direction of player
        Vector2 direction = new Vector2(temp, movement.rb.velocity.y);
        movement.rb.AddForce(direction * strength, ForceMode2D.Impulse);

        KnockbackCO = StartCoroutine(KnockbackReset(delay));
    }

    IEnumerator KnockbackReset(float delay, float recoveryDelay = .1f)
    {
        yield return new WaitForSeconds(delay);
        movement.rb.velocity = Vector3.zero;
        movement.canMove = false;
        yield return new WaitForSeconds(recoveryDelay); //delay before allowing move again
        movement.canMove = true;
        movement.ToggleFlip(true);
        isKnockedback = false;
    }

    void KnockbackNullCheckCO()
    {
        if (KnockbackCO == null) return;
        StopCoroutine(KnockbackCO);
        movement.canMove = true;
        movement.ToggleFlip(true);
        isKnockedback = false;
    }

    void StopAttack(bool toggleFlip = false)
    {
        if (AttackingCO != null) StopCoroutine(AttackingCO);
        isAttacking = false;
        movement.canMove = true;
        movement.ToggleFlip(toggleFlip);
        //Cancels Attack animation
        animator.StopAttackAnimCO();
    }

    public virtual void TakeDamage(float damageTaken, bool knockback = false, float strength = 8)
    {
        if (!isAlive || isSpawning) return;

        HitFlash(); //Set material to white, short delay before resetting

        float totalDamage = damageTaken - defense;

        //Damage can never be lower than 1
        if (totalDamage <= 0) totalDamage = 1;

        InstantiateManager.Instance.HitEffects.ShowHitEffect(hitEffectsOffset.position);
        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);

        if (knockback) GetKnockback(playerToRight, strength);

        //Display Damage number
        InstantiateManager.Instance.TextPopups.ShowDamage(totalDamage, textPopupOffset.position);

        if (currentHP <= 0)
        {
            isAlive = false;
            Die();
        }
    }

    void HitFlash(float resetDelay = .1f)
    {
        sr.material = mWhiteFlash;
        Invoke("ResetMaterial", resetDelay);
    }

    void ResetMaterial()
    {
        sr.material = mDefault;
    }

    protected virtual void Die()
    {
        healthBar.gameObject.SetActive(false);
        if(AttackingCO != null) StopCoroutine(AttackingCO);

        ScreenShakeListener.Instance.Shake(2);
        movement.rb.simulated = false;
        GetComponent<CircleCollider2D>().enabled = false;

        InstantiateManager.Instance.HitEffects.ShowKillEffect(hitEffectsOffset.position);
        InstantiateManager.Instance.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);

        //Base_EnemyAnimator checks for isAlive to play Death animation
        isAlive = false;
        if(enemyStageManager != null) enemyStageManager.UpdateEnemyCount();

        //Disable sprite renderer before deleting gameobject
        sr.enabled = false;
        Invoke("DeleteObj", .5f); //Wait for fade out to finish
    }

    private void DeleteObj()
    {
        Destroy(gameObject);
    }
}