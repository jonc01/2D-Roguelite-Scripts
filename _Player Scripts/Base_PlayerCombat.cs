using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Base_PlayerCombat : MonoBehaviour
{

    [Header("- References/Setup -")]
    public Base_Character character;
    public Base_PlayerMovement movement;
    public AnimatorManager animator;
    [SerializeField] private PlayAudioClips playAudioClips;
    [SerializeField] private AugmentInventory augmentInventory;
    [SerializeField] private LayerMask enemyLayer;
    //[SerializeField] private Transform attackPoint;
    //[SerializeField] private float attackRange;
    [SerializeField] private Transform textPopupOffset;
    [SerializeField] protected HealthBar healthBar;
    [SerializeField] private bool showGizmos = false;

    public HitStop hitStop; //Stops time, hitStop animations are separate
    
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Material mWhiteFlash;
    private Material mDefault;

    [Space(10)]
    [Header("- Hitboxes -")]
    [SerializeField] protected float attackMoveDelay = .0f;
    [SerializeField] protected Transform[] attackPoints;
    //G1(.56, .294), G2/A1(.437, .149), G3(.318, .144), A2(.479, .208)
    [SerializeField] float[] hitBoxLength; //.95f, 1.35f, 1.75f, 1.35f, 1.52f
    [SerializeField] float[] hitBoxHeight; //.63f, 0.25f, 0.25f, 0.25f, 0.28f
    
    [Space(10)]
    [Header("- Parry -")]
    [SerializeField] protected Transform parryPoint;
    [SerializeField] protected float parryHitBoxLength; //.64f
    [SerializeField] protected float parryHitboxHeight; //.61f   
    [SerializeField] protected float parryHitAddProcChance = .2f;
    [SerializeField] protected float parryShieldDuration = .3f;
    [SerializeField] GameObject parryImmuneTextUI;
    [SerializeField] GameObject parryImmuneShield;

    [Space(10)]
    [Header("Ground Attack Animator Setup")]
    //array of attack anim time (ground)
    public int totalNumGroundAttacks;
    public float[] groundAttackDelayAnimTimes;
    public float[] groundAttackFullAnimTimes;
    public float[] groundAttackDamageMultipliers;
    [Header("Air Attack Animator Setup")]
    //array of attack anim time (air)
    public int totalNumAirAttacks;
    public float[] airAttackDelayAnimTimes;
    public float[] airAttackFullAnimTimes;
    public float[] airAttackDamageMultipliers;

    [Header("Block Animator Setup")]
    public float blockFullAnimTime;
    public float blockDelayAnimTime;
    public float blockDelay;
    public float blockDuration = 0.125f;
    private float blockDelayAndDuration;
    public bool parryDeflecting;
    [SerializeField] public GameObject[] blockIndicatorAnim;

    [Space(10)]

    //Controls
    public bool allowInput = true; //toggled when player is stunned, or can't attack

    //Stats
    public float maxHP;
    public float currentHP;
    public float defense;
    public float kbResist; //Knockback resist
    protected float base_kbResist;

    [SerializeField]
    public float attackDamage,
        attackSpeed,
        critChance,
        critMultiplier,
        knockbackStrength;

    [Space(10)]
    [Header("- Debugging - Base Stats")]
    [SerializeField] public float base_attackDamage;
    [SerializeField] public float base_attackSpeed;

    [Space(10)]
    //Attack Bools and Counters
    public int currAttackIndex; //Used to reference hitboxes
    public int currentAttack;
    public int currentAirAttack;
    protected float timeSinceAttack;
    protected float timeSinceAirAttack; //only for move check, not attack speed
    protected float timeSinceBlock;
    public bool isAttacking;
    public bool isAirAttacking;
    [SerializeField] public bool isParrying;
    [SerializeField] protected float parryShieldTimer;

    public float blockAttackSpeed;

    [Space(10)]
    [Header("- Bools -")]
    [SerializeField] protected bool dashImmune;
    [SerializeField] protected bool damageImmune;
    //Bools
    public bool isStunned;
    public bool isAlive;
    public bool isKnockedback;

    //Coroutines - Stored to allow interrupts
    Coroutine AttackingCO;
    Coroutine BlockingCO;
    Coroutine StunnedCO;
    Coroutine KnockbackCO;
    Coroutine KnockupCO;
    Coroutine KnockbackResetCO;

    protected void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        mDefault = sr.material;
        if(augmentInventory == null) augmentInventory = GameManager.Instance.AugmentInventory;
        if(playAudioClips == null) playAudioClips = GetComponentInChildren<PlayAudioClips>();

        isAlive = true;
        isStunned = false;

        if (character != null)
        {
            maxHP = character.Base_MaxHP;
            attackDamage = character.Base_AttackDamage;
            attackSpeed = character.Base_AttackSpeed;
            //attackRange = character.Base_AttackRange;
            critChance = character.Base_CritChance;
            critMultiplier = character.Base_CritMultiplier;
            knockbackStrength = character.Base_KnockbackStrength;
        }

        base_attackDamage = attackDamage;
        base_attackSpeed = attackSpeed;

        currentHP = maxHP;
        if (healthBar != null) healthBar.SetHealth(maxHP);

        allowInput = true; //may not need (using Update return checks)

        isAttacking = false;
        isAirAttacking = false;
        isParrying = false;
        parryDeflecting = false;
        parryShieldTimer = 0;
        currentAttack = 0;
        currentAirAttack = 0;
        currAttackIndex = 0;

        base_kbResist = kbResist;

        damageImmune = false;
        dashImmune = false;

        blockDelayAndDuration = blockDelay+blockDuration;

        UpdateUI();
    }

    private void Update()
    {
        if (!allowInput) return;
        if (!isAlive) return;
        timeSinceAttack += Time.deltaTime;
        timeSinceAirAttack += Time.deltaTime; //only for move check, not attack speed
        timeSinceBlock += Time.deltaTime;
        ParryShieldCheck();
        if (isStunned) return;

        if (movement.isDashing)
        {
            dashImmune = true;
            return;
        }

        AttackMoveCheck();
        AirAttackMoveCheck();

        dashImmune = false;
    }

    private void AttackMoveCheck()
    {
        //Delay after attacking to resume movement
        if(!movement.isGrounded) return;
        float delay = attackSpeed + attackMoveDelay;
        if (timeSinceAttack <= delay) //air attacks not affected
        {
            movement.ToggleCanMove(false);
        }
        else movement.ToggleCanMove(true);
    }

    private void AirAttackMoveCheck()
    {
        // if (timeSinceAttack <= .2f)//attackSpeed - .1f)
        if (timeSinceAirAttack <= .2f) //Separate from ground attack to prevent velocity being set to 0
        {
            movement.ToggleAirMove(false);
        }
        else movement.ToggleAirMove(true);
    }

    //Attacks
    public void Attack1()
    {
        if (!isAlive) return;
        if (timeSinceAttack > attackSpeed && !movement.isDashing)// && !isAttacking && !isAirAttacking) //allowInput
        {
            currentAttack++;

            //Cycle through 3 attacks
            if (currentAttack > totalNumGroundAttacks) currentAttack = 1;
            //Reset Attack after timer reaches 2s
            if (timeSinceAttack > 2.0f) currentAttack = 1;

            //Coroutine
            currAttackIndex = currentAttack - 1; //[0-2]
            AttackingCO = StartCoroutine(Attacking(currentAttack));
            
            //Reset timer
            timeSinceAttack = 0.0f;
        }
    }

    public void Attack2()
    {
        if (!isAlive) return;
        if (timeSinceAttack > attackSpeed && !movement.isDashing)// && !isAttacking && !isAirAttacking) //allowInput
        {
            currentAirAttack++;

            if (currentAirAttack > totalNumAirAttacks) currentAirAttack = 1;

            if (timeSinceAttack > 2.0f) currentAttack = 1;
            // if (timeSinceAirAttack > 2.0f) currentAttack = 1;

            //Air attacks
            currAttackIndex = currentAirAttack + 2; //[3-4]
            AttackingCO = StartCoroutine(AirAttacking(currentAirAttack));

            //Reset timer
            timeSinceAttack = 0.0f;
            timeSinceAirAttack = 0.0f; //only for move check, not attack speed
        }
    }

    IEnumerator Attacking(int currentAttack)
    {
        isAttacking = true;

        int currIndex = currentAttack - 1;
        float attackDelay = groundAttackDelayAnimTimes[currIndex];
        float attackAnimFull = groundAttackFullAnimTimes[currIndex];
        float attackAnimEnd = attackAnimFull - attackDelay;

        animator.PlayAttackAnim(currentAttack, attackAnimFull);
        playAudioClips.PlayAttackSwing();
        //Movement is being toggled in Update when isAttacking is toggled

        yield return new WaitForSeconds(attackDelay);
        CheckAttack(groundAttackDamageMultipliers[currIndex], attackAnimFull);

        yield return new WaitForSeconds(attackAnimEnd); //attackAnimEnd
        
        isAttacking = false;
    }

    IEnumerator AirAttacking(int currentAirAttack)
    {
        isAirAttacking = true;

        int currIndex = currentAirAttack - 1;
        float attackDelay = airAttackDelayAnimTimes[currIndex];
        float attackAnimFull = airAttackFullAnimTimes[currIndex];
        float attackAnimEnd = attackAnimFull - attackDelay;

        animator.PlayAirAttackAnim(currentAirAttack, attackAnimFull);
        playAudioClips.PlayAttackSwing();

        yield return new WaitForSeconds(attackDelay);
        CheckAttack(airAttackDamageMultipliers[currIndex], attackAnimFull, false);

        yield return new WaitForSeconds(attackAnimEnd);
        isAirAttacking = false;
    }

    void CheckAttack(float damageMultiplier, float attackAnimFull, bool groundAttack = true)
    {
        float damageDealt = attackDamage * damageMultiplier;
        //TODO: Roll crit

        Collider2D[] hitEnemies = 
            Physics2D.OverlapBoxAll(attackPoints[currAttackIndex].position,
            new Vector2(hitBoxLength[currAttackIndex], hitBoxHeight[currAttackIndex]), 0, enemyLayer);

        //if (damageMultiplier > 1) knockbackStrength = 6; //TODO: set variable defintion in Inspector

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                damageable.TakeDamage(damageDealt, true, true, knockbackStrength, transform.position.x);
                // Transform enemyPos = damageable.GetHitPosition();
                // augmentInventory.OnHit(enemyPos); //Calling from Enemy TakeDamage
                HitStopAnim(attackAnimFull, groundAttack);

                if(isAirAttacking) movement.Float(.3f);
                //ScreenShakeListener.Instance.Shake(1); //TODO: if Crit
                //hitStop.Stop(.1f); //Successful hit
            }
        }
    }

    public float GetMultipliedDamage()
    {
        return groundAttackDamageMultipliers[currentAttack - 1] * attackDamage;
    }

    void ParryCounter()
    {
        parryDeflecting = true;
        Invoke("ResetParrying", blockDuration);
        parryShieldTimer = parryShieldDuration;
        if(parryImmuneTextUI != null) parryImmuneTextUI.SetActive(true);
        if(parryImmuneShield != null) parryImmuneShield.SetActive(true);

        Collider2D[] hitEnemies = 
            Physics2D.OverlapBoxAll(parryPoint.position,
            new Vector2(parryHitBoxLength, parryHitboxHeight), 0, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                // damageable.TakeDamage(1, true, knockbackStrength, transform.position.x);
                damageable.TakeDamageStatus(1, 0);
                Transform enemyHitPos = damageable.GetHitPosition();
                augmentInventory.OnHit(enemyHitPos, parryHitAddProcChance);

                Transform enemyGroundPos = damageable.GetGroundPosition();
                augmentInventory.OnParry(enemyGroundPos);
                // InstantiateManager.Instance.ParryEffects.ShowHitEffect(parryPoint.position, transform.localScale.x);

                hitStop.Stop(); //Successful hit //.083f is 1 frame
            }
        }
    }

    void ResetParrying()
    {
        parryDeflecting = false;
    }

    void ParryShieldCheck()
    {
        if(parryShieldTimer > 0)
        {
            parryShieldTimer -= Time.deltaTime;
            kbResist = 999;
            damageImmune = true;
        }else
        {
            kbResist = base_kbResist;
            damageImmune = false;
            if(parryImmuneTextUI != null) parryImmuneTextUI.SetActive(false);
            if(parryImmuneShield != null) parryImmuneShield.SetActive(false);
        }
    }

    public void Block()
    {
        if (!isAlive) return;
        if (timeSinceBlock < blockAttackSpeed) return;
        if (timeSinceAttack > attackSpeed && !movement.isDashing)
        {
            //Coroutine
            BlockingCO = StartCoroutine(Blocking());
            
            //Reset timer
            timeSinceAttack = 0.0f;
        }


        bool successfulParry = false;
        // isParrying
        animator.PlayBlockAnim(blockFullAnimTime, successfulParry);
        timeSinceAttack = 0;
        timeSinceBlock = 0;
    }

    IEnumerator Blocking()
    {
        //float blockDelay;
        //float blockDuration;

        // blockAnimTime
        yield return new WaitForSeconds(blockDelay + blockDelayAnimTime); //0.0834f\
        isParrying = true;
        yield return new WaitForSeconds(blockDuration);
        isParrying = false;
        
        yield return new WaitForSeconds(blockFullAnimTime - blockDelayAndDuration);
    }

    void HitStopAnim(float attackAnimFull, bool ground)
    {
        //Interrupt current attack animation with alternate hitstop animation
        if (ground) animator.PlayAttackAnim(currentAttack, attackAnimFull, true);
        else animator.PlayAirAttackAnim(currentAirAttack, attackAnimFull, true);
    }

    public void ToggleIndicator(int index)
    {
        if(blockIndicatorAnim[index].activeSelf) return;
        blockIndicatorAnim[index].SetActive(true);
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (attackPoints[currAttackIndex] == null) return;

            Gizmos.DrawWireCube(attackPoints[currAttackIndex].position, 
                new Vector3(hitBoxLength[currAttackIndex],
                hitBoxHeight[currAttackIndex], 0));
            
            Gizmos.DrawWireCube(parryPoint.position, new Vector3(parryHitBoxLength, parryHitboxHeight, 0));
        }
    }

    // public void GetKnockback(bool enemyToRight, float strength = 4, float recoveryDelay = .15f)
    public void GetKnockback(float enemyXPos, float kbForce = 4, float recoveryDelay = .15f)
    {
        if (!isAlive || dashImmune) return;
        if(isParrying) return;
        KnockbackNullCheckCO();

        if (kbResist > 0) kbForce -= kbResist;
        if (kbForce <= 0) return;

        isKnockedback = true;

        bool kbToRight;
        kbToRight = enemyXPos < transform.position.x;
        // if(enemyXPos < transform.position.x) kbToRight = false;

        KnockbackCO = StartCoroutine(Knockback(kbToRight, kbForce, recoveryDelay));
        // float temp = enemyToRight != true ? 1 : -1; //get knocked back in opposite direction of player
        // Vector2 direction = new Vector2(temp, .3f);
        // movement.rb.AddForce(direction * strength, ForceMode2D.Impulse);
    }

    public void GetKnockback(bool kbToRight, float kbForce = 4, float recoveryDelay = .15f)
    {
        if (!isAlive || dashImmune) return;
        KnockbackNullCheckCO();

        if (kbResist > 0) kbForce -= kbResist;
        if (kbForce <= 0) return; //Full knockback resist

        isKnockedback = true;

        KnockbackCO = StartCoroutine(KnockbackManual(kbToRight, kbForce, recoveryDelay));
    }

    IEnumerator Knockback(bool kbToRight, float strength = 4, float recoveryDelay = .15f)
    {
        movement.StopVelocityX();
        yield return new WaitForSeconds(.02f); //need delay for physics to update
        // float temp = kbToRight != true ? 1 : -1; //get knocked back in opposite direction of player
        float kbDir;
        if(kbToRight) kbDir = 1;
        else kbDir = -1;

        // Vector2 direction = new Vector2(temp, .3f);
        // movement.rb.AddForce(direction * strength, ForceMode2D.Impulse);
        movement.rb.velocity = new Vector2(kbDir * strength, movement.rb.velocity.y);

        KnockbackResetCO = StartCoroutine(KnockbackReset(recoveryDelay));
    }

    IEnumerator KnockbackManual(bool kbToRight, float strength = 4, float recoveryDelay = .15f)
    {
        movement.StopVelocityX();
        yield return new WaitForSeconds(.02f); //need delay for physics to update
        float temp = kbToRight != true ? -1 : 1; //get knocked back in opposite direction of player
        Vector2 direction = new Vector2(temp, .3f);
        movement.rb.AddForce(direction * strength, ForceMode2D.Impulse);

        KnockbackResetCO = StartCoroutine(KnockbackReset(recoveryDelay));
    }

    public void GetKnockup(float strength = 4, float recoveryDelay = .1f)
    {
        if (!isAlive || dashImmune) return;
        KnockbackNullCheckCO();

        if (kbResist > 0) strength -= kbResist;
        if (strength <= 0) return;

        isKnockedback = true;
        KnockbackCO = StartCoroutine(Knockup(strength, recoveryDelay));
    }

    IEnumerator Knockup(float strength, float recoveryDelay)
    {
        // movement.StopVelocityX();
        movement.StopVelocityY();
        yield return new WaitForSeconds(.02f); //Short delay to reset velocity
        Vector2 knockUpDir = new Vector2(movement.rb.velocity.x, strength);
        movement.rb.AddForce(knockUpDir, ForceMode2D.Impulse);
        
        KnockbackResetCO = StartCoroutine(KnockbackReset(recoveryDelay));
    }

    IEnumerator KnockbackReset(float recoveryDelay = .15f)
    {
        //Wait recoveryDelay, then set velocity to 0 and allow the player to move again
        movement.canMove = false;
        yield return new WaitForSeconds(recoveryDelay);
        movement.rb.velocity = Vector3.zero;
        movement.canMove = true;
        //movement.ToggleFlip(true); //TODO: can just stun player
        isKnockedback = false;
    }

    void KnockbackNullCheckCO()
    {
        if (KnockbackResetCO == null) return;
        StopCoroutine(KnockbackResetCO);
        movement.canMove = true;
        //movement.ToggleFlip(true); //TODO: can just stun player
        isKnockedback = false;
    }


    public void GetStunned(float stunDuration)
    {
        //Interrupt player attack
        if (AttackingCO != null) StopCoroutine(AttackingCO);

        isAttacking = false;
        isAirAttacking = false;

        StunnedCO = StartCoroutine(StunTimer(stunDuration));
    }

    IEnumerator StunTimer(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    public void TakeDamage(float damageTaken, float enemyXPos, bool screenshake = false, int shakeNum = 1)
    {
        if (!isAlive || dashImmune) return;

        if(damageImmune)
        {
            InstantiateManager.Instance.TextPopups.ShowDamage(0, textPopupOffset.position);
            return;
        }
        
        bool damageFromRight = enemyXPos > transform.position.x;
        //Only allow parry if the Player is facing the enemy
        if(damageFromRight == movement.isFacingRight)
        {
            if (isParrying)
            {
                ScreenShakeListener.Instance.Shake(3);
                InstantiateManager.Instance.ParryEffects.ShowHitEffect(parryPoint.position, transform.localScale.x);
                ParryCounter();
                if(playAudioClips != null) playAudioClips.PlayBlockedAudio();
                InstantiateManager.Instance.TextPopups.ShowParry(textPopupOffset.position);
                return;
            }
        }

        if(screenshake) ScreenShakeListener.Instance.Shake(shakeNum);
        TakeDamage(damageTaken, screenshake);
    }

    public void TakeDamage(float damageTaken, bool screenshakeOR = false)
    {
        if (!isAlive || dashImmune) return;

        if(damageImmune)
        {
            InstantiateManager.Instance.TextPopups.ShowDamage(0, textPopupOffset.position);
            return;
        }

        HitFlash();

        float totalDamage = damageTaken - defense;
        if(totalDamage <= 0)
        {
            totalDamage = 1; //Damage can never be lower than 1
        }

        augmentInventory.OnDamageTaken();

        InstantiateManager.Instance.TextPopups.ShowDamage(totalDamage, textPopupOffset.position);
        InstantiateManager.Instance.HitEffects.ShowHitEffect(textPopupOffset.position);
        if(playAudioClips != null) playAudioClips.PlayHitAudio();

        //Shake screen based on how much damage is taken (% of max HP)
        float damageToHealth = damageTaken/maxHP;
        int shakeNum;

        if(!screenshakeOR)
        {
            if (damageToHealth >= .3f) shakeNum = 2;
            else if (damageToHealth >= .15f) shakeNum = 1;
            else shakeNum = 0;

            ScreenShakeListener.Instance.Shake(shakeNum);
        }
        //

        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);
        CheckDie();
    }

    public void TakeTrueDamage(float damageTaken)
    {
        if(!isAlive) return;

        currentHP -= damageTaken;
        healthBar.UpdateHealth(currentHP);
        CheckDie();
    }

    public void HealPlayer(float healAmount, bool showNumber = true)
    {
        if (!isAlive) return;

        if(showNumber)
            InstantiateManager.Instance.TextPopups.ShowHeal(healAmount, textPopupOffset.position);

        if (currentHP < maxHP) currentHP += healAmount;
        if (currentHP > maxHP) currentHP = maxHP;

        if (healthBar != null)
        {
            healthBar.SetHealth(maxHP);
            healthBar.UpdateHealth(currentHP);
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

    public void CancelAttack()
    {
        if (AttackingCO != null) StopCoroutine(AttackingCO);
    }

    void UpdateUI() //TODO: move to healthbar script
    {
        //if (healthNumbers != null) healthNumbers.text = currentHP + "/" + maxHP;
        /*if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }*/
    }

    void CheckDie()
    {
        if(currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isAlive = false; //Triggers death anim in AnimatorManager
        hitStop.Stop(1.2f);

        //Stop Coroutines
        //if (AttackingCO != null) StopCoroutine(AttackingCO);
        //if (StunnedCO != null) StopCoroutine(StunnedCO);
        StopAllCoroutines();

        movement.ToggleCanMove(false);
        
        isAlive = false;
        
        Invoke("DisplayRespawnPrompt", 1f);
    }

    void DisplayRespawnPrompt()
    {
        GameManager.Instance.ToggleRespawnScreen(true);
    }
}
