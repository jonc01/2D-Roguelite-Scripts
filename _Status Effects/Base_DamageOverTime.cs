using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_DamageOverTime : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] public float tickFrequency;
    [SerializeField] public float damagePerTick;
    [SerializeField] float duration;
    [SerializeField] protected int damageColorIdx = 0;

    [Header("Status - Debugging -")]
    public bool isActive;
    [SerializeField] float overallTimer;
    private float tickTimer;

    [Header("Target Object")]
    [SerializeField] protected IDamageable enemyCombat;
    [SerializeField] Base_PlayerCombat playerCombat;


    [Header("Animation Setup")]
    [SerializeField] private Animator anim;
    [SerializeField] private string animName;
    [SerializeField] private int hashedAnimName;
    [SerializeField] private int hashedAnimNameBlank; //-34935967

    private bool endingStatus = false;

    void Awake()
    {
        if(anim == null) anim = GetComponent<Animator>();
    }
    
    void Start()
    {
        isActive = true;
        if(hashedAnimName != 0) anim.Play(hashedAnimName);
        else anim.Play(animName);
        tickTimer = 0;
        overallTimer = 0;
        endingStatus = false;

        enemyCombat = GetComponentInParent<IDamageable>();
        playerCombat = GetComponentInParent<Base_PlayerCombat>();
    }

    void FixedUpdate()
    {
        if(isActive)
        {
            overallTimer += Time.deltaTime;
            if(overallTimer >= duration + 1) //+1 to allow for 1s delay
            {
                isActive = false;
                return;
            }

            tickTimer += Time.deltaTime;

            if(tickTimer >= tickFrequency) //1 second
            {
                DealDamage();
                tickTimer = 0;
            }
        }
        else
        {
            if(endingStatus) return;
            StartCoroutine(EndStatus());
        }
    }

    void DealDamage()
    {
        if(enemyCombat != null) enemyCombat.TakeDamageStatus(damagePerTick, damageColorIdx);
        if(playerCombat != null) playerCombat.TakeDamage(damagePerTick);
        if(enemyCombat == null && playerCombat == null) isActive = false;
    }

    IEnumerator EndStatus(float endDelay = 1)
    {
        endingStatus = true;
        anim.Play(hashedAnimNameBlank);
        yield return new WaitForSeconds(endDelay);
        Destroy(gameObject);
    }
}
