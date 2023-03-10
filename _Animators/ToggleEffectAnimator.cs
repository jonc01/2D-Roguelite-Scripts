using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleEffectAnimator : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Animator anim;
    [SerializeField] int[] HashedAnimName;
    [SerializeField] float[] ChargeAnimTimes;
    [SerializeField] float[] FullAnimTimes;

    void Awake()
    {
        if (anim == null) GetComponent<Animator>();
    }

    public void PlayAnim(int index)
    {
        anim.Play(HashedAnimName[index]);
    }

    public float GetChargeTime(int index)
    {
        return ChargeAnimTimes[index];
    }

    public float GetAnimTime(int index)
    {
        return FullAnimTimes[index];
    }
}
