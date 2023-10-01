using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_DamageOverTime : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] public float tickFrequency;
    [SerializeField] public float damagePerTick;
    [SerializeField] public float duration;
    [SerializeField] protected int damageColorIdx = 0;

    [Header("Status - Debugging -")]
    public bool isActive;
    [SerializeField] public float overallTimer;
    protected float tickTimer;

    [Header("Target Object")]
    [SerializeField] protected IDamageable enemyCombat;
    [SerializeField] protected Base_PlayerCombat playerCombat;


    [Header("Animation Setup")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected string animName;
    [SerializeField] protected int hashedAnimName;
    [SerializeField] protected int hashedAnimNameBlank; //-34935967

    protected bool endingStatus = false;

    protected void Awake()
    {
        if(anim == null) anim = GetComponent<Animator>();
    }
    
    protected virtual void Start()
    {
        isActive = true;
        if(hashedAnimName != 0) anim.Play(hashedAnimName);
        else anim.Play(animName);
        tickTimer = 0;
        overallTimer = duration;
        endingStatus = false;

        enemyCombat = GetComponentInParent<IDamageable>();
        playerCombat = GetComponentInParent<Base_PlayerCombat>();

    }

    protected virtual void CheckForExistingDoT()
    {
        //Not called by default, override with inherit
        Base_DamageOverTime existingDoT = transform.parent.GetComponentInChildren<Base_DamageOverTime>();
        if(existingDoT != null)
        {
            overallTimer += duration;
            damagePerTick += 1;
        }
    }

    public virtual void AddToBaseDuration(float durationAdded)
    {
        duration += durationAdded;
    }

    public virtual void ExtendTimer(float timeAdded)
    {
        overallTimer += timeAdded;
        //Timer cannot exceed full duration by default
        if(overallTimer > duration) overallTimer = duration;
    }

    public virtual void ResetTimer()
    {
        overallTimer = duration;
    }

    protected virtual void FixedUpdate()
    {
        if(isActive)
        {
            overallTimer -= Time.deltaTime;
            if(overallTimer <= 0) //+1 to allow for 1s delay
            {
                isActive = false;
                return;
            }

            tickTimer -= Time.deltaTime; //Countdown timer

            if(tickTimer <= 0) //
            {
                DealDamage();
                tickTimer = tickFrequency; //Reset tick timer
            }
        }
        else
        {
            if(endingStatus) return;
            StartCoroutine(EndStatus());
        }
    }

    protected void DealDamage()
    {
        if(enemyCombat != null) enemyCombat.TakeDamageStatus(damagePerTick, damageColorIdx);
        if(playerCombat != null) playerCombat.TakeDamage(damagePerTick);
        if(enemyCombat == null && playerCombat == null) isActive = false;
    }

    protected IEnumerator EndStatus(float endDelay = 1)
    {
        endingStatus = true;
        anim.Play(hashedAnimNameBlank);
        yield return new WaitForSeconds(endDelay);
        Destroy(gameObject);
    }
}
