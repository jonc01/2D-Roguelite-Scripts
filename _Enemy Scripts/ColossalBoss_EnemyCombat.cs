using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColossalBoss_EnemyCombat : Base_BossCombat
// public class ColossalBoss_BossCombat : Base_BossCombat
{
    //float attackEndDelay = .5f;
    [Space(10)]
    //2D array with attack pools for each phase
    // [Header("= Colossal Boss =")]
    // [SerializeField] private int[] Phase1AtkPool;
    // [SerializeField] private int[] Phase2AtkPool;
    // [SerializeField] private int[] Phase3AtkPool;

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

    protected override IEnumerator SpawnCO(float delay)
    {
        isSpawning = true; //This prevents the enemy from attacking and taking damage
        
        yield return new WaitForSeconds(delay);
        ExplosionInit(4);
        
        animator.PlayManualAnim(4, 1f);
        yield return new WaitForSeconds(1f); //Wake animation
        healthBar.gameObject.SetActive(true);
        //Toggle SR on
        yield return new WaitForSeconds(0.5f);
        isSpawning = false;
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
        timeSinceAttack = 0;
        canAttack = false;

        currAttack++;
        // currAttackIndex++;
        // currAttackIndex = currAttack - 1;

        // int randIndex; //TODO: replacing with manual count ^

        //Get random attack index
        if(currentPhase == 0){
            // randIndex = Random.Range(0, Phase1AtkPool.Length);
            // currAttackIndex = Phase1AtkPool[randIndex];
            if(currAttack-1 >= Phase1AtkPool.Length)
            {
                //TODO: shuffle atkPool
                ShuffleAttackPools(Phase1AtkPool);
                currAttack = 1;
            }
            currAttackIndex = Phase1AtkPool[currAttack-1];
        }
        else if(currentPhase == 1){
            if(currAttack-1 >= Phase2AtkPool.Length)
            {
                ShuffleAttackPools(Phase2AtkPool);
                currAttack = 1;
            }
            currAttackIndex = Phase2AtkPool[currAttack-1];
        }
        else{
            if(currAttack-1 >= Phase3AtkPool.Length)
            {
                ShuffleAttackPools(Phase3AtkPool);
                currAttack = 1;
            }
            currAttackIndex = Phase3AtkPool[currAttack-1];
        }

        // currAttackIndex = currAttack - 1;

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
        yield return AttackEndCO = StartCoroutine(AttackEnd());
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
        yield return AttackEndCO = StartCoroutine(AttackEnd());
    }

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
    IEnumerator MeleeExplosion(int numExplosions = 4) //1
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

    void ExplosionInit(int iterations = 4, float yOffset = 5)
    {
        //This doesn't quite work, but does fix the pooled explosions issue all being called properly
        Vector3 castPos = new Vector3(transform.position.x, transform.position.y - yOffset, 0);
        for(int i=0; i<iterations; i++)
        {
            ExplosionHandler.SpawnPrefab(castPos);
        }
        Invoke("ShakeEvent", .33f);
    }

    void ShakeEvent()
    {
        ScreenShakeListener.Instance.Shake(3);
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
    IEnumerator ChargeUp(int iterations = 2)
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
        // if(currAttackIndex >= attackPoint.Length) currAttackIndex = 0; //TODO:

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
        float playerX = GameManager.Instance.playerTransform.position.x;
        Vector3 castPos = new Vector3(playerX, initialGroundOffset.y, 0);
        return castPos;
    }

    private Vector3 GetOwnPosX()
    {
        Vector3 castPos = new Vector3(bossGroundOffset.position.x, bossGroundOffset.position.y, 0);
        return castPos;
    }

#endregion
    protected override void HealthPhaseCheck()
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

    protected override IEnumerator ChangePhase()
    {
        // return base.ChangePhase();
        // protected virtual IEnumerator ChangePhase()
    
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

    protected override void StopAttack(bool toggleFlip = false)
    {
        if (AttackingCO != null) StopCoroutine(AttackingCO);
        if (AttackEndCO != null) StopCoroutine(AttackEndCO);
        isAttacking = false;

        canFly = false;
        movement.rb.gravityScale = originalScale;
        movement.rb.drag = originalDrag;

        movement.canMove = true;
        movement.ToggleFlip(toggleFlip);
        //Cancels Attack animation
        animator.StopAttackAnimCO();
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
        

        Invoke("ToggleHitbox", 1f); //Delay rb and collider toggle

        playDeathAnim = true;

        //Show death effects then spawn XP Orbs
        ScreenShakeListener.Instance.Shake(3);
        InstantiateManager.Instance.HitEffects.ShowKillEffect(hitEffectsOffset.position);
        // InstantiateManager.Instance.XPOrbs.SpawnOrbs(transform.position, totalXPOrbs);

        if(enemyStageManager != null) enemyStageManager.UpdateEnemyCount();
        GameManager.Instance.AugmentInventory.OnKill(transform);
    }

    void ToggleHitbox()
    {
        movement.rb.simulated = false;
        GetComponent<BoxCollider2D>().enabled = false;

        Vector3 offset = hitEffectsOffset.position;
        offset.y -= .35f;

        ScreenShakeListener.Instance.Shake(3);
        InstantiateManager.Instance.HitEffects.ShowKillEffect(offset);
        InstantiateManager.Instance.XPOrbs.SpawnOrbs(offset, totalXPOrbs);
        InstantiateManager.Instance.HealOrbs.SpawnOrbs(offset, totalHealOrbs);
    }
}
