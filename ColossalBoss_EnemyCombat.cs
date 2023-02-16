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
    [Header("= Colossal Boss = : (2) Melee/Explosion")]
    [SerializeField] GameObject SuperAttackExplosionPrefab;


    protected override void Awake()
    {
        base.Awake();

    }

    protected override void Start()
    {
        base.Start();

    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void FacePlayer()
    {
        // if(ColossalBoss_Controller.playerDetect)
    }

    public override void Attack(int attackIndex)
    {
        if (!isAlive || isAttacking || isSpawning) return;
        if (timeSinceAttack <= attackSpeed) return;

        timeSinceAttack = 0;
        isAttacking = true;
        
        //TODO: TESTING, delete when done to cycle attacks
        attackIndex = 1; 
        currAttackIndex = 1;
        //////////////////////////////////////////////////

        switch(attackIndex)
        {
            case 0:
                AttackingCO = StartCoroutine(RangeAttack());
                break;
            case 1:
                AttackingCO = StartCoroutine(MeleeExplosion());
                break;
            case 2:
                AttackingCO = StartCoroutine(SuperAttack());
                break;
            case 3:
                AttackingCO = StartCoroutine(MeleeSpin());
                break;
            default:
                // AttackingCO = StartCoroutine(MeleeExplosion());
                break;
        }
    }

#region Attack Coroutines

    int InstantiateFlip()
    {
        if(movement.isFacingRight) return 1;
        else return -1;
    }

    IEnumerator RangeAttack() //0
    {
        movement.canMove = false;
        movement.DisableMove();
        movement.ToggleFlip(false);
        // yield return new WaitForSeconds(startAttackDelay);

        animator.PlayAttackAnim(fullAttackAnimTime[0]);
        FacePlayer();

        yield return new WaitForSeconds(attackDelayTime[0] - startAttackDelay);
        Instantiate(RangeAttackExplosionPrefab, transform.position, transform.rotation);

        yield return new WaitForSeconds(fullAttackAnimTime[0] - attackDelayTime[0]);
        yield return new WaitForSeconds(attackEndDelay);
        movement.ToggleFlip(true);

        StartCoroutine(AttackEnd());
    }

    IEnumerator MeleeExplosion() //1
    {
        movement.canMove = false;
        movement.DisableMove();
        movement.ToggleFlip(false);
        //Start melee animation
        // yield return new WaitForSeconds(startAttackDelay); //delay before starting attack 
        
        animator.PlayManualAnim(currAttackIndex, fullAttackAnimTime[1]);
        yield return new WaitForSeconds(attackDelayTime[1] - startAttackDelay);

        CheckHit(); //Check Melee hit
        //Get Player position and cast explosion
        float playerX = GameManager.Instance.Player.position.x;
        Vector3 castPos = new Vector3(playerX, bossGroundOffset.position.y, 0);
        Instantiate(MeleeExplosionPrefab, castPos, Quaternion.identity);

        yield return new WaitForSeconds(fullAttackAnimTime[1] - attackDelayTime[1]);
        
        yield return new WaitForSeconds(attackEndDelay);

        StartCoroutine(AttackEnd());
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
