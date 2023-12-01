using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRoomManager : MonoBehaviour
{
    //Moves Camera to the Player's current room
    //! - Attach to Origin Child object with collider
    [Header("- Player Scene Only -")]
    [SerializeField] bool inPlayerScene = false;
    [Space(20)]

    [Header("References")]
    [SerializeField] public Transform CameraAnchor;
    [SerializeField] Vector3 originPosition;
    [SerializeField] bool playerIsHere;
    [SerializeField] RoomClear roomClear;
    float yOffset;
    [SerializeField] DoorManager doorManager;

    void Start()
    {
        if(!inPlayerScene)
        {
            SetCameraAnchor();
        }

        originPosition = GetComponentInParent<Transform>().position;
        
        if(doorManager == null) doorManager = GetComponentInParent<DoorManager>();
        if(roomClear == null) roomClear = GetComponentInParent<RoomClear>();
    }

    // void FixedUpdate()
    // {
    //     //may need to add another check to see if camera is in the correct room
    //     // if(CameraAnchor == null) CameraAnchor = GameObject.FindGameObjectWithTag("CameraAnchor").transform; //TODO: gamemanager test
    // }

    public void SetCameraAnchor()
    {
        CameraAnchor = GameObject.FindGameObjectWithTag("CameraAnchor").transform;
        yOffset = CameraAnchor.position.y;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            RelocateCamera();
            playerIsHere = true; //Debugging
            roomClear.ToggleMinimapIcon(true);
            doorManager.UpdateDoorState(.05f); //Open/Close doors if room has been cleared
        }

        if(collision.CompareTag("RevealRoom"))
        {
            //Revealing border rooms to the Player's current room
            roomClear.RevealRoomIconOnly();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        playerIsHere = false;
    }

    public void RelocateCamera()
    {
        //Call manually once player enters a new Room
        //Move cameraAnchor object to transform of the Player's current room
        if (CameraAnchor == null) return;
        CameraAnchor.position = 
        new Vector3(originPosition.x, originPosition.y + yOffset, CameraAnchor.position.z);
    }

    public void RelocateCamera(Vector3 pos)
    {
        if (CameraAnchor == null) return;
        CameraAnchor.position = 
        new Vector3(pos.x, pos.y + yOffset, CameraAnchor.position.z);
    }

    public Vector3 GetCameraRoom()
    {
        //Returns a Vector of the current origin position of the room the Camera is in
        if (CameraAnchor == null) return new Vector3(0, 0, 0);
        return new Vector3(CameraAnchor.position.x, CameraAnchor.position.y-yOffset, CameraAnchor.position.z); 
    }
}
