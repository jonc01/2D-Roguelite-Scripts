using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Base_PlayerCombat : MonoBehaviour
{

    [Header("References/Setup")]
    public Base_Character character;
    public Base_PlayerMovement movement;
    public AnimatorManager animator;
    [SerializeField] private LayerMask enemyLayer;
    //[SerializeField] private Transform attackPoint;
    //[SerializeField] private float attackRange;
    [SerializeField] TextPopupsHandler textPopups;
    [SerializeField] private Transform textPopupOffset;
    [SerializeField] HealthBar healthBar;
    [SerializeField] private bool showGizmos = false;

    //public Slider healthSlider; //TODO: replace
    public HitStop hitStop; //Stops time, hitStop animations are separate
    

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Material mWhiteFlash;
    private Material mDefault;

    [SerializeField] float attackMoveDelay = .0f;

    [SerializeField] Transform[] attackPoints;
    //G1(.56, .294), G2/A1(.437, .149), G3(.318, .144), A2(.479, .208)
    [SerializeField] float[] hitBoxLength; //.95f, 1.35f, 1.75f, 1.35f, 1.52f
    [SerializeField] float[] hitBoxHeight; //.63f, 0.25f, 0.25f, 0.25f, 0.28f

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


    [Space(10)]

    //Controls
    public bool allowInput = true; //toggled when player is stunned, or can't attack

    //Stats
    public float maxHP;
    public float currentHP;
    public float defense;
    public float kbResist; //Knockback resist

    [SerializeField]
    float attackDamage,
        attackSpeed,
        critChance,
        critMultiplier,
        knockbackStrength;

    //Attack Bools and Counters
    public int currAttackIndex; //Used to reference hitboxes
    public int currentAttack;
    public int currentAirAttack;
    float timeSinceAttack;
    float timeSinceAirAttack;
    public bool isAttacking;
    public bool isAirAttacking;

    bool dashImmune;

    //Bools
    public bool isStunned;
    public bool isAlive;
    public bool isKnockedback;

    //Coroutines - Stored to allow interrupts
    Coroutine AttackingCO;
    Coroutine StunnedCO;
    Coroutine KnockbackCO;


    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        mDefault = sr.material;

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
        currentHP = maxHP;
        if (healthBar != null) healthBar.SetHealth(maxHP);

        allowInput = true; //may not need (using Update return checks)

        isAttacking = false;
        isAirAttacking = false;
        currentAttack = 0;
        currentAirAttack = 0;
        currAttackIndex = 0;
        dashImmune = false;

        UpdateUI();
    }

    private void Update()
    {
        if (!isAlive) return;
        timeSinceAttack += Time.deltaTime;
        timeSinceAirAttack += Time.deltaTime;
        if (isStunned) return;

        if (Input.GetKeyDown(KeyCode.Y)) //TODO: temp testing
        {
            HealPlayer(20f);
        }

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
        float delay = attackSpeed + attackMoveDelay;
        if (timeSinceAttack <= delay) //air attacks not affected
        {
            movement.ToggleCanMove(false);
        }
        else movement.ToggleCanMove(true);
    }

    private void AirAttackMoveCheck()
    {
        if (timeSinceAirAttack <= .2f)//attackSpeed - .1f)
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
        if (timeSinceAirAttack > attackSpeed && !movement.isDashing)// && !isAttacking && !isAirAttacking) //allowInput
        {
            currentAirAttack++;

            if (currentAirAttack > totalNumAirAttacks) currentAirAttack = 1;

            if (timeSinceAirAttack > 2.0f) currentAttack = 1;

            //Air attacks
            currAttackIndex = currentAirAttack + 2; //[3-4]
            AttackingCO = StartCoroutine(AirAttacking(currentAirAttack));

            //Reset timer
            timeSinceAirAttack = 0.0f;
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
        //movement.ToggleCanMove(false); //Disable movement

        yield return new WaitForSeconds(attackDelay);
        CheckAttack(groundAttackDamageMultipliers[currIndex], attackAnimFull);

        yield return new WaitForSeconds(attackAnimEnd); //attackAnimEnd
        //movement.ToggleCanMove(true); //Enable movement
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

        foreach (Collider2D player in hitEnemies)
        {
            if (player.GetComponent<Base_EnemyCombat>() != null)
            {
                player.GetComponent<Base_EnemyCombat>().TakeDamage(damageDealt, true, knockbackStrength);
                HitStopAnim(attackAnimFull, groundAttack);

                if (isAirAttacking) movement.Float(.3f);
                //ScreenShakeListener.Instance.Shake(1); //TODO: if Crit
                //hitStop.Stop(.1f); //Successful hit
            }
        }
    }

    void HitStopAnim(float attackAnimFull, bool ground)
    {
        if (ground) animator.PlayAttackAnim(currentAttack, attackAnimFull, true);
        else animator.PlayAirAttackAnim(currentAirAttack, attackAnimFull, true);
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (attackPoints[currAttackIndex] == null) return;

            Gizmos.DrawWireCube(attackPoints[currAttackIndex].position, 
                new Vector3((hitBoxLength[currAttackIndex]),
                hitBoxHeight[currAttackIndex], 0));
        }
    }

    public void GetKnockback(bool enemyToRight, float strength = 8, float delay = .5f)
    {
        return; //TODO: TEMP, function needs testing
        if (!isAlive) return;
        KnockbackNullCheckCO();

        if (kbResist > 0) strength -= kbResist;
        if (strength <= 0) return;

        isKnockedback = true;
        Debug.Log("Knockback on player");
        //movement.ToggleFlip(false); //TODO; can just stun player
        //TODO: player canMove is toggled to false in attackCO, need to allow rb.velocity to change for knockback
        //GetStunned(.1f); 

        float temp = enemyToRight != true ? 1 : -1; //get knocked back in opposite direction of player
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
        //movement.ToggleFlip(true); //TODO: can just stun player
        isKnockedback = false;
    }

    void KnockbackNullCheckCO()
    {
        if (KnockbackCO == null) return;
        StopCoroutine(KnockbackCO);
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

    public void TakeDamage(float damageTaken)
    {
        if (!isAlive) return;
        if (dashImmune) return; //TODO: needs testing

        HitFlash();

        float totalDamage = damageTaken - defense;
        if(totalDamage <= 0)
        {
            totalDamage = 1; //Damage can never be lower than 1
        }

        textPopups.ShowDamage(totalDamage, textPopupOffset.position);

        //Shake screen based on how much damage is taken (% of max HP)
        float damageToHealth = damageTaken/maxHP;
        int shakeNum;

        if (damageToHealth >= .3f) shakeNum = 2;
        else if (damageToHealth >= .15f) shakeNum = 1;
        else shakeNum = 0;

        ScreenShakeListener.Instance.Shake(shakeNum);
        //

        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);
        CheckDie();
    }

    public void HealPlayer(float healAmount)
    {
        if (!isAlive) return;

        textPopups.ShowHeal(healAmount, textPopupOffset.position);

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
        hitStop.Stop(.4f);

        //Stop Coroutines
        //if (AttackingCO != null) StopCoroutine(AttackingCO);
        //if (StunnedCO != null) StopCoroutine(StunnedCO);
        StopAllCoroutines();

        movement.ToggleCanMove(false);
        
        isAlive = false;
    }
}
