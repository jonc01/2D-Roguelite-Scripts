using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    private float startChaseDelay = .5f;
    public float orbSpeed = 5f;

    [Header("Debugging")]
    [SerializeField] private bool findPlayer;

    [Space(10)]

    Rigidbody2D rb;
    CircleCollider2D collider;
    Transform player;
    Inventory inventory;
    Animator animator;

    Vector2 launchForce;
    public float direction = -1f; //-1 = left, 1 = right
    float xV, yV;
    [SerializeField] float xVelocityLower = 100;
    [SerializeField] float xVelocityUpper = 130;
    [SerializeField] float yVelocityLower = 130;
    [SerializeField] float yVelocityUpper = 160;


    private bool hitDone;

    private void Awake()
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
    }

    private void OnEnable()
    {
        direction = Random.Range(-1f, 1f); //TODO: testing, random left/up/right
        launchForce = new Vector2((direction * xV), yV);
        rb.AddForce(launchForce);
        StartCoroutine(MoveToPlayer());
    }

    private void FixedUpdate()
    {
        if (!findPlayer) return;

        orbSpeed *= 1.08f;
        var step = orbSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.position, step);

        if(Vector3.Distance(transform.position, player.position) < 0.2f)
        {
            HitPlayer();
        }
    }

    IEnumerator MoveToPlayer()
    {
        if (player != null) yield return null;
        yield return new WaitForSeconds(startChaseDelay);
        DisableColliderGrav();
    }

    void DisableColliderGrav()
    {
        findPlayer = true;
        collider.isTrigger = true; //allows orbs to fly through ground/walls
        rb.gravityScale = 0;
        rb.drag = 0;
    }

    void HitPlayer()
    {
        //Check to make sure hits aren't registered multiple times on collision
        if (hitDone) return;
        hitDone = true;
        inventory.UpdateGold(1); //GiveXP
        animator.Play("PuffOfSmoke");

        Invoke("DestroyObject", 0.67f); //Delay to play animation
    }

    void DestroyObject()
    {
        Destroy(this.gameObject);
    }
}
