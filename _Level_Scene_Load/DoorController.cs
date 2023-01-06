using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("References & Setup")]
    [SerializeField] bool horizontal;
    [SerializeField] Collider2D doorCollider;
    [SerializeField] BoxCollider2D groundCollider;
    [SerializeField] Collider2D blockDropThroughCollider;
    [SerializeField] Animator animator;
    [SerializeField] string[] animNames = { "Door_Closed", "Door_Opening", "Door_Opened" };
    [SerializeField] GameObject doorArrow; //TODO: replace with arrow

    [Header("Variables")]
    public bool isOpen;


    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
        if (horizontal && groundCollider == null) groundCollider = gameObject.transform.Find("Ground Platform").GetComponent<BoxCollider2D>();
        //blockDropThroughCollider
        ToggleOpenIndicator(false);
    }
    
    void Update()
    {
        //ToggleDoor(isOpen);
    }

    public void ToggleDoor(bool toggle)
    {
        isOpen = toggle;
        if(isOpen) PlayAnim(1); //Open anim
        else PlayAnim(0); //Close anim

        if(toggle) Invoke("OpenDoorCollider", .5f);
        else Invoke("CloseDoorCollider", .1f);
        // doorCollider.isTrigger = toggle;
    }

    void OpenDoorCollider() //Change collider to trigger, allow dropThrough
    {
        // if(groundCollider != null) groundCollider.tag = "OneWayPlatform";
        doorCollider.isTrigger = true;
        //GameManager.Instance.PlayerMovement.currentOneWayPlatform = gameObject;
        if(blockDropThroughCollider != null) blockDropThroughCollider.enabled = false;
        //Override canDropThrough in case player is on the platform when it changes
        //if(horizontal) GameManager.Instance.PlayerMovement.canDropThrough = true;
    }

    void CloseDoorCollider() //Disable trigger, block dropThrough
    {
        // if(groundCollider != null) groundCollider.tag = "SolidPlatform";
        doorCollider.isTrigger = false;
        if(blockDropThroughCollider != null) blockDropThroughCollider.enabled = true;
    }

    void ToggleOpenIndicator(bool toggle)
    {
        if (doorArrow != null)
            doorArrow.SetActive(toggle);
    }

    private void PlayAnim(int index)
    {
        animator.Play(animNames[index]);
    }
}
