using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColossalBoss_EnemyCombat : Base_BossCombat
{
    //float attackEndDelay = .5f;
    [Header("= Colossal Boss = : Melee/Explosion")]
    [SerializeField] GameObject MeleeExplosionPrefab;
    [SerializeField] Transform bossGroundOffset;

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
        
        attackIndex = 1; //TODO: TESTING, delete when done to cycle attacks

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

    IEnumerator RangeAttack() //0
    {
        Debug.Log("Calling RangeAttack"); //TODO: TESTING
        movement.canMove = false;
        movement.DisableMove();
        movement.ToggleFlip(false);
        // yield return new WaitForSeconds(startAttackDelay);

        animator.PlayAttackAnim(fullAttackAnimTime[0]);
        FacePlayer();

        yield return new WaitForSeconds(attackDelayTime[0] - startAttackDelay);
        CheckHit();
        yield return new WaitForSeconds(fullAttackAnimTime[0] - attackDelayTime[0]);
        
        yield return new WaitForSeconds(attackEndDelay);
        movement.ToggleFlip(true);

        //TODO: move all this to another CO AttackEndCO
        StartCoroutine(AttackEnd());
    }

    IEnumerator MeleeExplosion() //1
    {
        Debug.Log("Calling MeleeExplosion"); //TODO: TESTING
        movement.canMove = false;
        movement.DisableMove();
        movement.ToggleFlip(false);
        //Start melee animation
        // yield return new WaitForSeconds(startAttackDelay); //delay before starting attack 
        
        animator.PlayManualAnim(currAttackIndex, fullAttackAnimTime[currAttackIndex]);
        yield return new WaitForSeconds(attackDelayTime[currAttackIndex] - startAttackDelay);

        CheckHit(); //Check Melee hit
        //Get Player position and cast explosion
        float playerX = GameManager.Instance.Player.position.x;
        Vector3 castPos = new Vector3(playerX, bossGroundOffset.position.y, 0);
        Instantiate(MeleeExplosionPrefab, castPos, Quaternion.identity);

        yield return new WaitForSeconds(fullAttackAnimTime[currAttackIndex] - attackDelayTime[currAttackIndex]);
        
        yield return new WaitForSeconds(attackEndDelay);
        currAttackIndex = 0; //TODO: replace once setup

        StartCoroutine(AttackEnd());
    }

    IEnumerator SuperAttack() //2
    {
        yield return null;
        currAttackIndex = 0; //TODO: replace once setup
    }

    IEnumerator MeleeSpin() //3
    {
        yield return null;
        currAttackIndex = 0; //TODO: replace once setup
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
