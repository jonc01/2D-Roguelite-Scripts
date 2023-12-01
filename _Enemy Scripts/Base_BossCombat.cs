using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_BossCombat : MonoBehaviour, IDamageable
{
    [SerializeField] bool DEBUGGING = false;
    [SerializeField] bool DEBUGHP = false;
    [Space(20)]

    [Header("=== Attack Behavior Setup ===")]
    [SerializeField] public Base_CombatBehavior AttackCloseBehavior;
    [SerializeField] public Base_CombatBehavior AttackFarBehavior;
    public bool canAttackFar;
    public bool canAttack;

    [Space(10)]

    [Header("== Attack Phases ==")]
    [SerializeField] protected int[] Phase1AtkPool;
    [SerializeField] protected int[] Phase2AtkPool;
    [SerializeField] protected int[] Phase3AtkPool;

    [Space(10)]

    [Header("=== References/Setup ===")]
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
    [SerializeField] protected PlayAudioClips playAudioClips;

    [Space(10)]
    [SerializeField] protected float spawnFXScale = 2.5f; //2.5f default 
    [Header("=== *Animation Times ===")]
    [SerializeField] protected float[] fullAttackAnimTime; //1f, 1.416667f
    [SerializeField] protected float[] attackDelayTime; //0.0834f, 0.834f

    [Space(10)]

    [Header("=== Start() Reference Initialization ===")]
    public Base_BossMovement movement;
    public Base_BossAnimator animator;
    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected HealthBar healthBar;
    [SerializeField] protected EnemyWaveManager enemyWaveManager;
    [SerializeField] protected float spawnDelay = 1f;
    protected InstantiateManager instantiateManager;

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
    [SerializeField] protected int totalHealOrbs = 10;
    
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
    [SerializeField] protected int currAttack;
    //float critChance;
    //float critMultiplier;

    [Header("--- Status ---")]
    //Bools
    [SerializeField] public bool isAlive;
    [SerializeField] public bool playDeathAnim;
    [SerializeField] public bool isSpawning;
    [SerializeField] public bool isSleeping;
    public bool isStunned;
    public bool isAttacking;
    public bool playerToRight;
    Coroutine StunnedCO;
    protected Coroutine AttackingCO;
    protected Coroutine AttackEndCO;
    [Header("--- Health Phases ---")]
    [SerializeField] public int currentPhase;
    [SerializeField] protected float[] healthPhase;
    protected bool changingPhase;
    [SerializeField] protected GameObject PhaseShield;
    [SerializeField] protected ToggleEffectAnimator PhaseShieldBreak;

    [Space(10)]

    [Header("=== Health Phase Adds ===")]
    [SerializeField] protected GameObject[] AddWave;

    [Space(10)]

    [Header("--- Attack Logic variables ---")]
    public bool attackClose;
    public bool attackMain;
    public bool playerInFront;
    public bool faceToWall;
    public bool backToWall;
    public float distanceToPlayer;
    public bool chasePlayer; //override to prevent Boss from chasing Player
    


    protected virtual void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        mDefault = sr.material;
        animator = GetComponentInChildren<Base_BossAnimator>();
        //collider = GetComponent<BoxCollider2D>();
        //playerLayer = GameObject.FindGameObjectWithTag("Player").GetComponent<LayerMask>();
        if (movement == null) movement = GetComponent<Base_BossMovement>();
        if (bossController == null) bossController = GetComponent<Base_BossController>();
        if (playAudioClips == null) playAudioClips = GetComponentInChildren<PlayAudioClips>();
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
        playDeathAnim = false;
        isStunned = false;
        if (healthBar == null) healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetHealth(maxHP);
            healthBar.gameObject.SetActive(false);
        }

        if(PhaseShield != null) PhaseShield.SetActive(false);

        //Defaults
        // fullAttackAnimTime = 1f;
        // attackDelayTime = 0.0834f;
    }

    protected virtual void Start()
    {
        #if UNITY_EDITOR
        //TEMP
        if (DEBUGHP)
        {
            maxHP *= 100;
            currentHP = maxHP;
        }
        #endif

        isAttacking = false;
        canAttack = true;
        currAttackIndex = 0;
        currAttack = 0;
        currentPhase = 0;
        changingPhase = false;
        //Must be in Start(), because of player scene loading.
        //Awake() might work during actual build with player scene always being active before enemy scenes.
        enemyWaveManager = transform.parent.GetComponent<EnemyWaveManager>();
        instantiateManager = InstantiateManager.Instance;
    }

    protected virtual void OnEnable()
    {
        //Manual set, duration of SpawnIndicator SpawnIn
        //Toggle enemy before spawning in
        isSpawning = true;
        isSleeping = true;
        if(DEBUGGING) StartSpawn();
    }

    public virtual void StartSpawn() //Manual call on 
    {
        AudioManager.Instance.playMusic.PlayBossMusic();
        StartCoroutine(SpawnCO(spawnDelay));
    }

    protected virtual void OnDisable()
    { 
        
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
        isSleeping = false;
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

    public virtual void Attack(int attackIndex)
    {
        if (!isAlive || isAttacking || isSpawning || !canAttack) return;
        if (timeSinceAttack <= attackSpeed) return;
    
        timeSinceAttack = 0;
        currAttackIndex = attackIndex;
        StartCoroutine(Attacking());
    }

    #endregion

    protected virtual void ShuffleAttackPools(int[] atkPoolArray)
    {
        for(int i = atkPoolArray.Length - 1; i > 0; i--)
        {
            //Random index
            int randIndex = Random.Range(0, i + 1); 

            //Swap values
            int temp = atkPoolArray[i];
            atkPoolArray[i] = atkPoolArray[randIndex];
            atkPoolArray[randIndex] = temp;
        }
    }

    protected virtual IEnumerator Attacking()
    {
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
                var playerObj = player.GetComponent<Base_PlayerCombat>();
                playerObj.TakeDamage(attackDamage[currAttackIndex]);
                if (knockback) playerObj.GetKnockback(transform.position.x, knockbackStrength);
                // if (knockback) playerObj.GetKnockback(!playerToRight, knockbackStrength);
                if (knockup) playerObj.GetKnockup(knockupStrength);
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

    public virtual void LungeCheck(float lungeStrength = .1f, float duration = .3f)
    {
        //Player is too close, lunge backwards
        if(attackClose)
            LungeStart(!playerToRight, lungeStrength);
        //Player is out of normal attack range, lunge forward
        else if(!attackMain && !attackClose)
            LungeStart(playerToRight, lungeStrength);
        // else attackMain, don't move
    }

    void LungeStart(bool lungeToRight, float duration = .3f)
    {
        float strength = 4f;
        movement.Lunge(lungeToRight, strength, duration);
    }

    public virtual void GetStunned(float stunDuration = .5f)
    {
        if (!isAlive || isSpawning) return;

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

    protected virtual void StopAttack(bool toggleFlip = false)
    {
        if (AttackingCO != null) StopCoroutine(AttackingCO);
        if (AttackEndCO != null) StopCoroutine(AttackEndCO);
        isAttacking = false;

        movement.canMove = true;
        movement.ToggleFlip(toggleFlip);
        //Cancels Attack animation
        animator.StopAttackAnimCO();
    }

#region Health Threshold and Phases
    protected virtual void HealthPhaseCheck() //Determine number of combo attacks and frequency
    {
        //HP is at the last Phase, no need to update
        if(currentPhase == healthPhase.Length-1) return;
        
        //Checking if health reaches the next phase threshold
        float nextHealthThreshold = healthPhase[currentPhase+1];
        if(currentHP <= nextHealthThreshold)
        {
            //Change to next Phase
            if(changingPhase) return;
            currentPhase++;
            StartCoroutine(ChangePhase());
        }
        attackEndDelay = 0.1f; //If no delay, attackSpeed delay still applies
    }

    protected virtual IEnumerator ChangePhase()
    {
        changingPhase = true;
        //Stop Attack and AttackEnd Coroutines
        StopAttack();

        //Toggle Shield gameobject and increase defenses
        PhaseShieldBreak.PlayAnim(0);
        yield return new WaitForSeconds(PhaseShieldBreak.GetAnimTime(0)); //anim delay before enabling shield

        PhaseShield.SetActive(true);
        float baseDefense = defense;
        defense = 999;
        movement.canMove = false;
        canAttack = false;

        CleanseDebuffs();

        //Spawn Additionals
        SpawnAddsWave();
        
        yield return new WaitForSeconds(1.5f);
        animator.PlayManualAnim(6, 1.083f); //Buff anim
        yield return new WaitForSeconds(0.667f); //Shorter time to pop shield

        //Toggle Shield gameobject and remove defenses
        PhaseShieldBreak.PlayAnim(1);
        PhaseShield.SetActive(false);
        defense = baseDefense;
        ScreenShakeListener.Instance.Shake(3);
        yield return new WaitForSeconds(PhaseShieldBreak.GetAnimTime(1));
        movement.canMove = true;
        isAttacking = false;
        canAttack = true;
        changingPhase = false;
    }

    protected virtual void CleanseDebuffs()
    {
        int totalChildren = transform.childCount;
        for(int i=0; i<totalChildren; i++)
        {
            Base_DamageOverTime currDebuff = transform.GetChild(i).GetComponent<Base_DamageOverTime>();

            if(currDebuff != null) currDebuff.CleanseDebuff();
            //make sure index is correct
        }
    }

    protected void SpawnAddsWave()
    {
        if(AddWave[currentPhase-1] == null) return;
        AddWave[currentPhase-1].SetActive(true);
    }

    protected void ClearAdds()
    {
        for(int i=0; i<AddWave.Length; i++)
        {
            int waveLength = AddWave[i].transform.childCount;
            for(int j=0; j<waveLength; j++)
            {
                Base_EnemyCombat add = AddWave[i].transform.GetChild(j).GetComponent<Base_EnemyCombat>();
                if(add != null) add.TakeDamageStatus(999, 3);
            }
        }
    }

#endregion

#region TakeDamage, HitFlash, Die
    public virtual void TakeDamage(float damageTaken, bool knockback = false, bool procOnHit = false, float strength = 8, float xPos = 0)
    {
        if (!isAlive || isSpawning) return;

        float totalDamage = damageTaken - defense;
        //Damage can never be lower than 1
        if (totalDamage <= 0)
        {
            if(changingPhase) totalDamage = 0;
            else totalDamage = 1;
        }

        HitFlash(); //Set material to white, short delay before resetting
        if(procOnHit) GameManager.Instance.AugmentInventory.OnHit(transform);
        //reduce hp
        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);

        if(playAudioClips != null)
        {
            if (totalDamage == 0) playAudioClips.PlayBlockedAudio();
            else playAudioClips.PlayHitAudio();
        }

        //Play hit effect, display Damage number
        if(instantiateManager != null)
        {
            instantiateManager.HitEffects.ShowHitEffect(hitEffectsOffset.position);
            instantiateManager.TextPopups.ShowDamage(totalDamage, textPopupOffset.position);
        }

        //Check Boss HP
        HealthPhaseCheck();

        if (currentHP <= 0)
        {
            isAlive = false;
            Die();
        }
    }

    public virtual void TakeDamageStatus(float damageTaken, int colorIdx)
    {
        // TakeDamageStatus(damageTaken, colorIdx);

        if (!isAlive || isSpawning) return;

        HitFlash(); //Set material to white, short delay before resetting

        float totalDamage = damageTaken - defense;

        //Damage can never be lower than 1
        if (totalDamage <= 0)
        {
            if(changingPhase) totalDamage = 0;
            else totalDamage = 1;
        }

        //Display Damage number
        if(instantiateManager != null)
        {
            instantiateManager.HitEffects.ShowHitEffect(hitEffectsOffset.position);
            instantiateManager.TextPopups.ShowStatusDamage(totalDamage, textPopupOffset.position, colorIdx);
        }

        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);
        if(playAudioClips != null) playAudioClips.PlayHitAudio();

        HealthPhaseCheck();

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
        return transform;
        // return bottomOffset;
    }

    protected void HitFlash(float resetDelay = .1f)
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
        canAttack = false;
        isAlive = false;

        ClearAdds();

        AudioManager.Instance.playMusic.TransitionBossMusicToNormal();

        //Attack Coroutine checks
        StopAttack();
        StopAllCoroutines();

        ScreenShakeListener.Instance.Shake(2);
        movement.rb.simulated = false;
        GetComponent<BoxCollider2D>().enabled = false;

        //Show death effects then spawn XP Orbs
        if(instantiateManager != null)
        {
            instantiateManager.HitEffects.ShowKillEffect(hitEffectsOffset.position);
            instantiateManager.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);
        }

        if(playAudioClips != null) playAudioClips.PlayDeathSound();

        //Base_EnemyAnimator checks for playDeathAnim to play Death animation
        playDeathAnim = true;
        if(enemyWaveManager != null) enemyWaveManager.UpdateEnemyCount();

        GameManager.Instance.BossCleared();

        Invoke("ToggleHitbox", 1f); //Delay rb and collider toggle
        //Disable sprite renderer before deleting gameobject
        //sr.enabled = false;
    }

    void ToggleHitbox()
    {
        movement.rb.simulated = false;
        GetComponent<BoxCollider2D>().enabled = false;
    }
#endregion
}
