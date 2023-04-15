using UnityEngine;

public class PrefabHandler : MonoBehaviour
{
    // Enables objects stored in pools
    // ObjectPoolerList will enable objects if available
    //  otherwise Instantiates a new object
    [Header("Pooled Objects")]
    [SerializeField]
    private ObjectPoolerList PrefabPool;
    [SerializeField] float baseScale = 2;

    private void Start()
    {
        if (PrefabPool == null) PrefabPool = GetComponent<ObjectPoolerList>();
    }

    public void SpawnPrefab(Vector3 pos)
    {
        GameObject prefab = PrefabPool.GetObject();
        prefab.transform.position = pos;
        prefab.transform.rotation = Quaternion.identity;
        prefab.SetActive(true);
    }

    // public void SpawnPrefab(Vector3 pos, bool scaleOverride, float scale)
    // {
        //TODO: if using this, need to scale hitbox as well
    //     GameObject prefab = PrefabPool.GetObject();
    //     prefab.transform.position = pos;

    //     Vector3 prefabScale = new Vector3(baseScale, baseScale, 1);
    //     if(scaleOverride) prefab.transform.localScale = new Vector3(scale, scale, 1);
        
    //     prefab.transform.localScale = prefabScale;
    //     prefab.transform.rotation = Quaternion.identity;

    //     prefab.SetActive(true);
    // }

    public void SpawnPrefab(Vector3 pos, float yRotation)
    {
        GameObject prefab = PrefabPool.GetObject();
        prefab.transform.position = pos;
        prefab.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        prefab.SetActive(true);
    }
}
