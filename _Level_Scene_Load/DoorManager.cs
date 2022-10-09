using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    //Controls doors in Stages
    [Header("References")]
    [SerializeField] DoorController[] doors;
    [SerializeField] int totalDoors;

    void Start()
    {
        totalDoors = transform.childCount;
        doors = new DoorController[totalDoors];
        for(int i = 0; i < totalDoors; i++)
        {
            doors[i] = transform.GetChild(i).GetComponent<DoorController>();
        }
    }

    public void OpenDoors()
    {
        //StageClear call this when all enemies are dead
        //Opens all door child objects
        if (totalDoors <= 0) return;
        Debug.Log("Open doors");
        for (int i = 0; i < totalDoors; i++)
        {
            doors[i].isOpen = true;
        }
    }
}
