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

    [Header("= Colossal Boss = : (1) RangeAttack")]
    [SerializeField] GameObject RangeAttackExplosionPrefab;
    [SerializeField] Transform bossGroundOffset;

    [Header("= Colossal Boss = : (2) Melee/Explosion")]
    [SerializeField] GameObject MeleeExplosionPrefab;
    [SerializeField] GameObject MeleePrefab;
    [Header("= Colossal Boss = : (2) Melee/Explosion")]
    [SerializeField] GameObject SuperAttackExplosionPrefab;
    [SerializeField] float explosionCastDelay = .2f;

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
        if (!isAlive || isAttacking || isSpawning || !canAttack) return;
        if (timeSinceAttack <= attackSpeed) return;

        StartCoroutine(AttackCO());
    }

    public override void Attack(int attackIndex)
    {
        if (!isAlive || isAttacking || isSpawning || !canAttack) return;
        if (timeSinceAttack <= attackSpeed) return;
    
        StartCoroutine(ManualAttackCO(attackIndex));
    }
    
    IEnumerator AttackCO()
    {
        timeSinceAttack = 0;
        isAttacking = true;
        // ThresholdCheck(); //TODO: bring back when done testing
        
        //TODO: TESTING, delete when done to cycle attacks
        // int randAttack = Random.Range(0, 5);
        //int randAttack = 1;
        numAttacks = 1; //
        // currAttackIndex = 2;
        // currAttackIndex = randAttack;
        /////////////////////////////////////////////

        canAttack = false;
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
        Debug.Log("Curr AttackCO ended");
        yield return StartCoroutine(AttackEnd());
        canAttack = true;
    }

    IEnumerator ManualAttackCO(int attackIndex)
    {
        timeSinceAttack = 0;
        isAttacking = true;
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
        yield return StartCoroutine(AttackEnd());
        canAttack = true;
    }

#region Attack Inits
    void ThresholdCheck() //Determine number of combo attacks and frequency
    {
        float hpThrehold = currentHP/maxHP;
        //Phase determined by HP thresholds
        if(hpThrehold > .66f)
        {
            currentPhase = 1;
            numAttacks = 3;
            attackEndDelay = .3f;
        }
        if(hpThrehold <= .66f)
        {
            currentPhase = 2;
            numAttacks = 4; //TODO: 3?
            attackEndDelay = 0.1f; //No delay, attackSpeed delay still applies
        }
        if(hpThrehold <= .33f)
        {
            currentPhase = 3;
            numAttacks = 4;
            attackEndDelay = 0.1f;
        }
    }

    IEnumerator RangeAttackInit()
    {
        attackEndDelay = 0; //TODO: TESTING might be fine
        
        yield return StartCoroutine(RangeAttack());
        // movement.ToggleFlip(true); //TODO: remove, already called in AttackEnd()
        
        // yield return StartCoroutine(AttackEnd());
        // canAttack = true;
        //yield return new WaitForSeconds(.1f);
    }

    IEnumerator MeleeAttackInit()
    {
        attackEndDelay = 0; //TODO: TESTING might be fine
        yield return StartCoroutine(MeleeExplosion(4));
        // movement.ToggleFlip(true); //TODO: remove, already called in AttackEnd()
    }

    //

    IEnumerator ChargeAttackInit() //TODO: move this to Attack() CO
    {
        attackEndDelay = 0; //TODO: TESTING might be fine
        yield return StartCoroutine(ChargeUp());
        // movement.ToggleFlip(true); //TODO: remove, already called in AttackEnd()
    }

#endregion

#region Attack Coroutines
//Attack[0]: Shoots at the ground in front
    IEnumerator RangeAttack() //0
    {
        movement.canMove = false;
        movement.DisableMove();

        yield return new WaitForSeconds(startAttackDelay);
        animator.PlayManualAnim(0, fullAttackAnimTime[0]);
        // FacePlayer();
        movement.ToggleFlip(false);
        
        //Lunge backwards from Player if too close, otherwise lunge forward
        LungeCheck(3f);
        //else do nothing

        yield return new WaitForSeconds(attackDelayTime[0]);// - startAttackDelay);
        Instantiate(RangeAttackExplosionPrefab, transform.position, transform.rotation);

        yield return new WaitForSeconds(fullAttackAnimTime[0] - attackDelayTime[0]);
        yield return new WaitForSeconds(attackEndDelay);
    }

//Attack[1]: Punch ground and spawn a wave of explosions forward
    IEnumerator MeleeExplosion(int numExplosions = 3) //1
    {
        movement.canMove = false;
        movement.DisableMove();
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
    }

//Prefab: Pass in number of explosions to spawn, changing 
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
        yield return null;
        // Instantiate(SuperAttackExplosionPrefab, transform.position, Quaternion.identity);
    }

//Attack[3]: Boss moves towards a wall then launches its arms as a boomerang towards 
//  the opposite wall. The arms can hit multiple times, but have a cooldown per hit
    IEnumerator MeleeSpin() //3
    {
        yield return null;
    }

//Attack[4]: 
    IEnumerator ChargeUp(int iterations = 3)
    {
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
            movement.ToggleFlip(false);
            animator.PlayManualAnim(6, fullAttackAnimTime[4]);
            yield return new WaitForSeconds(attackDelayTime[4]);
            movement.ToggleFlip(true);
            StartCoroutine(MeleeExplosionOnly(1, true));
            yield return new WaitForSeconds((fullAttackAnimTime[4] - attackDelayTime[4])+.1f);
        }

        yield return new WaitForSeconds(.5f);
        movement.rb.gravityScale = originalScale;
        movement.rb.drag = originalDrag;

        yield return new WaitForSeconds(attackEndDelay);
    }

//Attack: if Player is too close
    IEnumerator ManualFlip()
    {
        //Lunge away from the Player, then flip towards the Player to attack
        movement.ToggleFlip(false);
        yield return new WaitForSeconds(.1f);
        movement.ManualFlip(!movement.isFacingRight);
    }

//Ending Coroutine
    IEnumerator AttackEnd()
    {
        //Use next attack, reset counter if out of bounds
        if(currAttackIndex >= attackPoint.Length) currAttackIndex = 0;

        yield return new WaitForSeconds(attackSpeed);
        movement.ToggleFlip(true);
        isAttacking = false;
        movement.canMove = true;
    }

    public override void LungeCheck(float lungeStrength = 4f, float duration = .3f)
    {
        if(backToWall)
        {
            LungeStart(movement.isFacingRight, lungeStrength, duration);
            StartCoroutine(ManualFlip());
        }
        else
        {
            //Player is too close, lunge backwards
            if(attackClose)
                LungeStart(!playerToRight, lungeStrength, duration);
            //Player is out of normal attack range, lunge forward
            else if(!attackMain && !attackClose)
                LungeStart(playerToRight, lungeStrength*1.5f, duration);
            // else attackMain, don't move
        }
    }

    void LungeStart(bool playerToRight, float strength = 4f, float duration = .3f)
    {
        movement.Lunge(playerToRight, strength, duration);
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
