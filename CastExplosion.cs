using System.Collections;
using UnityEngine;

public class CastExplosion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayAudioClips playAudioClips;

    [Header("Animation Variables")]
    //Separate animators to play at different scales
    [SerializeField] private Animator animChargeUp;
    [SerializeField] float chargeDuration = 0.667f;
    [SerializeField] int chargeUpHashedInt;
    [Space]
    [SerializeField] private Animator animExplosion;
    [SerializeField] float explosionDuration = 0.583f;
    [SerializeField] int explosionHashedInt;
    [Space(10)]
    [SerializeField] float animDelay = 0;
    [Space(10)]

    [Header("Adjustable Variables")]
    [SerializeField] int screenshakeIntensity = 1;
    [SerializeField] float startDelay = 0;

    [Header("(optional) Move to Ground")]
    [SerializeField] public bool moveToGround = false;
    [SerializeField] Transform optionalRaycast;
    [SerializeField] float groundCheckDist = 1;
    [SerializeField] LayerMask groundLayer;
    private bool checkForGround = false;

    [Space(10)]

    [Header("Knockback")]
    public bool hasKnockBack = true;
    [SerializeField] public float knockbackStrength = 2f;
    [SerializeField] float knockbackDuration = .5f;

    [Header("KnockUp")]
    public bool hasKnockUp = false;
    [SerializeField] public float knockupStrength = 4f;
    [SerializeField] float knockupDuration = .5f;
    [SerializeField] bool directionalKnockback = true;

    [Header("=== TOGGLE Prefab ===")]
    [SerializeField] bool TOGGLE = false;
    [SerializeField] bool POOLED = false;

    [Space(10)]
    [Header("Damage Variables")]
    public LayerMask targetLayer;
    public float damage = 5f;
    [SerializeField] private Transform overlapOffset;
    [SerializeField] Vector3 hitboxSize;

    void Awake()
    {
        if (animChargeUp == null) animChargeUp = GetComponentInChildren<Animator>();
        if (animExplosion == null) animExplosion = GetComponent<Animator>();
        if (playAudioClips == null) playAudioClips = GetComponentInChildren<PlayAudioClips>();
        if (TOGGLE) gameObject.SetActive(false);
        checkForGround = false;

    }

    void Start()
    {
        //! - Make sure animations are default set to a blank frame anim in animation controller
        // if (!TOGGLE) StartCoroutine(PlayAnims());
    }

    void OnEnable()
    {
        // if(moveToGround) checkForGround = true;

        // if (TOGGLE) StartCoroutine(PlayAnims());
        StartCoroutine(PlayAnims());
    }

    void Update()
    {
        if(checkForGround)
        {
            GetGround();
            DebugDrawRaycast();
        }
    }

    private void GetGround()
    {
        //Point raycast down to get ground, then move explosion to ground
        RaycastHit2D hit = Physics2D.Raycast(optionalRaycast.position, Vector2.down, groundCheckDist, groundLayer);
        if(hit.collider != null)
        {
            checkForGround = false;
            transform.position = hit.point;
        }
    }

    void DebugDrawRaycast()
    {
        Vector3 down = transform.TransformDirection(Vector3.down) * groundCheckDist;
        Debug.DrawRay(optionalRaycast.position, down, Color.red);
    }

    IEnumerator PlayAnims()
    {
        if(moveToGround)
        {
            yield return new WaitForSeconds(.02f);
            checkForGround = true;
        }

        yield return new WaitForSeconds(startDelay);

        if(animChargeUp != null) //Skip this delay if no ChargeUp
        {
            animChargeUp.Play(chargeUpHashedInt);//ExplosionChargeUp");
            yield return new WaitForSeconds(chargeDuration);
        }

        animExplosion.Play(explosionHashedInt);//"Explosion");

        yield return new WaitForSeconds(animDelay);
        CheckHit();
        if(playAudioClips) playAudioClips.PlayAttackSwing();

        ScreenShakeListener.Instance.Shake(screenshakeIntensity);
        yield return new WaitForSeconds(explosionDuration);
        
        // Toggle objects are Disabled, otherwise Destroy
        if (TOGGLE) gameObject.SetActive(false);
        else if (!POOLED) Destroy(gameObject);
    }

    void CheckHit()
    {
        Collider2D collider = Physics2D.OverlapBox(overlapOffset.position, hitboxSize, 0f, targetLayer);
        if(collider == null) return;
        var player = collider.GetComponent<Base_PlayerCombat>();
        //Check when there is a new collider coming into contact with the box
        if (player != null)
        {
            player.TakeDamage(damage);

            if(hasKnockBack)
            {
                bool playerToRight;

                if(directionalKnockback){
                    if(transform.rotation.y == 0) playerToRight = false;
                    else playerToRight = true;
                }else{
                    if(transform.position.x < player.transform.position.x) playerToRight = true;
                    else playerToRight = false;
                }
                player.GetKnockback(playerToRight, knockbackStrength, knockbackDuration);
            }

            if(hasKnockUp) //TODO: set true for Melee Explosion
            {
                player.GetKnockup(knockupStrength, knockupDuration); //TODO: test delay/recovery
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(overlapOffset.position, hitboxSize);
    }
}
