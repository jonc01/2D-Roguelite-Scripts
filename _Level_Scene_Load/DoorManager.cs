using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    //Controls doors in Stages
    [SerializeField] public List<DoorController> connectedDoors;
    [SerializeField] public List<GameObject> minimapIcons;
    private bool checkForDoors;
    public RoomClear roomClear;
    public bool startingRoom = false;

    void Start()
    {
        // startingRoom = false; //Gets set from EnemyStageManager at start //TODO: doesn't work, because of timers
        connectedDoors = new List<DoorController>();
        minimapIcons = new List<GameObject>();
        roomClear = GetComponent<RoomClear>();

        if(transform.position == new Vector3 (0,0,0)) startingRoom = true;
    }

    public void AddToList(DoorController door)
    {
        connectedDoors.Add(door);
    }

    public void AddIconToList(GameObject icon)
    {
        minimapIcons.Add(icon);
    }

    //Called from RoomManager
    public void OpenAllDoors(bool toggle = true)
    {
        //StageClear calls this when all enemies are dead
        //Opens all door child objects
        if(connectedDoors.Count == 0) return;
        for (int i = 0; i < connectedDoors.Count; i++)
        {
            if(connectedDoors[i] != null) connectedDoors[i].ToggleDoor(toggle);
        }
    }

    public void DelayedRevealDoor(float delay = .08f)
    {
        Invoke("RevealDoors", delay);
    }

    void DelayedHideDoor()
    {
        RevealDoors(false);
    }

    public void RevealDoors(bool toggle = true)
    {
        if(minimapIcons.Count == 0) return;
        for (int i = 0; i < minimapIcons.Count; i++)
        {
            if(minimapIcons[i] != null) minimapIcons[i].SetActive(toggle);
        }
    }

    public void UpdateDoorState(float delay = 0)
    {
        //GetComponent needed here as Start would sometimes miss the reference get
        if(roomClear == null) roomClear = GetComponent<RoomClear>();
        if(roomClear.trialRoom)
        {
            OpenAllDoors(true);
            return;
        }

        if(delay > 0) Invoke("DelayUpdateDoorState", delay);
        else OpenAllDoors(roomClear.roomCleared);

        roomClear.CheckSpawn();
    }

    private void DelayUpdateDoorState()
    {
        OpenAllDoors(roomClear.roomCleared);
    }
}
