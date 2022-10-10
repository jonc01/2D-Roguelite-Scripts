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
        collider.isTrigger = false;
    }

    void OpenDoor()
    {
        PlayAnim(1);
        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Player entered door");
        //TODO: Move player to new connected scene
    }

    private void PlayAnim(int index)
    {
        animator.Play(animNames[index]);
    }
}
