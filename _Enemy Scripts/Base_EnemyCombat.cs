using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_EnemyCombat : MonoBehaviour, IDamageable
{
    [Header("Attack Behavior Setup")]
    [SerializeField] protected GameObject indicator;
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
    protected Base_EnemyController enemyController;
    [Header("Audio References")]
    [SerializeField] PlayAudioClips playAudioClips;

    [Space(10)]
    [SerializeField] public bool DEBUGMODE = false;
    [SerializeField] float spawnFXScale = 2.5f; //2.5f default 
    [Header("*Animation Times")]
    [SerializeField] float fullAttackAnimTime; //1f, 1.416667f
    [SerializeField] float attackDelayTime; //0.0834f, 0.834f

    [Space(10)]

    [Header("Start() Reference Initialization")]
    [SerializeField] Transform playerTransform;
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
    [SerializeField] float maxHP = 8;
    [SerializeField] float currentHP;
    [SerializeField] float defense = 0;
    [SerializeField] protected int totalXPOrbs = 3;
    
    [Header("--- Attack ---")]
    [SerializeField] public float attackDamage;
    [SerializeField] float attackSpeed = .1f;
    [SerializeField] float attackEndDelay = 0;
    [SerializeField] float startAttackDelay = 0;
    public float attackRange;
    public bool allowFlipBeforeAttack = false;
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
    public bool isLunging;
    public bool isAttacking;
    public bool playerToRight; //Updated somewhere //TODO:
    public float kbResist = 0;
    public bool knockbackImmune = false;
    Coroutine StunnedCO;
    Coroutine KnockbackCO;
    Coroutine LungeCO;
    protected Coroutine AttackingCO;
    private bool initialEnable;


    protected virtual void Awake()
    {
        initialEnable = true;
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
        isLunging = false;
        knockbackImmune = false;
        if (healthBar == null) healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetHealth(maxHP);
            healthbarTransform = healthBar.GetComponent<Transform>();
        }

        //Defaults
        // fullAttackAnimTime = 1f;
        // attackDelayTime = 0.0834f;
    }

    protected virtual void Start()
    {
        #if UNITY_EDITOR
        //TEMP
        if (DEBUGMODE)
        {
            maxHP *= 100;
            currentHP = maxHP;
        }
        #endif

        if(indicator != null) indicator.SetActive(false);

        isAttacking = false;
        //Must be in Start(), because of player scene loading.
        //Awake() might work during actual build with player scene always being active before enemy scenes.
        enemyStageManager = transform.parent.parent.GetComponent<EnemyStageManager>();
        playerTransform = GameManager.Instance.playerTransform;
    }

    protected virtual void OnEnable()
    {
        //Manual set, duration of SpawnIndicator SpawnIn
        //Toggle enemy before spawning in
        if(initialEnable) return;
        StartCoroutine(SpawnCO(.75f));
    }

    protected virtual void OnDisable() { initialEnable = false; }

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
            InstantiateManager.Instance.Indicator.PlayIndicator(bottomOffset.position, 0, spawnFXScale);
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
        // knockbackImmune = true;

        movement.canMove = false;
        movement.ToggleFlip(false);
        
        animator.PlayAttackAnim(fullAttackAnimTime);

        yield return new WaitForSeconds(startAttackDelay);
        knockbackImmune = true;
        if(allowFlipBeforeAttack) FacePlayer(); //Flip to faceplayer before attacking
        
        yield return new WaitForSeconds(attackDelayTime - startAttackDelay);
        CheckHit();
        yield return new WaitForSeconds(fullAttackAnimTime - attackDelayTime);
        knockbackImmune = false;
        isAttacking = false;
        yield return new WaitForSeconds(attackEndDelay);
        movement.canMove = true;
        movement.ToggleFlip(true);
    }

    public virtual void FacePlayer()
    {
        // if(!movement.canFlip) return; //remove if overriding
        //Player behind enemy
        if (enemyController.raycast.playerDetectBack)
        {
            Debug.Log("Should Flip to get player");
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
                player.GetComponent<Base_PlayerCombat>().TakeDamage(attackDamage, transform.position.x);


                // player.GetComponent<Base_PlayerCombat>().GetKnockback(!playerToRight, knockbackStrength);
                player.GetComponent<Base_PlayerCombat>().GetKnockback(transform.position.x, knockbackStrength);
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

    public virtual void GetKnockback(bool kbToRight, float strength = 8, float delay = .1f)
    {
        KnockbackNullCheckCO();

        if (strength <= 0) return;
        // if(AttackFarBehavior != null) { if(AttackFarBehavior.knockbackImmune) return; }
        // if(AttackCloseBehavior != null) { if(AttackCloseBehavior.knockbackImmune) return; }
        if (knockbackImmune) return;

        if (kbResist > 0) strength -= kbResist;
        if (strength <= 0) return; //Full resist, no knockback effect

        isKnockedback = true;
        movement.ToggleFlip(false);

        // float temp = kbDir == true ? 1 : -1; //get knocked back in opposite direction of player
        
        float kbDir;
        if(kbToRight) kbDir = 1;
        else kbDir = -1;

        // Vector2 direction = new Vector2(temp, movement.rb.velocity.y);
        // movement.rb.AddForce(direction * strength, ForceMode2D.Impulse);
        movement.rb.velocity = new Vector2(kbDir * strength, movement.rb.velocity.y); //more consistent with hits

        KnockbackCO = StartCoroutine(KnockbackReset(delay));
    }

    IEnumerator KnockbackReset(float delay, float recoveryDelay = .1f)
    {
        yield return new WaitForSeconds(delay);
        movement.rb.velocity = Vector3.zero;
        movement.canMove = false;
        movement.ToggleFlip(false);
        yield return new WaitForSeconds(recoveryDelay); //delay before allowing move again
        isKnockedback = false;

        if(!isAttacking) {
            movement.ToggleFlip(true); //if Attacking coroutine is running, don't reset
            movement.canMove = true;
        }
    }

    void KnockbackNullCheckCO()
    {
        if (KnockbackCO == null) return;
        StopCoroutine(KnockbackCO);
        
        // if(isAttacking) return; //TODO: might be issue, just remove

        movement.canMove = true;
        movement.ToggleFlip(true);
        isKnockedback = false;
    }

    public virtual void Lunge(bool lungeToRight, float strength = 8, float delay = .1f)
    {
        KnockbackNullCheckCO();

        isKnockedback = true;
        movement.ToggleFlip(false);

        float lungeDir = lungeToRight != true ? -1 : 1; //get knocked back in opposite direction of player
        // Vector2 direction = new Vector2(lungeDir, movement.rb.velocity.y);
        // movement.rb.AddForce(direction * strength, ForceMode2D.Impulse);]
        movement.rb.velocity = new Vector2(lungeDir * strength, movement.rb.velocity.y);

        KnockbackCO = StartCoroutine(KnockbackReset(delay));
    }

    IEnumerator LungeReset(float delay, float recoveryDelay = .1f)
    {
        Debug.Log("4 - LungeResetting");
        yield return new WaitForSeconds(delay);
        movement.rb.velocity = Vector3.zero;
        movement.canMove = false;
        Debug.Log("5 - Stopping Lunge");
        yield return new WaitForSeconds(recoveryDelay); //delay before allowing move again
        movement.canMove = true;

        movement.ToggleFlip(true);
        
        isLunging = false;
    }

    void LungeNullCheckCO()
    {
        if (LungeCO == null) return;
        StopCoroutine(LungeCO);
        
        // if(isAttacking) return; //TODO: same with knockback, test

        movement.canMove = true;
        movement.ToggleFlip(true);
        isLunging = false;
    }

    public void StopAttack(bool toggleFlip = false)
    {
        if (AttackingCO != null) StopCoroutine(AttackingCO);
        isAttacking = false;
        movement.canMove = true;
        movement.ToggleFlip(toggleFlip);
        //Cancels Attack animation
        animator.StopAttackAnimCO();
    }

    public virtual void TakeDamage(float damageTaken, bool knockback = false, float strength = 8, float xPos = 0)
    {
        if (!isAlive || isSpawning) return;

        HitFlash(); //Set material to white, short delay before resetting

        float totalDamage = damageTaken - defense;

        //Damage can never be lower than 1
        if (totalDamage <= 0) totalDamage = 1;
        InstantiateManager.Instance.HitEffects.ShowHitEffect(hitEffectsOffset.position);
        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);
        if(playAudioClips != null) playAudioClips.PlayRandomClip();

        if(knockback && !knockbackImmune)
        {
            bool kbToRight;
            kbToRight = xPos < transform.position.x;

            if (xPos != 0) GetKnockback(kbToRight, strength);
        }

        //Display Damage number
        InstantiateManager.Instance.TextPopups.ShowDamage(totalDamage, textPopupOffset.position);

        if (currentHP <= 0)
        {
            isAlive = false;
            Die();
        }
    }

    public virtual void TakeDamageStatus(float damageTaken)
    {
        TakeDamage(damageTaken, false);
    }

    public virtual Transform GetPosition()
    {
        return hitEffectsOffset;
    }

    public virtual void PlayIndicator()
    {
        if(indicator == null) return;
        indicator.SetActive(true);
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

        ScreenShakeListener.Instance.Shake(3);
        movement.rb.simulated = false;
        GetComponent<CircleCollider2D>().enabled = false;

        InstantiateManager.Instance.HitEffects.ShowKillEffect(hitEffectsOffset.position);
        InstantiateManager.Instance.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);

        //Base_EnemyAnimator checks for isAlive to play Death animation
        isAlive = false;
        GameManager.Instance.AugmentInventory.OnKill(transform);
        if(enemyStageManager != null) enemyStageManager.UpdateEnemyCount();

        //Disable sprite renderer before deleting gameobject
        sr.enabled = false;
        Invoke("DeleteObj", .5f); //Wait for fade out to finish
    }

    protected virtual void DeleteObj()
    {
        //TODO: make sure this is called in all Enemy scripts
        Destroy(gameObject);
    }
}