using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStageManager : MonoBehaviour
{
    //Attach this to Room Prefab that holds Platforms and Enemies parent objects
    [Header("References")]
    //public bool neutralRoom = false;
    [SerializeField] Transform enemyParentObj;
    [SerializeField] int enemyCount; //number of enemies in level
    [SerializeField] private int totalEnemyCount; //used to store the original number
    private int nextWaveCount;
    [Header("Multiple Waves Setup")]
    [SerializeField] public bool trialRoom = false;
    [SerializeField] bool multipleWaves = false;
    [SerializeField] int[] waveThreshold;
    [SerializeField] private int currentWave;
    
    public RoomClear roomManager;

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
        }

        // if(trialRoom) roomManager.DoorManager.ToggleAllDoors(true);

        currentWave = 0;

        if(!multipleWaves)
        {
            waveThreshold = new int[1];
            waveThreshold[0] = enemyCount;
        }
    }

    public void EnemySetup()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).CompareTag("Enemy"))
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
                enemyParentObj.GetChild(i).gameObject.SetActive(false);
        }
    }

#region Spawning
    public void SpawnEnemies()
    {
        if(trialRoom) return;
        SpawnWave(currentWave);
    }

    public void StartTrial()
    {
        //Separate call for Trials
        if(!trialRoom) return;
        SpawnWave(currentWave);
    }

    private void SpawnWave(int wave)
    {
        //Spawn next # of enemies
        //TODO: this doesn't properly spawn all enemies in the second wave
        //needs fixing if using more than 2 waves (waveThreshold[1] not 0)
        for(int i = 0; i < waveThreshold[currentWave]; i++)
            enemyParentObj.GetChild(i).gameObject.SetActive(true);
    }

    private void SpawnWave()
    {
        //Spawn all remaining enemies
        for(int i = 0; i < enemyCount + 1; i++)
            enemyParentObj.GetChild(i).gameObject.SetActive(true);
    }

    void UpdateWaveCount()
    {
        //Check if all of the currently toggled enemies are dead
        if(!multipleWaves || (currentWave + 1 > waveThreshold.Length)) return;

        if(enemyCount <= (totalEnemyCount - waveThreshold[currentWave]))
        {
            //Spawn next wave based on number, spawn next number of enemies
            totalEnemyCount -= waveThreshold[currentWave];
            currentWave++;
            //nextWaveCount = enemyCount - waveThreshold[currentWave];
            if(currentWave + 1 <= waveThreshold.Length) //Check that this isn't the last wave
                SpawnWave(currentWave);
            else
                SpawnWave();
        }
    }
#endregion

    public void UpdateEnemyCount()
    {
        if (enemyCount > 0) enemyCount--;
        UpdateWaveCount();
        if (enemyCount <= 0) roomManager.Cleared();
    }
}
