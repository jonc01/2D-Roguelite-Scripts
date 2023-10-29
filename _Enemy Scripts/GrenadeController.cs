using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public class GrenadeController : MonoBehaviour
{
    [Header("Variables")]
    public float damage = 5;
    public float knockbackStrength = 1;
    public float launchForce = 3;
    public bool enemyProj = true;
    public float grenadeTimer = 3;

    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] string movingAnimName;
    [SerializeField] string explodeAnimName;
    [SerializeField] float explodeAnimTime = .1f;
    [SerializeField] GameObject explosionPrefab;
    [Space(10)]
    [SerializeField] GameObject indicatorObj;
    [SerializeField] Animator indicatorAnim;
    [SerializeField] string indicatorAnimName;
    private bool playTimerIndicator = true;
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public Collider2D collider;

    [Space(10)]
    [Header("Drop-Through Platform")]
    [SerializeField] private GameObject currentOneWayPlatform;
    [SerializeField] public bool canDropThrough = false;

    protected virtual void Awake()
    {
        if(animator == null) animator = GetComponent<Animator>();
        if(rb == null) rb = GetComponent<Rigidbody2D>();
        if(collider == null) collider = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        animator.Play(movingAnimName);
        Invoke("DetonateGrenade", grenadeTimer);
        indicatorObj.SetActive(true);
        playTimerIndicator = true;
        StartCoroutine(TimerIndicator());
    }

    protected virtual void Update()
    {
        
    }

    protected virtual void DetonateGrenade()
    {
        playTimerIndicator = false;
        rb.velocity = Vector2.zero;
        animator.Play(explodeAnimName);
        GameObject explosionObj = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        CastExplosion explosion = explosionObj.GetComponent<CastExplosion>();
        explosion.damage = damage;
        explosion.knockbackStrength = knockbackStrength;

        rb.simulated = false;

        Invoke("DelayedDestroy", explodeAnimTime);
    }

    void DelayedDestroy()
    {
        Destroy(gameObject);
    }

    IEnumerator TimerIndicator()
    {
        //play animation once every second
        while(playTimerIndicator)
        {
            indicatorAnim.Play(indicatorAnimName);
            yield return new WaitForSeconds(1f);
        }
    }

#region Drop Through

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = collision.gameObject;
            DropThroughCheck();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = null;
        }
    }

    public void TempAllowDropThrough()
    {
        canDropThrough = true;
        StartCoroutine(TempAllowDropThroughCO());
    }

    IEnumerator TempAllowDropThroughCO()
    {
        //Timer to prevent dropping through the Player's platform if the drone is on a ledge
        yield return new WaitForSeconds(.5f); 
        canDropThrough = false;
    }

    void DropThroughCheck()
    {
        if(!canDropThrough) return;
        canDropThrough = false;
        StartCoroutine(DisableCollision());
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(collider, platformCollider);
        yield return new WaitForSeconds(.25f);
        Physics2D.IgnoreCollision(collider, platformCollider, false);
    }

#endregion
}
