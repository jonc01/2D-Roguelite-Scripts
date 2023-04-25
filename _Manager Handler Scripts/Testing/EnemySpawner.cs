using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Reference Setup")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Transform spawnOffset;
    [SerializeField] Animator anim;

    [Header("Custom Variables")]
    [SerializeField] int totalSpawns = 1;

    void Start()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    public void SpawnEnemy()
    {
        for(int i=0; i<totalSpawns; i++)
        {
            Instantiate(enemyPrefab, spawnOffset.position, Quaternion.identity);
        }
    }
}
