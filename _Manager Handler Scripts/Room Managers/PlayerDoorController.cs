using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDoorController : MonoBehaviour
{
    //When player enters a new room entrance, move the player out of the doorway into the room
    
    [Header("References")]
    [SerializeField] DoorController doorController;
    [SerializeField] Transform teleportEndpoint;

    void Start()
    {
        if(doorController == null) doorController = GetComponentInParent<DoorController>();
        if(teleportEndpoint == null) teleportEndpoint = transform.GetChild(0).transform;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //When player enters trigger, reposition to the teleportEndpoint in the next room
        if(!doorController.isOpen) return;
        if(collision.CompareTag("Player"))
        {
            GameManager.Instance.playerTransform.position = teleportEndpoint.position;
        }
    }
}
