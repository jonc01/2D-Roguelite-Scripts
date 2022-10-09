using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectsAnimator : MonoBehaviour
{
    [SerializeField] private ObjectPoolerList pool;
    [SerializeField] Animator animator;
    public string[] animations;
    public float[] animTimes;

    private void Start()
    {
        pool = GetComponentInParent<ObjectPoolerList>();
    }

    private void OnEnable()
    {
        int rand = Random.Range(0, animations.Length);
        animator.Play(animations[rand]);
        StartCoroutine(PoolObject(animTimes[rand]));
    }

    IEnumerator PoolObject(float duration)
    {
        yield return new WaitForSeconds(duration);
        pool.ReturnObject(gameObject);
    }
}
