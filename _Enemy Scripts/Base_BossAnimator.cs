using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_BossAnimator : MonoBehaviour
{
    [Header("References / Setup")]
    public Animator anim;
    public Base_BossMovement movement;
    public Base_BossCombat combat;

    //State Checks

    private float lockedTill;
    [SerializeField] private bool attacking;
    Coroutine AttackCO;

    //Animation Names
    [Header("Additional Animations")]
    [SerializeField] string[] AdditionalAnims;
    [Header("! - If at 0, will be filled, Otherwise is automatically used")]
    [SerializeField] int[] AdditionalAnimHashes;
    //[SerializeField] float sampleRate = 12;
    //TODO: custom variables, or use this naming scheme

    #region Cached
    private int currentState;

    //Array used during setup, all hashed here

    protected static readonly int Idle = Animator.StringToHash("Idle");
    protected static readonly int Move = Animator.StringToHash("Move");
    protected static readonly int Attack = Animator.StringToHash("RangeAttack");
    protected static readonly int Death = Animator.StringToHash("Death");
    
    /*private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Death = Animator.StringToHash("Death");*/
    #endregion

    private void Awake()
    {
        //Get references
        if (anim == null) anim = GetComponent<Animator>();

        if (movement == null) movement = GetComponentInParent<Base_BossMovement>();
        if (combat == null) combat = GetComponentInParent<Base_BossCombat>();

        attacking = false;

        //Generate hashes if not already setup
        if (AdditionalAnimHashes.Length == 0) HashAdditionalAnims();
    }

    private void Update()
    {
        if (!combat.isAlive)
        {
            if(AttackCO != null) StopCoroutine(AttackCO);
            attacking = false;
        }

        //Stun override
        if (combat.isStunned)
        {
            //No stun animation :(
            attacking = false;
            return;
        }

        //Attack override
        if (attacking) return;

        //State Checks
        var state = GetState();

        if (state == currentState) return;
        anim.CrossFade(state, 0, 0);
        currentState = state;
    }

    public int LockState(int s, float t)
    {
        lockedTill = Time.time + t;
        return s;
    }

    public void PlayAttackAnim(float animTime)
    {
        StopAttackAnimCO();
        attacking = true;

        anim.CrossFade(Attack, 0, 0); //TODO: is crossfade needed?

        AttackCO = StartCoroutine(AttackState(animTime));
    }

    public void PlayManualAnim(int index, float animTime)
    {
        StopAttackAnimCO();
        attacking = true;
    
        anim.CrossFade(AdditionalAnimHashes[index], 0, 0);

        AttackCO = StartCoroutine(AttackState(animTime));
    }

    IEnumerator AttackState(float duration) //Timer
    {
        yield return new WaitForSeconds(duration);
        attacking = false;
        if (movement.canMove) anim.CrossFade(Move, 0, 0);
        else anim.CrossFade(Idle, 0, 0);
        //else anim.CrossFade(Move, 0, 0);
        //Makes no sense, but otherwise GetState GetsStuck
    }

    public void StopAttackAnimCO()
    {
        if (AttackCO != null)
        {
            StopCoroutine(AttackCO); //Stops current attackCO if attack speed overrides
            attacking = false;
        }
    }

    private int GetState()
    {
        if (Time.time < lockedTill) return currentState;

        if (!combat.isAlive) return Death;
        //if (movement.isJumping) return Jump;

        //if (movement.isGrounded)
        //return movement.canMove ? Move : Idle;
        return movement.rb.velocity.x != 0 ? Move : Idle;
        //return movement.rb.velocity.y > 0 ? Jump : Fall;
    }


    //
    private void HashAdditionalAnims()
    {
        int total = AdditionalAnims.Length;
        if (total <= 0) return;

        AdditionalAnimHashes = new int[total];
        
        for(int i = 0; i < total; i++)
        {
            AdditionalAnimHashes[i] = Animator.StringToHash(AdditionalAnims[i]);
        }
    }


}
