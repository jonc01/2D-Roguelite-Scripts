using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStageManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] int enemyCount; //number of enemies in level
    [SerializeField] StageClear stageClear;

    void Start()
    {
        enemyCount = transform.childCount;
        stageClear = GetComponentInParent<StageClear>();
    }

    public void UpdateEnemyCount()
    {
        if (enemyCount > 0) enemyCount--;

        if (enemyCount <= 0)
        {
            stageClear.Cleared();
        }
    }
}
