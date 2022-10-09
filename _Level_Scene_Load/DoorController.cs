using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Collider2D collider;
    [SerializeField] Animator animator;
    [SerializeField] string[] animNames = { "Door_Closed", "Door_Opening", "Door_Opened" };

    [Header("Variables")]
    public bool isOpen;


    void Start()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
    }
    
    void Update()
    {
        if (!isOpen) CloseDoor();
        if (isOpen) OpenDoor();
    }

    void CloseDoor()
    {
        PlayAnim(0);
        collider.enabled = true;
    }

    void OpenDoor()
    {
        PlayAnim(1);
        collider.enabled = false;
    }

    private void PlayAnim(int index)
    {
        animator.Play(animNames[index]);
    }
}
