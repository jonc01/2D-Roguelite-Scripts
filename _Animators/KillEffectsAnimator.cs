using System.Collections;
using UnityEngine;

public class KillEffectsAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] string animationName;
    [SerializeField] private int hashedAnimName;
    [SerializeField] float animationTime;
    [SerializeField] bool TOGGLE = false;

    private void OnEnable()
    {
        if(hashedAnimName == 0) animator.Play(animationName);
        else animator.Play(hashedAnimName);
        Invoke("DeleteObj", animationTime);
    }

    private void DeleteObj()
    {
        if(TOGGLE) gameObject.SetActive(false);
        else Destroy(gameObject);
    }
}
