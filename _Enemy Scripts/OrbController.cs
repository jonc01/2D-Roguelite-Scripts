using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    protected float startChaseDelay = .5f;
    public float orbSpeed = 5f;

    [Header("Debugging")]
    [SerializeField] protected bool findPlayer;

    [Space(10)]

    protected Rigidbody2D rb;
    protected CircleCollider2D collider;
    protected Transform player;
    protected Inventory inventory;
    protected Animator animator;

    protected Vector2 launchForce;
    public float direction = -1f; //-1 = left, 1 = right
    protected float xV, yV;
    [SerializeField] protected float xVelocityLower = 100;
    [SerializeField] protected float xVelocityUpper = 130;
    [SerializeField] protected float yVelocityLower = 130;
    [SerializeField] protected float yVelocityUpper = 160;

    protected bool hitDone;
    protected bool xpGiven;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
        findPlayer = false;

        xV = Random.Range(xVelocityLower, xVelocityUpper);
        yV = Random.Range(yVelocityLower, yVelocityUpper);

        player = GameManager.Instance.playerTargetOffset;
        inventory = GameManager.Instance.Inventory;
        animator = GetComponent<Animator>();
        hitDone = false;
        xpGiven = false;
    }

    protected virtual void OnEnable()
    {
        direction = Random.Range(-1f, 1f); //random left/up/right
        launchForce = new Vector2(direction * xV, yV);
        rb.AddForce(launchForce);
        StartCoroutine(MoveToPlayer());
    }

    protected virtual void FixedUpdate()
    {
        if (!findPlayer || hitDone) return;

        orbSpeed *= 1.08f;
        var step = orbSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.position, step);

        if(Vector3.Distance(transform.position, player.position) < 0.2f)
        {
            HitPlayer();
        }
    }

    protected IEnumerator MoveToPlayer()
    {
        if (player != null) yield return null;
        yield return new WaitForSeconds(startChaseDelay);
        DisableColliderGrav();
    }

    protected void DisableColliderGrav()
    {
        findPlayer = true;
        collider.isTrigger = true; //allows orbs to fly through ground/walls
        rb.gravityScale = 0;
        rb.drag = 0;
    }

    protected virtual void HitPlayer()
    {
        //Check to make sure hits aren't registered multiple times on collision
        if (hitDone) return;
        // hitDone = true
        GiveGold(); //GiveXP

        Invoke("DisableVelocity", .25f);

        animator.Play("PuffOfSmoke");
        Invoke("DestroyObject", 0.67f); //Delay to play animation
    }

    private void GiveGold()
    {
        if(xpGiven) return;
        xpGiven = true;
        inventory.UpdateGold(1); 
    }

    protected void DisableVelocity()
    {
        hitDone = true;
        rb.velocity = Vector2.zero;
    }

    protected void DestroyObject()
    {
        Destroy(this.gameObject);
    }

}
