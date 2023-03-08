using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColossalBoss_EnemyCombat : Base_BossCombat
{
    //float attackEndDelay = .5f;
    [Space(10)]
    //2D array with attack pools for each phase
    [Header("= Colossal Boss =")]
    [SerializeField] private int[] Phase1AtkPool;
    [SerializeField] private int[] Phase2AtkPool;
    [SerializeField] private int[] Phase3AtkPool;

    [Header("= Colossal Boss = : (0) RangeAttack")]
    [SerializeField] GameObject RangeAttackExplosionPrefab;
    [SerializeField] Transform bossGroundOffset;

    [Header("= Colossal Boss = : (1) Melee/Explosion")]
    [SerializeField] GameObject MeleePrefab; //Toggled GameObject
    [SerializeField] PrefabHandler ExplosionHandler;
    [SerializeField] float explosionCastDelay = .2f;

    [Header("= Colossal Boss = : (2) SuperAttack")]
    [SerializeField] GameObject SuperAttackExplosionPrefab;

    [Header("= Colossal Boss = : (3) Spin")]
    [SerializeField] GameObject BoomerangArms; //Toggled GameObject

    [Header("= Colossal Boss = : (4) ChargeUp")]
    [SerializeField] private bool canFly;
    [SerializeField] Vector3 initialGroundOffset;
    [SerializeField] float flyingOffsetY = 1f;
    [SerializeField] Vector3 flyingOffsetPos;
    private float parentObjX;
    private float originalScale;
    private float originalDrag;

    protected override void Awake()
    {
        base.Awake();
        originalScale = movement.rb.gravityScale;
        originalDrag = movement.rb.drag;
    }

    protected override void Start()
    {
        base.Start();
        canFly = false;
        initialGroundOffset = bossGroundOffset.position;
        flyingOffsetPos = transform.position;
        flyingOffsetPos.y += flyingOffsetY;

        animator.PlayManualAnim(5, 1f); 
        //Starts the Boss on the Sleep animation, prevents Idle to Sleep animation
    }

    protected override void OnEnable()
    {
        //Call the Sleep animation again to prevent Idle from taking over
        animator.PlayManualAnim(5, 2f);
        base.OnEnable();
        parentObjX = transform.parent.transform.position.x;
    }

    void FixedUpdate()
    {
        if(!canFly) return;
        
        //
        var step = movement.moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, flyingOffsetPos, step);

        if(Vector3.Distance(transform.position, flyingOffsetPos) < 0.1f)
        {
            //Stop moving
            movement.canMove = false;
            canFly = false;
        }
    }

    protected override void FacePlayer()
    {
        // if(ColossalBoss_Controller.playerDetect)
    }

    public override void Attack()
    {
        if (!isAlive || isSpawning || !canAttack) return;
        if (timeSinceAttack <= attackSpeed) return;

        StartCoroutine(AttackCO());
    }

    public override void Attack(int attackIndex)
    {
        if (!isAlive || isSpawning || !canAttack) return;
        if (timeSinceAttack <= attackSpeed) return;
    
        StartCoroutine(ManualAttackCO(attackIndex));
    }
    
    IEnumerator AttackCO()
    {
        // ThresholdCheck(); //TODO: moving to TakeDamage

        // if (currentPhase != activePhase) UpdateThresholdCheck(); //TODO: testing
        // while(currentPhase != activePhase) yield return null;

        timeSinceAttack = 0;
        canAttack = false;
        
        int randIndex;
        //Get random attack index
        if(currentPhase == 0){
            randIndex = Random.Range(0, Phase1AtkPool.Length);
            currAttackIndex = Phase1AtkPool[randIndex];
        }else if(currentPhase == 1){
            randIndex = Random.Range(0, Phase2AtkPool.Length);
            currAttackIndex = Phase2AtkPool[randIndex];
        }else{
            randIndex = Random.Range(0, Phase3AtkPool.Length);
            currAttackIndex = Phase3AtkPool[randIndex];
        }

        switch(currAttackIndex)
        {
            case 0:
                yield return AttackingCO = StartCoroutine(RangeAttack());
                break;
            case 1:
                yield return AttackingCO = StartCoroutine(MeleeExplosion(4));
                break;
            case 2:
            //TODO: needs Init setup
                yield return AttackingCO = StartCoroutine(SuperAttack()); //<50% hp
                break;
            case 3:
            //TODO: needs Init setup
                yield return AttackingCO = StartCoroutine(MeleeSpin()); //<50% hp, increased freq
                break;
            case 4:
                yield return AttackingCO = StartCoroutine(ChargeUp());
                break;
            default:
                // AttackingCO = StartCoroutine(MeleeExplosion());
                break;
        }
        // }
        yield return AttackEndingCO = StartCoroutine(AttackEnd());
    }

    IEnumerator ManualAttackCO(int attackIndex)
    {
        timeSinceAttack = 0;
        canAttack = false;
        switch(attackIndex)
        {
            case 0:
                yield return AttackingCO = StartCoroutine(RangeAttack());
                break;
            case 1:
                yield return AttackingCO = StartCoroutine(MeleeExplosion(4));
                break;
            case 2:
                yield return AttackingCO = StartCoroutine(SuperAttack()); //<50% hp
                break;
            case 3:
                yield return AttackingCO = StartCoroutine(MeleeSpin()); //<50% hp, increased freq
                break;
            case 4:
                yield return AttackingCO = StartCoroutine(ChargeUp());
                break;
            default:
                // AttackingCO = StartCoroutine(MeleeExplosion());
                break;
        }
        // if(AttackEndingCO != null) StopCoroutine(AttackEndingCO); //TODO:
        yield return AttackEndingCO = StartCoroutine(AttackEnd());
    }

#region Health Threshold and Phases
    void HealthPhaseCheck() //Determine number of combo attacks and frequency
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

    IEnumerator ChangePhase()
    {
        changingPhase = true;
        //Stop Attack and AttackEnd Coroutines
        StopAttack();

        Debug.Log("Activating Shield, no attack");
        //Toggle Shield gameobject
        //Shield.SetActive(true);
        float baseDefense = defense;
        defense = 999;
        movement.canMove = false;
        canAttack = false;
        yield return new WaitForSeconds(5);
        Debug.Log("Deactivating Shield, yes attack");
        // animator.PlayManualAnim(); //Buff
        //Shield.SetActive(false);
        defense = baseDefense;
        movement.canMove = true;
        isAttacking = false;
        yield return new WaitForSeconds(.1f);
        canAttack = true;
        changingPhase = false;
    }

#endregion

#region Attack Coroutines
//Attack[0]: Shoots at the ground in front
    IEnumerator RangeAttack() //0
    {
        movement.canMove = false;
        movement.DisableMove();

        isAttacking = true;
        yield return new WaitForSeconds(startAttackDelay);
        movement.ToggleFlip(true);

        //Attack 1
        animator.PlayManualAnim(0, fullAttackAnimTime[0]);
        LungeCheck(3f);
        yield return new WaitForSeconds(attackDelayTime[0]);// - startAttackDelay);
        movement.ToggleFlip(false);
        Instantiate(RangeAttackExplosionPrefab, transform.position, transform.rotation);
        // yield return new WaitForSeconds(fullAttackAnimTime[0] - attackDelayTime[0]);
        yield return new WaitForSeconds(.3f);

        //Attack 2
        animator.PlayManualAnim(7, fullAttackAnimTime[5]);
        // movement.ToggleFlip(true);
        LungeCheck(2.5f);
        yield return new WaitForSeconds(attackDelayTime[5]);
        movement.ToggleFlip(false);
        Instantiate(RangeAttackExplosionPrefab, transform.position, transform.rotation);
        yield return new WaitForSeconds(fullAttackAnimTime[5] - attackDelayTime[5]);
        isAttacking = false;
    }

//Attack[1]: Punch ground and spawn a wave of explosions forward
    IEnumerator MeleeExplosion(int numExplosions = 3) //1
    {
        movement.canMove = false;
        movement.DisableMove();
        isAttacking = true;
        movement.ToggleFlip(true);
        //Start melee animation
        animator.PlayManualAnim(1, fullAttackAnimTime[1]);
        yield return new WaitForSeconds(startAttackDelay); //delay before starting attack
        // FacePlayer();
        movement.ToggleFlip(false);

        LungeCheck(4f);

        yield return new WaitForSeconds(attackDelayTime[1]);// - startAttackDelay);
        if (MeleePrefab != null) MeleePrefab.SetActive(true);
        CheckHit(true, true); //Check Melee hit

        StartCoroutine(MeleeExplosionOnly(numExplosions));

        yield return new WaitForSeconds(fullAttackAnimTime[1] - attackDelayTime[1]);
        yield return new WaitForSeconds(attackEndDelay);
        isAttacking = false;
    }

//Prefab: Pass in number of explosions to spawn, 
//  change trackPlayer to Instantiate at Player or in a line after the previous position
    IEnumerator MeleeExplosionOnly(int iterations, bool trackPlayer = false, bool multiple = false)
    {
        if(iterations <= 0) yield return null;
        Vector3 castPos;
        //Spawn multiple explosions in a wave
        for(int i=0; i<iterations; i++)
        {
            if(!isAlive) break;
            if(!trackPlayer)
            {
                castPos = GetOwnPosX();
                if(movement.isFacingRight) castPos.x += i;
                else castPos.x -= i;
            }
            else
            {
                castPos = GetPlayerPosX();
            }

            ExplosionHandler.SpawnPrefab(castPos);
            // Instantiate(MeleeExplosionPrefab, castPos, Quaternion.identity);
            if(multiple)
            {
                yield return new WaitForSeconds(.2f);
                Vector3 castPos1 = new Vector3(castPos.x+1, castPos.y, 0);
                ExplosionHandler.SpawnPrefab(castPos1);
                // Instantiate(MeleeExplosionPrefab, castPos1, Quaternion.identity);
                Vector3 castPos2 = new Vector3(castPos.x-1, castPos.y, 0);
                ExplosionHandler.SpawnPrefab(castPos2);
                // Instantiate(MeleeExplosionPrefab, castPos2, Quaternion.identity);

                yield return new WaitForSeconds(.2f);
                Vector3 castPos3 = new Vector3(castPos.x+2, castPos.y, 0);
                ExplosionHandler.SpawnPrefab(castPos3);
                // Instantiate(MeleeExplosionPrefab, castPos3, Quaternion.identity);
                Vector3 castPos4 = new Vector3(castPos.x-2, castPos.y, 0);
                ExplosionHandler.SpawnPrefab(castPos4);
                // Instantiate(MeleeExplosionPrefab, castPos4, Quaternion.identity);
            }

            yield return new WaitForSeconds(explosionCastDelay);
        }
    }

//Attack[2]: 
    IEnumerator SuperAttack() //2
    {
        movement.canMove = false;
        movement.DisableMove();

        isAttacking = true;
        yield return new WaitForSeconds(startAttackDelay);
        movement.ToggleFlip(true);

        //Attack 1
        animator.PlayManualAnim(2, fullAttackAnimTime[2]);
        LungeCheck(3f);
        yield return new WaitForSeconds(attackDelayTime[2]);
        movement.ToggleFlip(false);
        Instantiate(SuperAttackExplosionPrefab, transform.position, transform.rotation);
        yield return new WaitForSeconds(.35f);

        //Attack 2
        animator.PlayManualAnim(8, fullAttackAnimTime[6]); //Alternate animation
        LungeCheck(3f);
        yield return new WaitForSeconds(attackDelayTime[6]);
        movement.ToggleFlip(false);
        Instantiate(SuperAttackExplosionPrefab, transform.position, transform.rotation);
        yield return new WaitForSeconds(fullAttackAnimTime[6] - attackDelayTime[6]);
        isAttacking = false;

        // Instantiate(SuperAttackExplosionPrefab, transform.position, Quaternion.identity);
    }

//Attack[3]: Boss moves towards a wall then launches its arms as a boomerang towards 
//  the opposite wall. The arms can hit multiple times, but have a cooldown per hit
    IEnumerator MeleeSpin() //3
    {
        isAttacking = true;

        movement.canMove = false;
        movement.DisableMove();
        //Move Boss to the further side of the room
        bool moveToRightWall; //= (Random.value > 0.5f); //randomize

        //Move to the furthest wall, checking x position
        if(transform.position.x < parentObjX) moveToRightWall = true;
        else moveToRightWall = false;

        //Disable chasePlayer logic
        chasePlayer = false;
        movement.canMove = true;
        yield return MoveToWall(moveToRightWall);

        ManualFlip(!moveToRightWall); //Boss will have back to the wall and face outward
        movement.canMove = false;
        yield return new WaitForSeconds(0.2f);
        
        //Charge attack after moving to the side of the room
        animator.PlayManualAnim(3, fullAttackAnimTime[3]);
        yield return new WaitForSeconds(attackDelayTime[3]);

        //Charge done, spawn/enable separate Arms object
        BoomerangArms.SetActive(true);

        while(BoomerangArms.activeInHierarchy)
        {
            //Play armless animation until the arms gameobject is inactive
            animator.PlayManualAnim(9, .1f);
            yield return null;
        }

        //Play spin animation one more time then play end of charge
        animator.PlayManualAnim(3, fullAttackAnimTime[3]);
        yield return new WaitForSeconds(attackDelayTime[3]);
        //Charge end animation
        animator.PlayManualAnim(10, .25f);
        yield return new WaitForSeconds(.25f);

        yield return new WaitForSeconds(attackEndDelay + 0.1f);
        chasePlayer = true;
        isAttacking = false;
    }

    IEnumerator MoveToWall(bool moveRight)
    {
        ManualFlip(moveRight);
        float startingMoveSpeed = movement.moveSpeed;
        movement.moveSpeed *= 3f;
        yield return new WaitForSeconds(.1f); //short delay to give Boss time to move if already near a wall
        
        while(!faceToWall)
        {
            movement.MoveRight(moveRight);
            yield return null;
        }
        yield return new WaitForSeconds(.1f);
        movement.moveSpeed = startingMoveSpeed;
    }

//Attack[4]: 
    IEnumerator ChargeUp(int iterations = 3)
    {
        isAttacking = true;

        movement.rb.gravityScale = 0;
        movement.rb.drag = 0;
        canFly = true;

        yield return new WaitForSeconds(.2f);
        movement.canMove = false;
        movement.DisableMove();

        for(int i=0; i<iterations; i++)
        {
            animator.PlayManualAnim(6, fullAttackAnimTime[4]);
            yield return new WaitForSeconds(attackDelayTime[4]);
            movement.ToggleFlip(false);
            StartCoroutine(MeleeExplosionOnly(1, true, true));
            yield return new WaitForSeconds((fullAttackAnimTime[4] - attackDelayTime[4])+.1f);
            movement.ToggleFlip(true);
        }

        yield return new WaitForSeconds(.5f);
        movement.rb.gravityScale = originalScale;
        movement.rb.drag = originalDrag;

        yield return new WaitForSeconds(attackEndDelay);
        isAttacking = false;
    }

//Attack: if Player is too close
    void ManualFlip(bool faceRight)
    {
        movement.ManualFlip(faceRight);
        movement.ToggleFlip(false);
    }

//Ending Coroutine
    IEnumerator AttackEnd()
    {
        //Use next attack, reset counter if out of bounds
        if(currAttackIndex >= attackPoint.Length) currAttackIndex = 0;

        // float nextAttackDelay;
        // // if(currAttackIndex == 0) nextAttackDelay = 0;
        // else 
        // nextAttackDelay = attackSpeed;

        yield return new WaitForSeconds(attackSpeed + .01f);
        movement.ToggleFlip(false);
        movement.canMove = true;
        canAttack = true;
    }

    public override void LungeCheck(float lungeStrength = 4f, float duration = .3f)
    {
        // Increased lunge strength to catch Player if too far
        if(distanceToPlayer > 1.6f) lungeStrength = distanceToPlayer*1.8f;
        movement.ToggleFlip(false);
        
        if(backToWall)
        { //Back to wall and Player is close behind/under the Boss
            if(attackClose || attackMain)
            {
                if(currAttackIndex == 0) lungeStrength += 2f;
                else if(currAttackIndex == 1) lungeStrength += 2f;
                else if(currAttackIndex == 2) lungeStrength += 2f;
                //Lunge away then flip to Attack
                LungeStart(movement.isFacingRight, lungeStrength, duration);
            }else{
                if(currAttackIndex == 0) lungeStrength += 2f;
                else if(currAttackIndex == 2) lungeStrength += 2.2f;
                LungeStart(playerToRight, lungeStrength, duration);
            }
        }
        else
        {
            if(!playerInFront)
            { //Player is behind Boss
                //Player is either close or in range
                if(attackClose || attackMain)
                {
                    if(currAttackIndex == 0) lungeStrength += 2f;
                    else if(currAttackIndex == 1) lungeStrength += 2f;
                    else if(currAttackIndex == 2) lungeStrength += 2.2f;

                    LungeStart(!playerToRight, lungeStrength, duration);
                }
                else{
                    LungeStart(playerToRight, lungeStrength, duration);
                }
            }
            else
            { //Player is in front
                if(attackClose)
                {
                    if(currAttackIndex == 0) lungeStrength += 1f;
                    else if(currAttackIndex == 2) lungeStrength += 2f;

                    //Player is too close, lunge backwards, lungeStrength based on Attack
                    LungeStart(!playerToRight, lungeStrength, duration);
                }
                //Player is out of normal attack range, lunge forward
                else if(!attackMain && !attackClose)
                {
                    LungeStart(playerToRight, lungeStrength, duration);
                }
                // else attackMain, don't move
            }
        }
        ManualFlip(playerToRight);
    }

    void LungeStart(bool lungeToRight, float strength = 4f, float duration = .3f)
    {
        movement.Lunge(lungeToRight, strength, duration);
    }

    private Vector3 GetPlayerPosX()
    {
        //Get Player position and cast explosion for Initial explosion
        float playerX = GameManager.Instance.Player.position.x;
        Vector3 castPos = new Vector3(playerX, initialGroundOffset.y, 0);
        return castPos;
    }

    private Vector3 GetOwnPosX()
    {
        Vector3 castPos = new Vector3(bossGroundOffset.position.x, bossGroundOffset.position.y, 0);
        return castPos;
    }

#endregion

    public override void TakeDamage(float damageTaken, bool knockback = false, float strength = 8)
    {
        // base.TakeDamage(damageTaken, knockback, strength);

        if (!isAlive || isSpawning) return;

        float totalDamage = damageTaken - defense;
        //Damage can never be lower than 1
        if (totalDamage <= 0) totalDamage = 1;
        
        HitFlash(); //Set material to white, short delay before resetting
        //Play hit effect, reduce hp
        InstantiateManager.Instance.HitEffects.ShowHitEffect(hitEffectsOffset.position);
        currentHP -= totalDamage;
        healthBar.UpdateHealth(currentHP);
        //Display Damage number
        InstantiateManager.Instance.TextPopups.ShowDamage(totalDamage, textPopupOffset.position);

        //Check Boss HP
        HealthPhaseCheck();

        if (currentHP <= 0)
        {
            isAlive = false;
            Die();
        }
    }
    
    protected override void StopAttack(bool toggleFlip = false)
    {
        //Stops AttackCO if not null, resets isAttacking and canMove, 
        //Stops current Attack animation
        base.StopAttack();
        //TODO: stop AttackEnd if not null
        if(AttackEndingCO != null) StopCoroutine(AttackEndingCO);
        //canAttack = false;
    }

    protected override void Die()
    {
        healthBar.gameObject.SetActive(false);
        isAlive = false;

        //Attack Coroutine checks
        if(AttackingCO != null) StopCoroutine(AttackingCO);
        StopAllCoroutines();
        canAttack = false;
        // StopAllCoroutines();

        //Boomerang/Melee Spin attack cancel
        if(BoomerangArms.activeInHierarchy) BoomerangArms.SetActive(false);

        //ChargeUp attack cancel
        canFly = false;
        movement.rb.gravityScale = originalScale;
        movement.rb.drag = originalDrag;
        

        ScreenShakeListener.Instance.Shake(2);
        Invoke("ToggleHitbox", 1f);
        // movement.rb.simulated = false;
        // GetComponent<BoxCollider2D>().enabled = false;

        playDeathAnim = true;

        //Show death effects then spawn XP Orbs
        InstantiateManager.Instance.HitEffects.ShowKillEffect(hitEffectsOffset.position);
        InstantiateManager.Instance.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);

        if(enemyStageManager != null) enemyStageManager.UpdateEnemyCount();
    }

    void ToggleHitbox()
    {
        movement.rb.simulated = false;
        GetComponent<BoxCollider2D>().enabled = false;
    }
}
