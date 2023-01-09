using System.Collections;
using UnityEngine;

public class HitEffectsAnimator : MonoBehaviour
{
    // Plays an animation from an array of animation names
    // Once animation is done, object is added to a pool
    [Header("References")]
    [SerializeField] ObjectPoolerList pool;
    [SerializeField] Animator animator;

    [Header("Main Effects")]
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
