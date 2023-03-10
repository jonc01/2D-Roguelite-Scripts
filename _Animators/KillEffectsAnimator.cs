using System.Collections;
using UnityEngine;

public class KillEffectsAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] string animationName;
    [SerializeField] float animationTime;
    [SerializeField] bool TOGGLE = false;

    private void OnEnable()
    {
        animator.Play(animationName);
        Invoke("DeleteObj", animationTime);
    }

    private void DeleteObj()
    {
        if(TOGGLE) gameObject.SetActive(false);
        else Destroy(gameObject);
    }
}
