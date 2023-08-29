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
    [SerializeField] GameObject[] RoomGuarantees;
    [SerializeField] GameObject[] Shops;
    [SerializeField] GameObject[] Trials;

    [Header("--- Generation Results ---")]
    [SerializeField] List<int> availableIndexes;

    [Header("Variables")]
    [SerializeField] List<int> shuffledRoomsIdx;
    [SerializeField] List<int> generatedRoomsDEBUG;

    public bool roomGenRunning;
    public bool roomGenDone;

    private void Start()
    {
        Builder = GetComponent<LevelBuilder>();
        roomGenDone = false;

        availableIndexes = new List<int>();
        shuffledRoomsIdx = new List<int>();

        generatedRoomsDEBUG = new List<int>();

        FillShuffleList();
        //shopAdded = false; //Example
    }

    void Update()
    {
        if (DEBUGGING)
        {
            if (Input.GetKeyDown(KeyCode.I)) GenerateRooms();
        }
    }

#region Room Generation
    public void GenerateRooms()
    {
        // Debug.Log("Generating Rooms...");
        StartCoroutine(GenerateRoomsCO());
    }

    IEnumerator GenerateRoomsCO()
    {
        generatedRoomsDEBUG.Clear();

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
        int totalShops = Random.Range(numShopsLower, numShopsUpper + 1); //int Random.Range is not max inclusive
        for (int i = 0; i < totalShops; i++)
        {
            int randRoomIndex = Random.Range(0, availableIndexes.Count); //int no max inclusive, so this works for index values

            //Add Guaranteed rooms first before adding random rooms
            if(i < RoomGuarantees.Length)
            {
                CreateRoom(RoomGuarantees[i], availableIndexes[randRoomIndex]);
            }
            else{
                int randShop = Random.Range(0, Shops.Length); //Pick random shop from array of variations
                CreateRoom(Shops[randShop], availableIndexes[randRoomIndex]);
            }
        }

        yield return new WaitForSecondsRealtime(.01f);

        //Create Trial(s)
        int totalTrials = Random.Range(numTrialsLower, numTrialsUpper + 1);
        for (int i = 0; i < totalTrials; i++) //Reserve indexes for Trials
        {
            int randTrial = Random.Range(0, Trials.Length);
            int randRoomIndex = Random.Range(0, availableIndexes.Count);
            //Setting trialRoom variable in RoomClear
            int roomIndex = availableIndexes[randRoomIndex];
            Builder.GeneratedOrigins[roomIndex].GetComponent<RoomClear>().trialRoom = true;
            CreateRoom(Trials[randTrial], roomIndex);
        }

        yield return new WaitForSecondsRealtime(.01f);

        //Create normal rooms in remaining origins
        //Clear out list as we go
        while (availableIndexes.Count > 0)
        {
            //int i = availableIndexes[0];
            //yield return new WaitForSecondsRealtime(.01f); //0.001

            // int randIdx = Random.Range(0, Rooms.Length); 

            int randIdx = GetShuffledIdx();
            CreateRoom(Rooms[randIdx], availableIndexes[0]);
        }
        roomGenRunning = false;
        roomGenDone = true;

        SortGeneratedDEBUG();
        // Debug.Log("Rooms Generated");
    }
#endregion

    private void CreateRoom(GameObject roomObj, int roomIndex)
    {
        //Instantiate GameObject at index transform, remove that index from the list
        Transform currRoom = Builder.GeneratedOrigins[roomIndex].transform;
        Instantiate(roomObj, currRoom.position, Quaternion.identity, currRoom);
        availableIndexes.Remove(roomIndex);
    }

#region Shop Shuffle

    void FillShuffleList()
    {
        shuffledRoomsIdx.Clear();
        for(int i=0; i<Rooms.Length; i++)
        {
            shuffledRoomsIdx.Add(i);
        }
    }

    private int GetShuffledIdx()
    {
        if(shuffledRoomsIdx.Count <= 0)
        {
            FillShuffleList();
        }

        //Get random shuffled index
        int randShuffledIdx = Random.Range(0, shuffledRoomsIdx.Count);
        //Store shuffled index value as Rooms idx
        int roomIdx = shuffledRoomsIdx[randShuffledIdx];
        //Remove from list to prevent duplicates until all rooms have been used
        shuffledRoomsIdx.Remove(roomIdx);

        generatedRoomsDEBUG.Add(roomIdx);
        return roomIdx;
    }

    void SortGeneratedDEBUG()
    {
        generatedRoomsDEBUG.Sort();
    }

#endregion
}
