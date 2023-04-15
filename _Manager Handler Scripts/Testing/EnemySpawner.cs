using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Reference Setup")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Transform spawnOffset;
    [SerializeField] Animator anim;


    void Start()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    public void SpawnEnemy()
    {
        Instantiate(enemyPrefab, spawnOffset.position, Quaternion.identity);
    }
}
