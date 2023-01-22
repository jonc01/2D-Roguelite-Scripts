using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObjectAnimator : MonoBehaviour
{
    //! - Attach this to the Prefab
    // Plays an animation from an array of animation names
    // Once animation is done, object is added to a pool
    [Header("References")]
    [SerializeField] ObjectPoolerList pool;
    [SerializeField] Animator animator;
    [SerializeField] private string animName;
    [SerializeField] private float animTime;
    private GameObject prefab;

    private void Start()
    {
        pool = GetComponentInParent<ObjectPoolerList>();
    }

    private void OnEnable()
    {
        animator.Play(animName);
        StartCoroutine(PoolObject(animTime));
    }

    IEnumerator PoolObject(float duration)
    {
        yield return new WaitForSeconds(duration);
        pool.ReturnObject(gameObject);
    }
}
