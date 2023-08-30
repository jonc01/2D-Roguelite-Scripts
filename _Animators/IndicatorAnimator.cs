using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] string animationName;
    [SerializeField] private int hashedAnimName;
    [SerializeField] float animationTime;

    

    private void OnEnable()
    {
        if(hashedAnimName == 0) animator.Play(animationName);
        else animator.Play(hashedAnimName);
        Invoke("ToggleOff", animationTime);
    }

    private void ToggleOff()
    {
        gameObject.SetActive(false);
    }
}
