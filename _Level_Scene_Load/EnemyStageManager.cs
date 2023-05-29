using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStageManager : MonoBehaviour
{
    //Attach this to Room Prefab that holds Platforms and Enemies parent objects
    [Header("References")]
    public bool neutralRoom;
    [SerializeField] Transform enemyParentObj;
    [SerializeField] int enemyCount; //number of enemies in level
    [SerializeField] private int totalEnemyCount; //used to store the original number
    public RoomClear roomManager;
    private int nextWaveCount;
    [Header("Multiple Waves Setup")]
    [SerializeField] public bool trialRoom = false;
    [SerializeField] bool multipleWaves = false;
    [SerializeField] int[] waveThreshold;
    [SerializeField] private int currentWave;
    
    [Header("Wave Debugging")]
    [SerializeField] private int currWaveEnemyCount;

    void Start()
    {
        roomManager = GetComponentInParent<RoomClear>();
        if (enemyParentObj == null) EnemySetup();
        else enemyCount = enemyParentObj.childCount;
        totalEnemyCount = enemyCount;

        if(enemyCount == 0)
        {
            RoomClear temp = GetComponentInParent<RoomClear>();
            temp.roomCleared = true;
            roomManager.Cleared();
            neutralRoom = true;
        }
        else neutralRoom = false;

        currentWave = 0;

        if(multipleWaves)
        {
            //If not trial room, start first wave
            if(!trialRoom) SpawnNextWave();
        }
        else
        {
            waveThreshold = new int[1];
            waveThreshold[0] = enemyCount;
        }
    }

    public void EnemySetup()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).CompareTag("Enemy") || transform.GetChild(i).CompareTag("Boss"))
            {
                //Only need the one Enemy parent object, break when found
                enemyParentObj = transform.GetChild(i).transform;
                break;
            }
        }
        if (enemyParentObj != null) enemyCount = enemyParentObj.childCount;

        //Toggle all enemies
        if(enemyCount > 0)
        {
            for(int i = 0; i < enemyCount; i++)
            {
                var enemyObj = enemyParentObj.GetChild(i).gameObject;
                enemyObj.SetActive(false);
                // if(enemyObj.CompareTag("Enemy")) enemyObj.SetActive(false);
            }
        }
    }

#region Spawning
    public void SpawnEnemies()
    {
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
        currWaveEnemyCount = waveThreshold[currentWave];
        int numSpawns;
        //Spawn next # of enemies
        //Compare wave index to total number of Waves
        if((currentWave - 1) < waveThreshold.Length)
        {
            numSpawns = waveThreshold[currentWave];
        }
        else numSpawns = enemyCount + 1;

        for(int i = 0; i < numSpawns; i++)
                enemyParentObj.GetChild(i).gameObject.SetActive(true);
    }

    private void SpawnWave()
    {
        //Spawn all or remaining enemies
        for(int i = 0; i < enemyCount + 1; i++)
            enemyParentObj.GetChild(i).gameObject.SetActive(true);
    }

    void CheckWaveCount()
    {
        //Check if all of the currently toggled enemies are dead
        if(!multipleWaves || enemyCount <= 0) return;

        currWaveEnemyCount--;
        if(currWaveEnemyCount <= 0)
        {
            if((currentWave + 1) > waveThreshold.Length)
                SpawnWave();
            else
            {
                currentWave++;
                SpawnNextWave();
            }
        }
    }

    private void SpawnNextWave()
    {
        if(enemyCount <= 0) return;
        //Check if index is in bounds, if not, spawn remaining enemies
        if((currentWave + 1) > waveThreshold.Length)
        {
            //Make sure everything else is spawned here
            SpawnWave();
        }
        else Invoke("SpawnCurrentWave", .5f);
    }
#endregion

    public void UpdateEnemyCount()
    {
        if (enemyCount > 0) enemyCount--;
        CheckWaveCount();
        if (enemyCount <= 0) roomManager.Cleared();
    }
}
