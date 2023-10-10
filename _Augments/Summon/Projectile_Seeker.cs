using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Seeker : MonoBehaviour
{
    [Header("- Variable Setup -")]
    public float damage = 5;
    public float knockbackStrength = 1;
    public float flightSpeed = 3;
    [SerializeField] private bool projectileHit;
    [SerializeField] protected float setTargetRadius = 1;
    [SerializeField] protected float hitRadius = .4f;

    [Space(10)]

    [Header("- Launch Setup -")]
    [SerializeField] protected float flyToTargetDelay = .1f;
    [SerializeField] protected Vector2 launchForce;
    public float direction = -1f; //-1 = left, 1 = right
    protected float xV, yV;
    [SerializeField] protected float xVelocityLower = 100;
    [SerializeField] protected float xVelocityUpper = 130;
    [SerializeField] protected float yVelocityLower = 130;
    [SerializeField] protected float yVelocityUpper = 160;

    [Space(10)]

    [Header("- References -")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected int hashedAnimName;
    [SerializeField] private float hitAnimTime;

    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected CircleCollider2D collider;
    [SerializeField] protected Rigidbody2D rb;

    [Space(10)]
    [Header("- Debug -")]
    [SerializeField] protected bool flyToTarget;
    [SerializeField] Transform targetTransform;
    [SerializeField] Vector3 targetPos;
    [SerializeField] bool showGizmos = false;
    [SerializeField] List<Transform> nearbyTargets;

    protected virtual void Start()
    {
        if(animator == null) animator = GetComponent<Animator>();
        projectileHit = false;
        if(collider == null) collider = GetComponent<CircleCollider2D>();
        if(rb == null) rb = GetComponent<Rigidbody2D>();

        nearbyTargets = new List<Transform>();

        xV = Random.Range(xVelocityLower, xVelocityUpper);
        yV = Random.Range(yVelocityLower, yVelocityUpper);

        flyToTarget = false;

        StartCoroutine(FlyToTarget());
    }

    protected virtual void Update()
    {
        if(!flyToTarget || projectileHit) return;

        //Increase flight speed over time
        flightSpeed *= 1.08f;
        var step = flightSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        if(Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            HitTarget();
        }
    }

    protected void GetTargets()
    {
        //Get all enemies within range
        Collider2D[] hitEnemies = 
            Physics2D.OverlapCircleAll(transform.position, setTargetRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                targetTransform = damageable.GetHitPosition();
                nearbyTargets.Add(targetTransform);
            }
        }
    }

    protected void SetTarget()
    {
        if(nearbyTargets.Count == 0)
        {
            targetPos = transform.position;
            HitTarget();
            return;
        }

        //Compare distances of nearby enemies to find the closest target
        int closestTargetIdx = 0;
        float closestDist = Vector3.Distance(nearbyTargets[closestTargetIdx].position, transform.position);
        for(int i=0; i<nearbyTargets.Count; i++)
        {
            float dist = Vector3.Distance(nearbyTargets[i].position, transform.position);
            if(dist <= closestDist)
            {
                closestDist = dist;
                closestTargetIdx = i;
            }
        }



        //Set target
        targetPos = nearbyTargets[closestTargetIdx].position;
    }
    
    protected IEnumerator FlyToTarget()
    {
        direction = Random.Range(-1f, 1f); //random left/up/right
        launchForce = new Vector2(direction * xV, yV);
        rb.AddForce(launchForce);

        yield return new WaitForSeconds(.2f);
        GetTargets();
        yield return new WaitForSeconds(.1f);
        SetTarget();
        yield return new WaitForSeconds(flyToTargetDelay);
        flyToTarget = true;
    }

    protected virtual void HitTarget()
    {
        //Checks to make sure hits aren't registered multiple times on collision
        if (projectileHit) return;

        animator.Play(hashedAnimName);
        ExplodeHit();
        DisableVelocity();
        Invoke("DestroyObject", hitAnimTime); //Delay to allow animation to play
    }

    protected void ExplodeHit()
    {
        Collider2D[] hitEnemies = 
            Physics2D.OverlapCircleAll(transform.position, hitRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                projectileHit = true;
                damageable.TakeDamage(damage, true, true, knockbackStrength, transform.position.x);
            }
        }
    }

    protected void DisableVelocity()
    {
        //Disable gravity and stop velocity to keep projectile in place
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.DrawWireSphere(transform.position, setTargetRadius);
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
    }
}
