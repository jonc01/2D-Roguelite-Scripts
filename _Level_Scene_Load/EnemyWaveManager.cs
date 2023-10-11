using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    [Header("- References -")]
    [SerializeField] EnemyStageManager enemyStageManager;

    [Header("- Variables -")]
    [SerializeField] public int enemyCount;
    [SerializeField] private bool bossRoom;

    private void Awake()
    {
        if(enemyStageManager == null) enemyStageManager = transform.parent.GetComponent<EnemyStageManager>();
        //Get all enemy child objects
        enemyCount = transform.childCount;

        bossRoom = enemyStageManager.bossRoom;
    }

    private void Start()
    {
        enemyCount = transform.childCount;

        if(!bossRoom)
        {
            //Toggle all enemies
            if(enemyCount > 0)
            {
                for(int i = 0; i < enemyCount; i++)
                {
                    var enemyObj = transform.GetChild(i).gameObject;
                    enemyObj.SetActive(false);
                }
            }
        }
        else
        {
            //do nothing
            // var bossObj = transform.GetChild(0);
            // bossObj.gameObject.SetActive(true);
            // bossObj.GetComponent<Base_BossCombat>().StartSpawn();
        }
    }

    public void SpawnEnemies()
    {
        if(bossRoom)
        {
            var bossObj = transform.GetChild(0);
            bossObj.GetComponent<Base_BossCombat>().StartSpawn();
        }
        else
        {
            //Spawn in current Wave
            for(int i = 0; i < enemyCount; i++)
            {
                var enemyObj = transform.GetChild(i).gameObject;
                enemyObj.SetActive(true);
            }
        }
    }

    public void UpdateEnemyCount()
    {
        if (enemyCount > 0) enemyCount--;

        if (enemyCount <= 0)
        {
            enemyStageManager.SpawnNextWave();
        }
    }
}
