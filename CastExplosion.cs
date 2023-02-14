using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastExplosion : MonoBehaviour
{
    [Header("Animation Variables")]
    //Separate animators to play at different scales
    [SerializeField] private Animator animExplosion;
    [SerializeField] private Animator animChargeUp;
    [SerializeField] float chargeDuration = 0.667f;
    [SerializeField] float explosionDuration = 0.583f;

    [Space]
    [Header("Damage Variables")]
    public LayerMask targetLayer;
    public float damage = 5f;
    [SerializeField] private Transform overlapOffset;
    [SerializeField] Vector3 hitboxSize;

    void Awake()
    {
        if (animExplosion == null) animExplosion = GetComponent<Animator>();
        if (animChargeUp == null) animChargeUp = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        animExplosion.Play(-49316988); //"DefaultBlank"
        StartCoroutine(PlayAnims());
    }

    // void OnEnable()
    // { 
    //     StartCoroutine(PlayAnims());
    // }

    IEnumerator PlayAnims()
    {
        animChargeUp.Play(-1454319710);////ExplosionChargeUp");
        yield return new WaitForSeconds(chargeDuration);
        CheckHit();
        animExplosion.Play(-211360833);//"Explosion");
        yield return new WaitForSeconds(explosionDuration);
        gameObject.SetActive(false);
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

            bool playerToRight;
            if(transform.position.x < player.transform.position.x) playerToRight = true;
            else playerToRight = false;

            player.GetKnockback(playerToRight);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(overlapOffset.position, hitboxSize);
    }
}
