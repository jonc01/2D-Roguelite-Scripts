using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] bool DEBUGGING = true;
    public LevelBuilder Builder;

    [Header("Shops")]
    public int numShopsLower = 2;
    [SerializeField] int numShopsUpper = 3;

    [Header("Trials")]
    public int numTrialsLower = 1;
    [SerializeField] int numTrialsUpper = 1;

    [Space(10)]
    [Header("--- Generator Components (*Setup) ---")]
    [SerializeField] GameObject BossRoom;
    [Header("Generator Component Arrays")]
    [SerializeField] GameObject[] StartRooms;
    [SerializeField] GameObject[] Rooms; //Platforms and Room Collider
    [SerializeField] GameObject[] Shops;
    [SerializeField] GameObject[] Trials;

    [Header("--- Generation Results ---")]
    [SerializeField] List<int> availableIndexes;

    [Header("Variables")]
    //[SerializeField] bool shopAdded; //Shop Types: General, Attack Items, Defense Items, Healing Items

    public bool roomGenRunning;
    public bool roomGenDone;
    private RoomDoorGenerator roomDoorGen;

    private void Start()
    {
        Builder = GetComponent<LevelBuilder>();
        roomGenDone = false;

        availableIndexes = new List<int>();
        //shopAdded = false; //Example
        roomDoorGen = GetComponentInChildren<RoomDoorGenerator>();
    }

    void Update()
    {
        if (DEBUGGING)
        {
            if (Input.GetKeyDown(KeyCode.I)) GenerateRooms();
        }
    }

    public void GenerateRooms()
    {
        Debug.Log("Generating Rooms...");
        StartCoroutine(GenerateRoomsCO());
    }

    IEnumerator GenerateRoomsCO()
    {
        roomGenDone = false;
        roomGenRunning = true;

        //Assigning origin indexes to specific rooms
        int totalRooms = Builder.totalRooms;
        int bossRoomIndex = Builder.GeneratedOrigins.Length - 1;

        //List Setup, Keep track of available indexes
        //availableIndexes = new List<int>();
        for (int i = 0; i < totalRooms; i++)
        {
            availableIndexes.Add(i);
        }

        //Reserve first and last index for Starting and Boss room
        CreateRoom(StartRooms[0], 0); //TODO: randomize StartRooms if multiple
        CreateRoom(BossRoom, bossRoomIndex);

        

        //Create Shops
        int totalShops = Random.Range(numShopsLower, numShopsUpper + 1);
        for (int i = 0; i < totalShops; i++)
        {
            int randShop = Random.Range(0, totalShops); //Pick random shop from array of variations
            int randRoomIndex = Random.Range(0, availableIndexes.Count);
            CreateRoom(Shops[randShop], availableIndexes[randRoomIndex]);
        }

        yield return new WaitForSecondsRealtime(.01f);

        //Create Trial(s)
        int totalTrials = Random.Range(numTrialsLower, numTrialsUpper + 1);
        for (int i = 0; i < totalTrials; i++) //Reserve indexes for Trials
        {
            int randTrial = Random.Range(0, totalTrials);
            int randRoomIndex = Random.Range(0, availableIndexes.Count);
            CreateRoom(Trials[randTrial], availableIndexes[randRoomIndex]);
        }

        yield return new WaitForSecondsRealtime(.01f);

        //Create normal rooms in remaining origins
        //Clear out list as we go
        while (availableIndexes.Count > 0)
        {
            //int i = availableIndexes[0];
            //yield return new WaitForSecondsRealtime(.01f); //0.001

            int rand = Random.Range(0, Rooms.Length);

            CreateRoom(Rooms[rand], availableIndexes[0]);
        }
        roomGenRunning = false;
        roomGenDone = true;

        roomDoorGen.AddBorderingDoors();

        Debug.Log("Rooms Generated");
    }

    private void CreateRoom(GameObject roomObj, int roomIndex)
    {
        //Instantiate GameObject at index transform, remove that index from the list
        Transform currRoom = Builder.GeneratedOrigins[roomIndex].transform;
        Instantiate(roomObj, currRoom.position, Quaternion.identity, currRoom);
        availableIndexes.Remove(roomIndex);
    }
}
