using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    [Header("Reference Setup")]
    [SerializeField] private Animator boomerangAnim;

    [Header("Animation Variables")]
    [SerializeField] int hashedAnimName = -830989809;
    [SerializeField] float animDuration;
    
    [Header("Final Values (Calculated)")]
    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 endPosition;

    [Space(10)]
    [Header("Adjustable Variables")]
    public LayerMask targetLayer;
    public float damage = 3f;
    public float hitFrequency = .3f; //.2 - .3
    [SerializeField] float flightDistance = 8; //11
    [SerializeField] float flightSpeed = 0.26f;
    [SerializeField] float accelerationSpeed = .2f;
    private float currFlightSpeed;
    [SerializeField] private Transform overlapOffset;
    [SerializeField] Vector3 hitboxSize;

    [Header("Knockback")]
    public bool hasKnockBack = true;
    [SerializeField] float knockbackStrength = 2f;
    [SerializeField] float knockbackDuration = .5f;

    [Header("KnockUp")]
    public bool hasKnockUp = false;
    [SerializeField] float knockupStrength = 4f;
    [SerializeField] float knockupDuration = .5f;

    private bool canFly = false;
    private bool startThrow;
    private bool endThrow;
    [SerializeField] float endPointBuffer = .1f;
    private float timeSinceHit;
    private bool initialKnockbackToRight;

    void Awake()
    {
        if(boomerangAnim == null) boomerangAnim = GetComponent<Animator>();
        canFly = false;
        startThrow = false;
        endThrow = false;
    }

    void Update()
    {
        //Only check for hits at the hitFrequency, allows multiple hits, but not too quickly
        timeSinceHit += Time.deltaTime;
        if(timeSinceHit < hitFrequency) return;
        CheckHit();
    }

    void FixedUpdate()
    {
        if(!canFly) return;

        if(startThrow) //Boomerang throw
        {
            //Once boomerang slows down enough, switch to returning checks
            if(currFlightSpeed < 0)
            {
                startThrow = false;
                return;
            }
            currFlightSpeed -= accelerationSpeed * Time.deltaTime;
            transform.position = 
                Vector3.MoveTowards(transform.position, endPosition, currFlightSpeed);
        }

        if(endThrow) //Boomerang returning
        {
            //Stop once the boomerang reaches the starting position
            if(Vector3.Distance(transform.position, startPosition) < endPointBuffer)
            {
                currFlightSpeed = 0;
                endThrow = false;
                return;
            }
            currFlightSpeed += accelerationSpeed * Time.deltaTime;
            transform.position = 
                Vector3.MoveTowards(transform.position, startPosition, currFlightSpeed);
        }
    }

    void OnEnable()
    {
        StartCoroutine(SpinCO());
    }

    IEnumerator SpinCO()
    {
        currFlightSpeed = flightSpeed;
        GetFlightEndPos();
        boomerangAnim.Play(hashedAnimName);
        startThrow = true;
        endThrow = false;

        yield return new WaitForSeconds(.1f);
        
        canFly = true;
        
        //Start Lerp calls in Update
        while(startThrow) yield return null; 
        yield return new WaitForSeconds(.1f);
        endThrow = true;
        yield return new WaitForSeconds(.1f);
        while(endThrow) yield return null;

        //Boomerang has returned, disable object, Combat script takes over
        canFly = false;
        yield return new WaitForSeconds(animDuration+.1f);
        gameObject.SetActive(false);
    }

    private void GetFlightEndPos()
    {
        Transform parentObj = transform.parent;
        //Reset position to current Boss position
        Vector3 parentPos = parentObj.position;
        transform.position = parentPos;
        //Get start and end positions based on main Boss transform
        startPosition = new Vector3(parentPos.x, parentPos.y, 0);
        endPosition = startPosition;

        //Get the center coordinate of the Enemy room
        Vector3 parentObjPos = parentObj.parent.transform.position;
        //Check for left or right positioning endpoint based on Boss position
        if(parentPos.x < parentObjPos.x)
        {
            endPosition.x += flightDistance;
            initialKnockbackToRight = false;
        }
        else
        {
            endPosition.x -= flightDistance;
            initialKnockbackToRight = true;
        }
    }

    void CheckHit()
    {
        Collider2D collider = Physics2D.OverlapBox(overlapOffset.position, hitboxSize, 0f, targetLayer);
        if(collider == null) return;
        var player = collider.GetComponent<Base_PlayerCombat>();
        //Check when there is a new collider coming into contact with the box
        if (player != null)
        {
            timeSinceHit = 0;
            player.TakeDamage(damage);
            if(hasKnockBack)
            {
                bool knockbackToRight;
                
                //Player gets knockedback in the direction the Boomerang is moving
                if(startThrow) knockbackToRight = initialKnockbackToRight;
                else knockbackToRight = !initialKnockbackToRight;

                player.GetKnockback(knockbackToRight, knockbackStrength, knockbackDuration);
            }

            if(hasKnockUp)
            {
                player.GetKnockup(knockupStrength, knockupDuration);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(overlapOffset.position, hitboxSize);
    }
}
