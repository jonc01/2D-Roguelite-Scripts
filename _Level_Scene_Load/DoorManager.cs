using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    //Controls doors in Stages
    [SerializeField] public List<DoorController> connectedDoors;
    private bool checkForDoors;
    public RoomClear roomClear;

    void Start()
    {
        connectedDoors = new List<DoorController>();
        roomClear = GetComponent<RoomClear>();
    }

    public void AddToList(DoorController door)
    {
        connectedDoors.Add(door);
    }

    //Called from RoomManager
    public void ToggleAllDoors(bool toggle = true)
    {
        //StageClear calls this when all enemies are dead
        //Opens all door child objects
        if(connectedDoors.Count == 0) return;
        for (int i = 0; i < connectedDoors.Count; i++)
        {
            if(connectedDoors[i] != null) connectedDoors[i].ToggleDoor(toggle);
        }
    }

    public void UpdateDoorState(float delay = 0)
    {
        if(delay > 0) Invoke("DelayUpdateDoorState", delay);
        else ToggleAllDoors(roomClear.roomCleared);
    }

    private void DelayUpdateDoorState()
    {
        ToggleAllDoors(roomClear.roomCleared);
    }
}
