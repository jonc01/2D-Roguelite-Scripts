using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_EnemyCombat : MonoBehaviour
{
    [Header("References/Setup")]
    public Base_EnemyMovement movement;
    public Base_EnemyAnimator animator;
    [SerializeField] private EnemyStageManager enemyStageManager;
    public OrbHolder orbHolder;
    [SerializeField] private Transform attackPoint;
    public LayerMask playerLayer;
    [SerializeField] private Transform textPopupOffset;
    [SerializeField] private Transform hitEffectsOffset;
    [SerializeField] HealthBar healthBar;
    public Transform healthbarTransform;
    [SerializeField] private BoxCollider2D collider;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Material mWhiteFlash;
    private Material mDefault;

    [Space(10)]
    [SerializeField] public bool DEBUGMODE = false;

    [Header("Required Animations Setup")]
    public float attackAnimDelayFrames = .1f;
    public float attackAnimTotalFrames = 1f;
    public float sampleRate = 12f;
    [Header("*Don't Edit* Animation Results")]
    [SerializeField] float fullAttackAnimTime;
    [SerializeField] float attackDelayTime;

    [Space(10)]

    [Header("Start() Reference Initialization")]
    [SerializeField] TextPopupsHandler textPopups;
    [SerializeField] HitEffectsHandler hitEffects;

    [Space(10)]

    //HealthBar
    
    //Stats
    [Header("=== Stats (char optional) ===")]
    public Base_Character character;
    [SerializeField] float maxHP;
    [SerializeField] float currentHP;
    [SerializeField] float defense = 0;

    [SerializeField] float attackDamage;
    [SerializeField] float attackSpeed;
    [SerializeField] float startAttackDelay = 0;
    public float attackRange;
    [Space(10)]
    float timeSinceAttack;
    //float critChance;
    //float critMultiplier;

    //Bools
    [SerializeField] public bool isAlive;
    public bool isStunned;
    public bool isKnockedback;
    public bool isAttacking;
    public bool playerToRight;
    public float kbResist = 0;
    Coroutine StunnedCO;
    Coroutine KnockbackCO;
    Coroutine AttackingCO;


    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        mDefault = sr.material;
        collider = GetComponent<BoxCollider2D>();

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

    private void Start()
    {
        if (DEBUGMODE)
        {
            maxHP *= 100;
            currentHP = maxHP;
        }

        isAttacking = false;
        //Must be in Start(), because of player scene loading. 
        //Awake() might work during actual build with player scene always being active before enemy scenes.
        textPopups = GameObject.FindGameObjectWithTag("TextPopupsHandler").GetComponent<TextPopupsHandler>();
        hitEffects = GameObject.FindGameObjectWithTag("HitEffectsHandler").GetComponent<HitEffectsHandler>();
        if(orbHolder == null) orbHolder = GameObject.FindGameObjectWithTag("XPOrbs").GetComponent<OrbHolder>();
        enemyStageManager = GetComponentInParent<EnemyStageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) return; //Stops all updates if dead
        timeSinceAttack += Time.deltaTime;
        if (isStunned) return;

        AttackMoveCheck();
    }

    void AttackMoveCheck()
    {
        float delay = attackSpeed + fullAttackAnimTime;
        if (timeSinceAttack <= delay) //air attacks not affected
        {
            movement.canMove = false;
        }
        else movement.canMove = true;
    }

    public void Attack()
    {
        if (!isAlive || isAttacking) return;
        if(timeSinceAttack > attackSpeed)
        {
            timeSinceAttack = 0;
            AttackingCO = StartCoroutine(Attacking());
        }
    }

    IEnumerator Attacking()
    {
        yield return new WaitForSeconds(startAttackDelay);
        isAttacking = true;
        movement.canMove = false;
        animator.PlayAttackAnim(fullAttackAnimTime);
        movement.ToggleFlip(false);
        yield return new WaitForSeconds(attackDelayTime);
        CheckHit();
        yield return new WaitForSeconds(fullAttackAnimTime - attackDelayTime);
        isAttacking = false;
        movement.canMove = true;
        movement.ToggleFlip(true);
    }

    public void CheckHit()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach(Collider2D player in hitPlayers)
        {
            if (player.GetComponent<Base_PlayerCombat>() != null)
            {
                player.GetComponent<Base_PlayerCombat>().TakeDamage(attackDamage);
                //knockback
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void GetStunned(float stunDuration = .5f)
    {
        if (!isAlive) return;

        //TODO: 
        //StopAllCoroutines(); //This could allow stun locks, depending on how often player can apply stun
        StopAttack();
        StunnedCO = StartCoroutine(Stunned(stunDuration));
    }

    IEnumerator Stunned(float stunDuration)
    {
        isStunned = true;
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    public void GetKnockback(bool playerToRight, float strength = 8, float delay = .5f)
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
        if(AttackingCO != null) StopCoroutine(AttackingCO);
        isAttacking = false;
        movement.canMove = true;
        movement.ToggleFlip(toggleFlip);
        //Cancels Attack animation
        animator.StopAttackAnimCO();
    }

    public void TakeDamage(float damageTaken, bool knockback = false, float strength = 8)
    {
        if (!isAlive) return;

        HitFlash(); //Set material to white, short delay before resetting
        
        float totalDamage = damageTaken - defense;
        if (totalDamage <= 0)
        {
            totalDamage = 1; //Damage can never be lower than 1
        }

        hitEffects.ShowHitEffect(hitEffectsOffset.position);
        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);

        if (knockback) GetKnockback(playerToRight, strength);

        //Display Damage number
        if (textPopups != null) textPopups.ShowDamage(totalDamage, textPopupOffset.position);

        if (currentHP <= 0)
        {
            isAlive = false;
            Die();
        }
    }

    void HitFlash()
    {
        sr.material = mWhiteFlash;
        Invoke("ResetMaterial", .1f);
    }

    void ResetMaterial()
    {
        sr.material = mDefault;
    }

    void Die()
    {
        healthBar.gameObject.SetActive(false);
        if(AttackingCO != null) StopCoroutine(AttackingCO);

        ScreenShakeListener.Instance.Shake(2);
        movement.rb.simulated = false;
        collider.enabled = false;

        if (orbHolder != null) orbHolder.Launch(playerToRight);

        //Base_EnemyAnimator checks for isAlive to play Death animation
        isAlive = false;
        enemyStageManager.UpdateEnemyCount();
    }
}
