using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyStageManager : MonoBehaviour
{
    //Attach this to Room Prefab that holds Platforms and Enemies parent objects
    [Header("References")]
    public SpriteRenderer minimapIcon; //Only for Shops, Trials, Boss rooms with specific icons
    [SerializeField] string lockedAnimName;
    [SerializeField] string unlockedAnimName;
    [SerializeField] int hashedLockedAnim;
    [SerializeField] int hashedUnlockedAnim;
    [SerializeField] Animator anim;
    [Space(5)]
    [SerializeField] Transform enemyParentObj;
    // [SerializeField] int enemyCount; //number of enemies in level
    [SerializeField] int totalWaves;
    public RoomClear roomManager;
    [SerializeField] EnemyWaveManager[] waveManagers;
    
    [Space(20)]
    [Header("= SETUP =")]
    public bool isStartingRoom = false; //Manually set this in room Prefab
    public bool normalRoom; //Allows room to count towards augment reward from clears
    public bool neutralRoom;
    public bool hasAugmentRewards = false;
    public bool bossRoom = false;
    [Space(20)]
    [Header("Multiple Waves Setup")]
    [SerializeField] public bool trialRoom = false;
    // [SerializeField] bool multipleWaves = false;
    // [SerializeField] int[] waveThreshold;
    [SerializeField] private int currentWaveIdx;
    
    // [Header("Wave Debugging")]
    // [SerializeField] private int currWaveEnemyCount;
    // private DoorManager doorManager;

    [Space(10)]
    [Header("- Clear Rewards -")]
    [SerializeField] GameObject[] clearRewardPrefabs;
    [SerializeField] int[] clearRewardsQuantity;
    [SerializeField] Transform rewardSpawnPoint;
    [Space(5)]
    [Header("- Clear Platforms -")]
    [SerializeField] GameObject platformsParent;

    void Start()
    {
        roomManager = GetComponentInParent<RoomClear>();
        if(roomManager != null) roomManager.stageManager = this;

        totalWaves = waveManagers.Length;

        if(waveManagers.Length == 0)
        {
            RoomClear temp = GetComponentInParent<RoomClear>();
            temp.roomCleared = true;
            roomManager.Cleared();
            neutralRoom = true;
            // temp.DoorManager.startingRoom = isStartingRoom;
        }
        else neutralRoom = false;
        currentWaveIdx = 0;

        if(!isStartingRoom) ToggleMinimapIcon(false);

        if(bossRoom)
        {
            ToggleBossUnlockIcon(false);
            GameManager.Instance.AddBossStage(this);
        }
    }

    public void ToggleMinimapIcon(bool toggle)
    {
        if(minimapIcon == null) return;
        minimapIcon.gameObject.SetActive(toggle);
    }

    public void ToggleBossUnlockIcon(bool toggle)
    {
        if(!bossRoom) return;
        
        if(toggle) anim.Play(hashedUnlockedAnim);
        else anim.Play(hashedLockedAnim);
    }

#region Spawning
    public void SpawnEnemies()
    {
        // Debug.Log("Player entered room " + name);
        //Called by default, when Player enters room
        if(trialRoom) return;
        SpawnCurrentWave();
    }

    public void StartTrial()
    {
        //Separate call for Trials, manually called when Player interacts with statue
        if(!trialRoom) return;
        SpawnCurrentWave();
    }

    private void SpawnCurrentWave()
    {
        currentWaveIdx = 0;
        SpawnWave();
        
        //Boss is already enabled, manually starting spawn
        // if(bossRoom)
        // SpawnWave();
    }

    private void SpawnWave()
    {
        //Spawn current wave
        // waveManagerObj[currentWave].SetActive(true);
        waveManagers[currentWaveIdx].SpawnEnemies();
    }

    public void SpawnNextWave()
    {
        currentWaveIdx++;
        
        if(currentWaveIdx <= totalWaves - 1)
        {
            SpawnWave();
        }
        else
        {
            SpawnClearRewards();
            roomManager.Cleared();
        }
    }
#endregion

    private void SpawnClearRewards()
    {
        Invoke("DelayPlatformSpawn", 1.2f);
        if(clearRewardPrefabs.Length == 0) return;
        if(rewardSpawnPoint == null) rewardSpawnPoint = transform;

        for(int i=0; i<clearRewardPrefabs.Length; i++)
        {
            for(int j=0; j<clearRewardsQuantity[i]; j++)
            {
                Instantiate(clearRewardPrefabs[i], rewardSpawnPoint.position, Quaternion.identity);
            }
        }
    }

    private void DelayPlatformSpawn()
    {
        if(platformsParent != null)
        {
            platformsParent.SetActive(true);
        }
    }
}
