using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    //
    [Header("References")]
    [SerializeField] Animator anim;
    [SerializeField] int[] animHashes;
    [SerializeField] float[] animTimes;
    public bool toggled = false;


    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        anim.Play(animHashes[0]);
    }

    void Update()
    {
        if(toggled)
        {
            ToggleTeleporter();
        }
    }

    public void ToggleTeleporter()
    {
        StartCoroutine(PlayAnim(1)); //turning on animation
    }
    
    IEnumerator PlayAnim(int index)
    {
        anim.Play(animHashes[index]); 
        yield return new WaitForSeconds(animTimes[index]);
    }

    //TODO: add interact prompt with Player
}
