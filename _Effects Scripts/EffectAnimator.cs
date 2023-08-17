using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAnimator : MonoBehaviour
{
    [SerializeField] string animName;
    [SerializeField] Animator animator;

    void OnEnable()
    {
        animator.Play(animName);
    }
}
