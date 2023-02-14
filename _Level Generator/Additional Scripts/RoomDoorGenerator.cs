using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDoorGenerator : MonoBehaviour
{
    //! - Not in-use
    //If using, call after Rooms are generated
    //

    [Header("References")]
    private LevelBuilder Builder;
    [SerializeField] GameObject BossRoomOrigin;
    [SerializeField] List<Transform> borderingDoors;

    [SerializeField] GameObject BossDoorProps;

    private void Start()
    {
        borderingDoors = new List<Transform>();
        if (Builder == null) Builder = GetComponentInParent<LevelBuilder>();
    }

    public void AddBorderingDoors()
    {
        Invoke("GetBorderingDoors", .1f);
    }

    public void GetBorderingDoors() //Call in RoomGenerator after Walls are already added
    {
        borderingDoors = new List<Transform>();
        // borderingDoors.Clear();

        BossRoomOrigin = Builder.GeneratedOrigins[Builder.GeneratedOrigins.Length - 1];

        DoorManager bossDoors = BossRoomOrigin.GetComponent<DoorManager>();
        int totalDoors = bossDoors.connectedDoors.Count;

        for(int i = 0; i < totalDoors; i++)
        {
            borderingDoors.Add(bossDoors.connectedDoors[i].transform);
        }

        Invoke("AddBossDoor", .1f);
        //yield return on reference != null
    }

    void AddBossDoor()
    {
        for(int i = 0; i < borderingDoors.Count; i++)
        {
            Transform currDoor = borderingDoors[i].transform;
            //Main Door/Wall object needed to get relative position to Origin
            Vector3 doorParentPos = currDoor.position;
            Transform doorPos = currDoor.GetComponent<DoorController>().doorOffset;
            
            float xRotate = 0, zRotate = 0;

            if(doorParentPos.x == transform.position.x)
            {//Door is Above or Below, Rotate Prop X 0 or 180
                if(doorParentPos.y < transform.position.y) xRotate = 180;//Instantiate(BossDoorProps, doorPos, Quaternion.identity, BossRoomOrigin.transform);
                else xRotate = 0;// Instantiate(BossDoorProps, doorPos, Quaternion.identity, BossRoomOrigin.transform);
                
            }
            if(doorParentPos.y == transform.position.y)
            {//Door is Left or Right, Rotate Prop Z 90 or -90
                if(doorParentPos.x < transform.position.x) zRotate = 90;//Instantiate(BossDoorProps, currDoorPos, Quaternion.identity, BossRoomOrigin.transform);
                else zRotate = -90; //Instantiate(BossDoorProps, currDoorPos, Quaternion.identity, BossRoomOrigin.transform);
            }
            
            Instantiate(BossDoorProps, doorPos.position,
            Quaternion.Euler(doorPos.rotation.x + xRotate, 0, doorPos.rotation.z + zRotate),
            BossRoomOrigin.transform);
        }
    }
    
    //TODO: repeat for Shops, Trials, ...
}
