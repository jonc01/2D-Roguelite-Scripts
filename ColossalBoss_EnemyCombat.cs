using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColossalBoss_EnemyCombat : Base_BossCombat
{
    //float attackEndDelay = .5f;
    [Header("= Colossal Boss = : (1) RangeAttack")]
    [SerializeField] GameObject RangeAttackExplosionPrefab;
    [SerializeField] Transform bossGroundOffset;

    [Header("= Colossal Boss = : (2) Melee/Explosion")]
    [SerializeField] GameObject MeleeExplosionPrefab;
    [SerializeField] GameObject MeleePrefab;
    [Header("= Colossal Boss = : (2) Melee/Explosion")]
    [SerializeField] GameObject SuperAttackExplosionPrefab;
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
    
    IEnumerator AttackCO()
    {
        timeSinceAttack = 0;
        isAttacking = true;
        // ThresholdCheck(); //TODO: bring back when done testing
        
        //TODO: TESTING, delete when done to cycle attacks
        // int randAttack = Random.Range(0, 5);
        //int randAttack = 1;
        numAttacks = 1; //
        currAttackIndex = 2;
        // currAttackIndex = randAttack;
        //////////////////////////////////////////////////

        canAttack = false;
        switch(currAttackIndex)
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
        canAttack = true;
    }

#region Attack Inits
    void ThresholdCheck() //Determine number of combo attacks and frequency
    {
        float hpThrehold = currentHP/maxHP;
        //Phase determined by HP thresholds
        if(hpThrehold > .5f) 
        {
            currentPhase = 1;
            numAttacks = 2;
            attackEndDelay = .5f;
        }
        if(hpThrehold <= .5f)
        {
            currentPhase = 2;
            numAttacks = 4; //TODO: 3?
            attackEndDelay = 0.1f; //No delay, attackSpeed delay still applies
        }
    }

    IEnumerator RangeAttackInit()
    {
        attackEndDelay = 0; //TODO: TESTING might be fine
        for(int i=0; i<numAttacks; i++)
        {
            Debug.Log("looping... " + i);
            yield return StartCoroutine(RangeAttack());
            movement.ToggleFlip(true);
        }
        // yield return StartCoroutine(AttackEnd());
        // canAttack = true;
        //yield return new WaitForSeconds(.1f);
    }

    IEnumerator MeleeAttackInit()
    {
        attackEndDelay = 0; //TODO: TESTING might be fine
        for(int i=0; i<numAttacks; i++)
        {
            yield return StartCoroutine(MeleeExplosion());
            movement.ToggleFlip(true);
        }
        // yield return StartCoroutine(AttackEnd());
        // canAttack = true;
    }

    //

    IEnumerator ChargeAttackInit() //TODO: move this to Attack() CO
    {
        attackEndDelay = 0; //TODO: TESTING might be fine
        yield return StartCoroutine(ChargeUp());
        // yield return StartCoroutine(AttackEnd());
        // canAttack = true;
    }
#endregion

#region Attack Coroutines

    IEnumerator RangeAttack() //0
    {
        movement.canMove = false;
        movement.DisableMove();
        movement.ToggleFlip(false);

        yield return new WaitForSeconds(startAttackDelay);
        animator.PlayAttackAnim(fullAttackAnimTime[0]);
        // FacePlayer();

        yield return new WaitForSeconds(attackDelayTime[0]);// - startAttackDelay);
        Instantiate(RangeAttackExplosionPrefab, transform.position, transform.rotation);

        yield return new WaitForSeconds(fullAttackAnimTime[0] - attackDelayTime[0]);
        yield return new WaitForSeconds(attackEndDelay);
        // movement.ToggleFlip(true);
    }

    IEnumerator MeleeExplosion() //1
    {
        movement.canMove = false;
        movement.DisableMove();
        movement.ToggleFlip(false);
        //Start melee animation
        animator.PlayManualAnim(currAttackIndex, fullAttackAnimTime[1]);
        yield return new WaitForSeconds(startAttackDelay); //delay before starting attack
        // FacePlayer();

        yield return new WaitForSeconds(attackDelayTime[1]);// - startAttackDelay);
        if (MeleePrefab != null) MeleePrefab.SetActive(true);
        CheckHit(true, true); //Check Melee hit

        StartCoroutine(MeleeExplosionOnly(2));

        yield return new WaitForSeconds(fullAttackAnimTime[1] - attackDelayTime[1]);
        yield return new WaitForSeconds(attackEndDelay);
    }

    IEnumerator MeleeExplosionOnly(int iterations, bool trackPlayer = false)
    {
        if(iterations <= 0) yield return null;
        Vector3 castPos;

        //Spawn multiple explosions in a wave
        for(int i=0; i<iterations; i++)
        {
            if(!trackPlayer)
            {
                castPos = GetOwnPosX();
                if(movement.isFacingRight) castPos.x += i;
                else castPos.x -= i;

                Instantiate(MeleeExplosionPrefab, castPos, Quaternion.identity);
            }
            else
            {
                Instantiate(MeleeExplosionPrefab, GetPlayerPosX(), Quaternion.identity);
            }
            
            yield return new WaitForSeconds(.3f);
        }
    }

    IEnumerator SuperAttack() //2
    {
        yield return null;
        // Instantiate(SuperAttackExplosionPrefab, transform.position, Quaternion.identity);
    }

    IEnumerator MeleeSpin() //3
    {
        yield return null;
    }

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
            animator.PlayManualAnim(6, fullAttackAnimTime[4]);
            yield return new WaitForSeconds(attackDelayTime[4]);
            StartCoroutine(MeleeExplosionOnly(1, true));
            yield return new WaitForSeconds((fullAttackAnimTime[4] - attackDelayTime[4])+.1f);
        }

        yield return new WaitForSeconds(.5f);
        movement.rb.gravityScale = originalScale;
        movement.rb.drag = originalDrag;

        yield return new WaitForSeconds(attackEndDelay);
    }

    //Ending Coroutine
    IEnumerator AttackEnd()
    {
        //Use next attack, reset counter if out of bounds
        currAttackIndex++;
        if(currAttackIndex >= attackPoint.Length) currAttackIndex = 0;

        yield return new WaitForSeconds(attackSpeed);
        movement.ToggleFlip(true);
        isAttacking = false;
        movement.canMove = true;
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
