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
    [SerializeField] GameObject MeleeExplosionPrefab;
    [SerializeField] GameObject MeleePrefab;
    [SerializeField] float explosionCastDelay = .2f;

    [Header("= Colossal Boss = : (2) SuperAttack")]
    [SerializeField] GameObject SuperAttackExplosionPrefab;

    [Header("= Colossal Boss = : (3) Spin")]
    // [SerializeField] float rightWallX;
    [SerializeField] GameObject BoomerangArms;

    [Header("= Colossal Boss = : (4) ChargeUp")]
    [SerializeField] private bool canFly;
    [SerializeField] Vector3 initialGroundOffset;
    [SerializeField] float flyingOffsetY = 1f;
    [SerializeField] Vector3 flyingOffsetPos;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        canFly = false;
        initialGroundOffset = bossGroundOffset.position;
        flyingOffsetPos = transform.position;
        flyingOffsetPos.y += flyingOffsetY;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
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
        timeSinceAttack = 0;
        canAttack = false;
        ThresholdCheck(); //TODO: bring back when done testing
        
        //TODO: TESTING, delete when done to cycle attacks
        // int randAttack = Random.Range(0, 5);
        //int randAttack = 1;
        numAttacks = 1; //
        // currAttackIndex = 2;
        // currAttackIndex = randAttack;
        /////////////////////////////////////////////
        int randIndex;
        // for(int i=0; i<numAttacks; i++)
        // {
        //Get random attack index
        if(currentPhase == 1){
            randIndex = Random.Range(0, Phase1AtkPool.Length);
            currAttackIndex = Phase1AtkPool[randIndex];
        }else if(currentPhase == 2){
            randIndex = Random.Range(0, Phase2AtkPool.Length);
            currAttackIndex = Phase2AtkPool[randIndex];
        }else{
            randIndex = Random.Range(0, Phase3AtkPool.Length);
            currAttackIndex = Phase3AtkPool[randIndex];
        }

        switch(currAttackIndex)
        {
            case 0:
                yield return AttackingCO = StartCoroutine(RangeAttackInit());
                break;
            case 1:
                yield return AttackingCO = StartCoroutine(MeleeAttackInit());
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
                yield return AttackingCO = StartCoroutine(ChargeAttackInit());
                break;
            default:
                // AttackingCO = StartCoroutine(MeleeExplosion());
                break;
        }
        // }
        yield return StartCoroutine(AttackEnd());
    }

    IEnumerator ManualAttackCO(int attackIndex)
    {
        timeSinceAttack = 0;
        canAttack = false;
        switch(attackIndex)
        {
            case 0:
                yield return AttackingCO = StartCoroutine(RangeAttackInit());
                break;
            case 1:
                yield return AttackingCO = StartCoroutine(MeleeAttackInit());
                break;
            case 2:
                yield return AttackingCO = StartCoroutine(SuperAttack()); //<50% hp
                break;
            case 3:
                yield return AttackingCO = StartCoroutine(MeleeSpin()); //<50% hp, increased freq
                break;
            case 4:
                yield return AttackingCO = StartCoroutine(ChargeAttackInit());
                break;
            default:
                // AttackingCO = StartCoroutine(MeleeExplosion());
                break;
        }
        yield return StartCoroutine(AttackEnd());
    }

#region Attack Inits
    void ThresholdCheck() //Determine number of combo attacks and frequency
    {
        float hpThrehold = currentHP/maxHP;
        //Phase determined by HP thresholds
        if(hpThrehold > .66f)
        {
            currentPhase = 1;
            // numAttacks = 3;
            attackEndDelay = .3f;
        }
        if(hpThrehold <= .66f)
        {
            currentPhase = 2;
            // numAttacks = 4; //TODO: 3?
            attackEndDelay = 0.1f; //No delay, attackSpeed delay still applies
        }
        if(hpThrehold <= .33f)
        {
            currentPhase = 3;
            // numAttacks = 4;
            attackEndDelay = 0.1f;
        }
    }

    IEnumerator RangeAttackInit()
    {
        //TODO: this function might not be needed at all
        attackEndDelay = 0; //TODO: TESTING might be fine
        
        yield return StartCoroutine(RangeAttack());
        // yield return StartCoroutine(AttackEnd());
        //yield return new WaitForSeconds(.1f);
    }

    IEnumerator MeleeAttackInit()
    {
        //TODO: this function might not be needed at all
        attackEndDelay = 0; //TODO: TESTING might be fine
        yield return StartCoroutine(MeleeExplosion(4));
    }

    //

    IEnumerator ChargeAttackInit() //TODO: move this to Attack() CO
    {
        //TODO: this function might not be needed at all
        attackEndDelay = 0; //TODO: TESTING might be fine
        yield return StartCoroutine(ChargeUp());
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
        movement.ToggleFlip(true); //TODO: may not be needed, since ManualFlip overrides
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
    IEnumerator MeleeExplosionOnly(int iterations, bool trackPlayer = false)
    {
        if(iterations <= 0) yield return null;
        Vector3 castPos;
        //Spawn multiple explosions in a wave
        for(int i=0; i<iterations; i++)
        {
            if(!trackPlayer)
            {
                if(!isAlive) break;
                castPos = GetOwnPosX();
                if(movement.isFacingRight) castPos.x += i;
                else castPos.x -= i;
                Instantiate(MeleeExplosionPrefab, castPos, Quaternion.identity);
            }
            else
            {
                Instantiate(MeleeExplosionPrefab, GetPlayerPosX(), Quaternion.identity);
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
        yield return new WaitForSeconds(.3f);

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
        //Move Boss to a random side of the room
            //Lerp? or MoveTowards

        bool moveToRightWall; //= (Random.value > 0.5f); //randomize

        //Move to the furthest wall, checking x position
        if(transform.position.x < 0) moveToRightWall = true; //TODO: this doesn't work :)
        else moveToRightWall = false;

        //-------------------------

        chasePlayer = false;
        movement.canMove = true;
        yield return MoveToWall(moveToRightWall);
        
        // ------------

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
        float originalScale = movement.rb.gravityScale;
        float originalDrag = movement.rb.drag;

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
            StartCoroutine(MeleeExplosionOnly(1, true));
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

    protected override void Die()
    {
        healthBar.gameObject.SetActive(false);
        if(AttackingCO != null) StopCoroutine(AttackingCO);
        if(BoomerangArms.activeInHierarchy) BoomerangArms.SetActive(false);

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
}
