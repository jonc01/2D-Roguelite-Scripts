using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    //Set at Start()

    [Space(10)]
    [SerializeField] public bool DEBUGMODE = false;
    [SerializeField] bool showGizmos = false;
    [SerializeField] float spawnFXScaleX = 2.5f; //2.5f default 
    [SerializeField] float spawnFXScaleY = 2.5f; //2.5f default 
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
    [SerializeField] protected EnemyWaveManager enemyWaveManager;
    protected AugmentInventory augmentInventory;
    public InstantiateManager instantiateManager;
    protected ScreenShakeListener screenShakeListener;
    [SerializeField] protected PlayAudioClips playAudioClips;

    [Space(15)]

    //HealthBar

    //Stats
    [Header("=== STATS (char optional) ===")]
    [Header("--- Health ---")]
    public Base_Character character;
    [SerializeField] float maxHP = 8;
    [SerializeField] protected float currentHP;
    [SerializeField] float defense = 0;
    [SerializeField] protected int totalXPOrbs = 3;
    
    [Header("--- Attack ---")]
    [SerializeField] public float attackDamage;
    [SerializeField] float attackSpeed = .1f;
    [SerializeField] float attackEndDelay = 0;
    [SerializeField] float startAttackDelay = 0;
    [SerializeField] protected float optionalAttackDelay = 0;
    [SerializeField] protected float indicatorOffset = 0;
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
    // public bool playerToRight; //Updated somewhere //TODO:
    public bool altAttacking;
    public bool chasePlayer;
    [SerializeField] protected bool damageImmune;
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
        if(playAudioClips == null) playAudioClips = GetComponentInChildren<PlayAudioClips>();
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
        damageImmune = false;
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
        altAttacking = false;
        chasePlayer = true;
        //Must be in Start(), because of player scene loading.
        //Awake() might work during actual build with player scene always being active before enemy scenes.
        playerTransform = GameManager.Instance.playerTransform;
        augmentInventory = GameManager.Instance.AugmentInventory;
        instantiateManager = InstantiateManager.Instance;
        screenShakeListener = ScreenShakeListener.Instance;

        Transform currObj = transform;
        for(int i=0; i<3; i++) //Limiting check to 3 parents
        {
            EnemyWaveManager waveManagerCheck = currObj.GetComponent<EnemyWaveManager>();
            if(waveManagerCheck != null)
            {
                enemyWaveManager = waveManagerCheck; //Wave Manager found
                break;
            }
            else
            {
                currObj = currObj.parent;
                if(currObj == null) break; //no more parent objects
            }
        }

        // if(transform.parent.parent == null) Debug.Log("No Enemy StageManager");
        // else enemyWaveManager = transform.parent.GetComponent<EnemyWaveManager>();
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
        if(bottomOffset != null && instantiateManager != null)
            instantiateManager.Indicator.PlayIndicator(bottomOffset.position, 0, spawnFXScaleX, spawnFXScaleY);
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
        if(AttackFarBehavior == null) return false;
        return AttackFarBehavior.canAttack;
    }

    #endregion

    protected virtual IEnumerator Attacking()
    {
        isAttacking = true;
        // knockbackImmune = true;

        movement.canMove = false;
        movement.ToggleFlip(false);

        instantiateManager.TextPopups.ShowIndicator(hitEffectsOffset.position, indicatorOffset);
        yield return new WaitForSeconds(optionalAttackDelay);
        FacePlayer();
        
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
        if (!showGizmos) return;
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
        // KnockbackNullCheckCO();
        LungeNullCheckCO();
        // if(strength <= 0) return;
        Debug.Log("lunge called");
        // isKnockedback = true;
        isLunging = true;
        movement.ToggleFlip(false);

        // float lungeDir = lungeToRight != true ? 1 : -1; //lunge towards player

        float lungeDir;
        if(lungeToRight) lungeDir = 1;
        else lungeDir = -1;

        // Vector2 direction = new Vector2(lungeDir, movement.rb.velocity.y);
        // movement.rb.AddForce(direction * strength, ForceMode2D.Impulse);]
        movement.rb.velocity = new Vector2(lungeDir * strength, movement.rb.velocity.y);

        // KnockbackCO = StartCoroutine(KnockbackReset(delay));
        LungeCO = StartCoroutine(LungeReset(delay));
    }

    IEnumerator LungeReset(float delay, float recoveryDelay = .1f)
    {
        yield return new WaitForSeconds(delay);
        movement.rb.velocity = Vector3.zero;
        movement.canMove = false;
        movement.ToggleFlip(false);
        yield return new WaitForSeconds(recoveryDelay); //delay before allowing move again
        isLunging = false;

        if(!isAttacking)
        {
            movement.ToggleFlip(true);
            movement.canMove = true; //TODO: avoid overlapping canMove resets
        }
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

    public virtual void TakeDamage(float damageTaken, bool knockback = false, bool procOnHit = false, float strength = 8, float xPos = 0)
    {
        if (!isAlive || isSpawning) return;
        if (damageImmune || damageTaken <= 0) return;

        HitFlash(); //Set material to white, short delay before resetting
        if(procOnHit) augmentInventory.OnHit(hitEffectsOffset);

        float totalDamage = damageTaken - defense;


        //Damage can never be lower than 1
        if (totalDamage <= 0) totalDamage = 1;
        if(instantiateManager != null) instantiateManager.HitEffects.ShowHitEffect(hitEffectsOffset.position);
        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);
        if(playAudioClips != null) playAudioClips.PlayHitAudio();

        if(knockback && !knockbackImmune)
        {
            bool kbToRight;
            kbToRight = xPos < transform.position.x;

            if (xPos != 0) GetKnockback(kbToRight, strength);
        }

        //Display Damage number
        if(instantiateManager != null)
            instantiateManager.TextPopups.ShowDamage(totalDamage, textPopupOffset.position);

        if (currentHP <= 0)
        {
            isAlive = false;
            Die();
        }
    }

    public virtual void TakeDamageStatus(float damageTaken, int colorIdx)
    {
        // TakeDamage(damageTaken, false, false);

        if (!isAlive || isSpawning) return;
        if (damageImmune || damageTaken <= 0) return;

        HitFlash(); //Set material to white, short delay before resetting

        if(instantiateManager != null)
            instantiateManager.HitEffects.ShowHitEffect(hitEffectsOffset.position);

        currentHP -= damageTaken;
        healthBar.UpdateHealth(currentHP);
        if(playAudioClips != null) playAudioClips.PlayHitAudio();

        //Display Damage number
        if(instantiateManager != null)
            instantiateManager.TextPopups.ShowStatusDamage(damageTaken, textPopupOffset.position, colorIdx);

        if (currentHP <= 0)
        {
            isAlive = false;
            Die();
        }
    }

    public virtual Transform GetHitPosition()
    {
        return hitEffectsOffset;
    }

    public virtual Transform GetGroundPosition()
    {
        // return transform;
        return bottomOffset;
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

    public virtual void ToggleHealthbar(bool toggle)
    {
        healthBar.gameObject.SetActive(toggle);
    }

    public virtual void ToggleDamageImmune(bool toggle)
    {
        damageImmune = toggle;
    }

    protected virtual void Die()
    {
        ToggleHealthbar(false);
        if(AttackingCO != null) StopCoroutine(AttackingCO);

        if(screenShakeListener != null) screenShakeListener.Shake(3);

        movement.rb.simulated = false;
        GetComponent<CapsuleCollider2D>().enabled = false;

        if(instantiateManager != null)
        {
            instantiateManager.HitEffects.ShowKillEffect(hitEffectsOffset.position);
            instantiateManager.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);
        }

        if(playAudioClips != null) playAudioClips.PlayDeathSound();

        //Base_EnemyAnimator checks for isAlive to play Death animation
        isAlive = false;
        // GameManager.Instance.AugmentInventory.OnKill(hitEffectsOffset);
        augmentInventory.OnKill(GetGroundPosition());
        if(enemyWaveManager != null) enemyWaveManager.UpdateEnemyCount();

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