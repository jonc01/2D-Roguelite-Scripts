using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] bool findPlayer;
    [SerializeField] float timeSpentFlying;

    [Space(10)]

    public OrbHolder orbHolder;
    Rigidbody2D rb;
    CircleCollider2D collider;
    Transform player;
    Inventory inventory;
    Animator animator;

    Vector2 launchForce;
    public float direction = -1f; //-1 = left, 1 = right
    float xV, yV;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        collider = gameObject.GetComponent<CircleCollider2D>();
        orbHolder = gameObject.GetComponentInParent<OrbHolder>();
        findPlayer = false;
        timeSpentFlying = 0;

        xV = Random.Range(100, 150);
        yV = Random.Range(100, 150);

        player = GameManager.Instance.PlayerTargetOffset;
        inventory = GameManager.Instance.Inventory;
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        direction = orbHolder.launchDirection;

        launchForce = new Vector2((direction * xV), yV);
        rb.AddForce(launchForce);
        StartCoroutine(MoveToPlayer());
    }

    private void Update()
    {
        if (!findPlayer) return;

        timeSpentFlying += Time.deltaTime * 4f;
        var step = (orbHolder.orbSpeed + timeSpentFlying) * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.position, step);

        if(Vector3.Distance(transform.position, player.position) < 0.1f)
        {
            HitPlayer();
        }
    }

    IEnumerator MoveToPlayer()
    {
        if (player != null) yield return null;
        yield return new WaitForSeconds(2);
        timeSpentFlying = .1f;
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
        inventory.GiveGold(1);
        animator.Play("PuffOfSmoke");
        Invoke("DestroyObject", 0.67f);
    }

    void DestroyObject()
    {
        Destroy(this.gameObject);
    }
}
