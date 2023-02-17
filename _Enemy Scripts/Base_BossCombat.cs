using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_BossCombat : MonoBehaviour, IDamageable
{

    [Header("Attack Behavior Setup")]
    [SerializeField] public Base_CombatBehavior AttackCloseBehavior;
    [SerializeField] public Base_CombatBehavior AttackFarBehavior;
    public bool canAttackFar;
    public bool canAttack;

    [Header("References/Setup")]
    public LayerMask playerLayer;
    [SerializeField] protected Transform[] attackPoint;
    public float[] attackRangeX;
    public float[] attackRangeY;
    [SerializeField] protected Transform textPopupOffset;
    [SerializeField] public Transform hitEffectsOffset;
    [SerializeField] protected Transform bottomOffset;
    [SerializeField] private Material mWhiteFlash;
    private Material mDefault;
    protected Base_BossController bossController;

    [Space(10)]
    [SerializeField] public bool DEBUGMODE = false;
    [SerializeField] protected float spawnFXScale = 2.5f; //2.5f default 
    [Header("*Animation Times")]
    [SerializeField] protected float[] fullAttackAnimTime; //1f, 1.416667f
    [SerializeField] protected float[] attackDelayTime; //0.0834f, 0.834f

    [Space(10)]

    [Header("Start() Reference Initialization")]
    public Base_BossMovement movement;
    public Base_BossAnimator animator;
    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected HealthBar healthBar;
    [SerializeField] protected EnemyStageManager enemyStageManager;

    [Space(10)]

    //HealthBar

    //Stats
    [Header("=== STATS (char optional) ===")]
    [Header("--- Health ---")]
    public Base_Character character;
    [SerializeField] protected float maxHP;
    [SerializeField] protected float currentHP;
    [SerializeField] protected float defense = 0;
    [SerializeField] protected int totalXPOrbs = 20;
    
    [Header("--- Attack ---")]
    [SerializeField] public float[] attackDamage;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float attackEndDelay = 0;
    [SerializeField] protected float startAttackDelay = 0;

    [Header("--- Knock(back/up) ---")]
    [SerializeField] public float knockbackStrength = 1; //2 low, 4 moderate
    [SerializeField] public float knockupStrength = 2;
    [Space(10)]
    protected float timeSinceAttack;
    [SerializeField] public int currAttackIndex;
    //float critChance;
    //float critMultiplier;

    [Header("--- Status ---")]
    //Bools
    [SerializeField] public bool isAlive;
    [SerializeField] public bool isSpawning;
    [SerializeField] public int currentPhase;
    protected int numAttacks;
    public bool isStunned;
    public bool isAttacking;
    public bool playerToRight;
    Coroutine StunnedCO;
    protected Coroutine AttackingCO;
    private bool initialEnable;


    protected virtual void Awake()
    {
        initialEnable = true;
        sr = GetComponentInChildren<SpriteRenderer>();
        mDefault = sr.material;
        animator = GetComponentInChildren<Base_BossAnimator>();
        //collider = GetComponent<BoxCollider2D>();
        //playerLayer = GameObject.FindGameObjectWithTag("Player").GetComponent<LayerMask>();
        if (movement == null) movement = GetComponent<Base_BossMovement>();
        if (bossController == null) bossController = GetComponent<Base_BossController>();
        //Initiating base stats before modifiers

        if (character != null)
        {
            defense = character.Base_Defense;
            attackDamage[0] = character.Base_AttackDamage;
            attackSpeed = character.Base_AttackSpeed;
            // attackRange = character.Base_AttackRange;
            maxHP = character.Base_MaxHP;
        }

        currentHP = maxHP;
        isAlive = true;
        isStunned = false;
        if (healthBar == null) healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetHealth(maxHP);
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

        isAttacking = false;
        canAttack = true;
        currAttackIndex = 0; //TODO: randomize?
        currentPhase = 1;
        //Must be in Start(), because of player scene loading.
        //Awake() might work during actual build with player scene always being active before enemy scenes.
        enemyStageManager = transform.parent.parent.GetComponent<EnemyStageManager>();
    }

    protected virtual void OnEnable()
    {
        //Manual set, duration of SpawnIndicator SpawnIn
        //Toggle enemy before spawning in
        if(initialEnable) return;
        float spawnDelay = .5f;
        animator.PlayManualAnim(5, spawnDelay);
        StartCoroutine(SpawnCO(spawnDelay));
    }

    protected virtual void OnDisable()
    { 
        initialEnable = false; 
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isAlive || isSpawning) return; //Stops all updates if dead
        timeSinceAttack += Time.deltaTime;
        if (isStunned) return;
        if (isAttacking) return;

        AttackMoveCheck();
    }

    protected virtual IEnumerator SpawnCO(float delay)
    {
        isSpawning = true; //This prevents the enemy from attacking and taking damage
        // healthBar.gameObject.SetActive(false);
        // if(bottomOffset != null) //!~ no indicator
        //     InstantiateManager.Instance.Indicator.PlayIndicator(bottomOffset.position, 1, spawnFXScale);
        //Toggle SR off
        // sr.enabled = false;
        yield return new WaitForSeconds(delay);
        animator.PlayManualAnim(4, 1f);
        yield return new WaitForSeconds(1f); //Wake animation
        // sr.enabled = true;
        // HitFlash(delay);
        healthBar.gameObject.SetActive(true);
        //Toggle SR on
        yield return new WaitForSeconds(0.5f);
        isSpawning = false;
    }

    protected virtual void AttackMoveCheck()
    {
        float delay = attackSpeed + fullAttackAnimTime[currAttackIndex];
        if (timeSinceAttack <= delay) //air attacks not affected
        {
            movement.canMove = false;
        }
        else movement.canMove = true;
    }

    #region Attack Behavior Overrides

    public virtual void Attack()
    {
        if (!isAlive || isAttacking || isSpawning || !canAttack) return;
        if (timeSinceAttack <= attackSpeed) return;
    
        timeSinceAttack = 0;
        //Default behavior, use an override script for multiple custom attacks
        StartCoroutine(Attacking());
    }

    #endregion

    protected virtual IEnumerator Attacking()
    {
        //TODO: replace with individual attack behaviors
        //Allow flip for a little longer
        isAttacking = true;
        movement.canMove = false;
        movement.ToggleFlip(false);
        
        animator.PlayAttackAnim(fullAttackAnimTime[currAttackIndex]);

        yield return new WaitForSeconds(startAttackDelay);
        FacePlayer(); //Flip to faceplayer before attacking
        
        yield return new WaitForSeconds(attackDelayTime[currAttackIndex] - startAttackDelay);
        CheckHit();
        yield return new WaitForSeconds(fullAttackAnimTime[currAttackIndex] - attackDelayTime[currAttackIndex]);
        isAttacking = false;
        yield return new WaitForSeconds(attackEndDelay);
        movement.canMove = true;
        movement.ToggleFlip(true);
        
        currAttackIndex++;
        if(currAttackIndex >= attackPoint.Length) currAttackIndex = 0;
    }

    protected virtual void FacePlayer()
    {
        //Player behind enemy
        if (!bossController.playerDetect.playerDetectFront)
        {
            movement.ManualFlip(!movement.isFacingRight);
        }
    }

    public virtual void CheckHit(bool knockback = false, bool knockup = false)
    {
        Vector3 hitboxSize = new Vector3 (attackRangeX[currAttackIndex], attackRangeY[currAttackIndex], 0);
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackPoint[currAttackIndex].position, hitboxSize, 0f, playerLayer);
        
        foreach (Collider2D player in hitPlayers)
        {
            if (player.GetComponent<Base_PlayerCombat>() != null)
            {
                var p = player.GetComponent<Base_PlayerCombat>();
                p.TakeDamage(attackDamage[currAttackIndex]);
                if (knockback) p.GetKnockback(!playerToRight, knockbackStrength);
                if (knockup) p.GetKnockup(knockupStrength);
                //if (AttackFarBehavior != null) AttackFarBehavior.playerHit = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        for(int i=0; i<attackPoint.Length; i++)
        {
            Gizmos.DrawWireCube(attackPoint[i].position, new Vector3(attackRangeX[i], attackRangeY[i], 0));
        }
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

    void StopAttack(bool toggleFlip = false)
    {
        if (AttackingCO != null) StopCoroutine(AttackingCO);
        isAttacking = false;
        movement.canMove = true;
        movement.ToggleFlip(toggleFlip);
        //Cancels Attack animation
        animator.StopAttackAnimCO();
    }

#region TakeDamage, HitFlash, Die
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
        GetComponent<BoxCollider2D>().enabled = false;

        InstantiateManager.Instance.HitEffects.ShowKillEffect(hitEffectsOffset.position);
        InstantiateManager.Instance.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);

        //Base_EnemyAnimator checks for isAlive to play Death animation
        isAlive = false;
        if(enemyStageManager != null) enemyStageManager.UpdateEnemyCount();

        //Disable sprite renderer before deleting gameobject
        //sr.enabled = false;
        //Invoke("DeleteObj", .5f); //Wait for fade out to finish
    }

    protected virtual void DeleteObj()
    {
        //TODO: bosses won't be deleted, just switch to static death anim
        Destroy(gameObject);
    }
#endregion
}
