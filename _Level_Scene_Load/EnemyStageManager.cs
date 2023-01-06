using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStageManager : MonoBehaviour
{
    //Attach this to Room Prefab that holds Platforms and Enemies parent objects
    [Header("References")]
    [SerializeField] int enemyCount; //number of enemies in level
    [SerializeField] Transform enemyParentObj;
    RoomClear roomManager;

    void Start()
    {
        if (enemyParentObj == null) EnemySetup();
        else enemyCount = enemyParentObj.childCount;

        roomManager = GetComponentInParent<RoomClear>();
    }

    public void EnemySetup()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).CompareTag("Enemy"))
            {
                enemyParentObj = transform.GetChild(i).transform;
                break;
            }
        }
        if (enemyParentObj != null) enemyCount = enemyParentObj.childCount;
    }

    public void UpdateEnemyCount()
    {
        if (enemyCount > 0) enemyCount--;
        if (enemyCount <= 0) roomManager.Cleared();
    }
}
