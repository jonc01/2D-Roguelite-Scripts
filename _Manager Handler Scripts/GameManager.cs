using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Space(10)]
    public bool inputAllowed;
    public bool shopOpen;
    public bool rewardOpen;
    public PauseMenu Pause;

    [Space(10)]
    [Header("- Player References -")]
    public Transform playerTransform;
    public Base_PlayerMovement PlayerMovement;
    public Base_PlayerCombat PlayerCombat;
    public Transform playerTargetOffset;
    public int PlayerCurrPlatform;

    [Space(20)]
    [Header("- Augments -")]
    public AugmentInventory AugmentInventory;
    public Inventory Inventory;
    public AugmentPool AugmentPool;
    public int normalRoomClearCount;
    public int normalRoomClearRewardLimit = 3;
    [Header("- Debugging -")]
    public int roomAugmentRewardsGiven; //Total augments the player has received from Normal room rewards
    // public int[] rewardRoomCounts;
    [Space(10)]
    [Header("- Boss Unlock -")]
    public int totalTrialsCleared;
    public int totalTrialsNeeded = 2;
    [SerializeField] private List<DoorController> bossDoors;

    private void Awake()
    {
        Instance = this;

        if (Pause == null) Pause = GameObject.Find("Menu UI Canvas").GetComponent<PauseMenu>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerMovement = playerTransform.GetComponent<Base_PlayerMovement>();
        PlayerCombat = playerTransform.GetComponent<Base_PlayerCombat>();
        playerTargetOffset = GameObject.FindGameObjectWithTag("PlayerTargetOffset").transform;
        if (Inventory == null) Inventory = GetComponent<Inventory>();

        shopOpen = false;
        rewardOpen = false;
        inputAllowed = true;
        normalRoomClearCount = 0;
        roomAugmentRewardsGiven = 0;

        totalTrialsCleared = 0;
        bossDoors = new List<DoorController>();
    }

    public void RestartLevelCount()
    {
        //Reset room counters and level specific references
        normalRoomClearCount = 0;
        roomAugmentRewardsGiven = 0;
        totalTrialsCleared = 0;
        bossDoors = new List<DoorController>();
    }

    private void Update()
    {
        if(PlayerMovement.currPlatform != PlayerCurrPlatform)
        {
            PlayerCurrPlatform = PlayerMovement.currPlatform;
        }
    }

    public void MovePlayer(Vector3 newPos)
    {
        playerTransform.position = newPos;
    }

    public void TogglePlayerInput(bool toggle)
    {
        inputAllowed = toggle; //Referenced by Shop and Pause menus
        PlayerMovement.allowInput = toggle; //Direct set in Player scripts
        PlayerCombat.allowInput = toggle;
    }

    public bool CheckPlayerAugmentReward()
    {
        //Give player augments after clearing a certain number of normal rooms (not including Trials, Boss, etc)
        //Reached Normal reward count limit
        if(roomAugmentRewardsGiven >= normalRoomClearRewardLimit) return false;

        //Checking if the number of normal room clears has reached the miletones
        if(normalRoomClearCount == 1 || normalRoomClearCount == 3 || normalRoomClearCount == 5)
        {
            return true;
        }
        else return false; //Player hasn't reached any threshold yet
    }

    public bool CheckBossUnlock()
    {
        // public int totalTrialsCleared;
        // public int totalTrialsNeeded = 2;

        if(totalTrialsCleared >= totalTrialsNeeded)
        {
            return true;
        }
        else return false;
    }

    public void UnlockBossDoor()
    {
        for(int i=0; i<bossDoors.Count; i++)
        {
            bossDoors[i].DisplayLockIcons(true);
        }

        if(!CheckBossUnlock()) return;
        for(int i=0; i<bossDoors.Count; i++)
        {
            bossDoors[i].ToggleDoor(true);
            bossDoors[i].DisplayLockIcons(false);
        }
    }

    public void AddBossDoor(DoorController door)
    {
        bossDoors.Add(door);
    }
}
