using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Setup Horizontal ONLY")]
    [SerializeField] bool horizontal; //Only horizontal needs a blockDropThrough
    [SerializeField] Collider2D blockDropThroughCollider;
    [SerializeField] BoxCollider2D groundCollider;

    [Header("References & Setup")]
    [SerializeField] Collider2D doorCollider;
    public Transform doorOffset; //For use with door props
    [SerializeField] Animator animator;
    [SerializeField] string[] animNames = { "Door_Closed", "Door_Opening", "Door_Opened" };
    [SerializeField] GameObject doorArrow; //TODO: replace with arrow

    [Header("- Boss Door Only -")]
    [SerializeField] public bool isBossDoor = false;
    [SerializeField] GameObject DoorLockedIcon;
    [SerializeField] GameObject[] doorLockIcons;
    [SerializeField] TextMeshPro[] lockConditionsText;

    [Header("Variables")]
    public bool isOpen;


    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
        if (horizontal && groundCollider == null) groundCollider = gameObject.transform.Find("Ground Platform").GetComponent<BoxCollider2D>();
        //blockDropThroughCollider
        if(doorCollider != null) doorOffset = doorCollider.transform;
        ToggleOpenIndicator(false);

        if(DoorLockedIcon != null)
        {
            DoorLockedIcon.SetActive(true);
        }

        if(isBossDoor)
        {
            isOpen = false;
            GameManager.Instance.AddBossDoor(this);
            DisplayLockIcons(true);
        } 
    }
    
    void Update()
    {
        //ToggleDoor(isOpen); //debugging
    }

    public void DisplayLockIcons(bool toggle)
    {
        if(doorLockIcons.Length == 0 || lockConditionsText.Length == 0) return;
        int clearedTrials = GameManager.Instance.totalTrialsCleared;
        int totalClearsNeeded = GameManager.Instance.totalTrialsNeeded;

        for(int i=0; i<doorLockIcons.Length; i++)
        {
            doorLockIcons[i].SetActive(toggle);
            lockConditionsText[i].text = clearedTrials + "/" + totalClearsNeeded;
        }
    }

    public void ToggleDoor(bool toggle)
    {
        if(isBossDoor)
        {
            if(GameManager.Instance.CheckBossUnlock()) isOpen = true;
            else
            {
                isOpen = false;
                return;
            }
        }

        isOpen = toggle;
        if(isOpen) PlayAnim(1); //Open anim
        else PlayAnim(0); //Close anim

        if(toggle) Invoke("OpenDoorCollider", .5f);
        else Invoke("CloseDoorCollider", .1f);
        // doorCollider.isTrigger = toggle;
        if(DoorLockedIcon != null)
        {
            DoorLockedIcon.SetActive(!toggle);
        }
    }

    void OpenDoorCollider() //Change collider to trigger, allow dropThrough
    {
        // if(groundCollider != null) groundCollider.tag = "OneWayPlatform";
        doorCollider.isTrigger = true;
        if(blockDropThroughCollider != null) blockDropThroughCollider.enabled = false;

        ToggleOpenIndicator(true);
        //Override canDropThrough in case player is on the platform when it changes
        //if(horizontal) GameManager.Instance.PlayerMovement.canDropThrough = true;
    }

    void CloseDoorCollider() //Disable trigger, block dropThrough
    {
        // if(groundCollider != null) groundCollider.tag = "SolidPlatform";
        doorCollider.isTrigger = false;
        if(blockDropThroughCollider != null) blockDropThroughCollider.enabled = true;
        ToggleOpenIndicator(false);
    }

    void ToggleOpenIndicator(bool toggle)
    {
        if (doorArrow != null) doorArrow.SetActive(toggle);
    }

    private void PlayAnim(int index)
    {
        animator.Play(animNames[index]);
    }
}
