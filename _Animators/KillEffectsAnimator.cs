using System.Collections;
using UnityEngine;

public class KillEffectsAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] string animationName;
    [SerializeField] float animationTime;

    private void OnEnable()
    {
        animator.Play(animationName);
        Invoke("DeleteObj", animationTime);
    }

    private void DeleteObj()
    {
        Destroy(gameObject);
    }
}
