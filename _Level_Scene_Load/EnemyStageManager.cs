using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyStageManager : MonoBehaviour
{
    //Attach this to Room Prefab that holds Platforms and Enemies parent objects
    [Header("References")]
    public GameObject minimapIcon;
    [SerializeField] Transform enemyParentObj;
    // [SerializeField] int enemyCount; //number of enemies in level
    [SerializeField] int totalWaves;
    public RoomClear roomManager;
    [SerializeField] EnemyWaveManager[] waveManagers;
    
    [Space(20)]
    [Header("= SETUP =")]
    public bool isStartingRoom = false; //Manually set this in room Prefab
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
    }

    public void ToggleMinimapIcon(bool toggle)
    {
        if(minimapIcon == null) return;
        minimapIcon.SetActive(toggle);
    }

#region Spawning
    public void SpawnEnemies()
    {
        Debug.Log("Player entered room " + name);
        //Called by default, when Player enters room
        if(trialRoom) return;
        SpawnCurrentWave();
    }

    public void StartTrial()
    {
        //Separate call for Trials, manually called with interact
        if(!trialRoom) return;
        SpawnCurrentWave();
    }

    private void SpawnCurrentWave()
    {
        currentWaveIdx = 0;
        SpawnWave();
        
        //Boss is already enabled, manually starting spawn
        // if(bossRoom) 
        SpawnWave();
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



































//========================================================================
//========================================================================
//========================================================================
//========================================================================
//========================================================================
//========================================================================

    // public void EnemySetup1()
    // {
    //     // for(int i = 0; i < transform.childCount; i++)
    //     // {
    //     //     if(transform.GetChild(i).CompareTag("Enemy") || transform.GetChild(i).CompareTag("Boss"))
    //     //     {
    //     //         //Only need the one Enemy parent object, break when found
    //     //         enemyParentObj = transform.GetChild(i).transform;
    //     //         break;
    //     //     }
    //     // }
    //     // if (enemyParentObj != null) enemyCount = enemyParentObj.childCount;

    //     // //Toggle all enemies
    //     // if(enemyCount > 0)
    //     // {
    //     //     for(int i = 0; i < enemyCount; i++)
    //     //     {
    //     //         var enemyObj = enemyParentObj.GetChild(i).gameObject;
    //     //         enemyObj.SetActive(false);
    //     //         // if(enemyObj.CompareTag("Enemy")) enemyObj.SetActive(false);
    //     //     }
    //     // }

    //     // if(bossRoom)
    //     // {
    //     //     var bossObj = enemyParentObj.GetChild(0);
    //     //     bossObj.gameObject.SetActive(true);
    //     // } 
    // }

    private void SpawnWave1()
    {
        //Spawn all or remaining enemies
        // for(int i = 0; i < enemyCount + 1; i++)
        //     enemyParentObj.GetChild(i).gameObject.SetActive(true);
    }

    private void SpawnCurrentWave1()
    {
        // currWaveEnemyCount = waveThreshold[currentWave];
        int numSpawns;
        //Spawn next # of enemies
        //Compare wave index to total number of Waves
        // if((currentWave - 1) < waveThreshold.Length)
        // {
        //     numSpawns = waveThreshold[currentWave];
        // }
        // else numSpawns = enemyCount + 1;

        // for(int i = 0; i < numSpawns; i++)
        // {
        //     enemyParentObj.GetChild(i).gameObject.SetActive(true);
        // }
        
        //Boss is already enabled, manually starting spawn
        if(bossRoom)
            enemyParentObj.GetChild(0).GetComponent<Base_BossCombat>().StartSpawn();
    }

    // private void SpawnNextWave()
    // {
    //     if(enemyCount <= 0) return;
    //     //Check if index is in bounds, if not, spawn remaining enemies
    //     if((currentWave + 1) > waveThreshold.Length)
    //     {
    //         //Make sure everything else is spawned here
    //         SpawnWave();
    //     }
    //     else Invoke("SpawnCurrentWave", .5f);
    // }
    
    // public void UpdateEnemyCount() //TODO: remove
    // {
    //     // if (enemyCount > 0) enemyCount--;
    //     // // CheckWaveCount();
    //     // if (enemyCount <= 0)
    //     // {
    //     //     roomManager.Cleared();
    //     //     SpawnClearRewards();
    //     // }
    // }
}
