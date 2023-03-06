using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPoolerHelper : MonoBehaviour
{
    //! - Attached to a Prefab, will move to back to the pool once the object is disabled
    //Object Disable needs to be handled in the Prefab's script
    [Header("References")]
    [SerializeField] ObjectPoolerList pool;
    [SerializeField] float poolDelay = 1f;

    private void Start()
    {
        pool = GetComponentInParent<ObjectPoolerList>();
        if(pool != null) pool.ReturnObject(gameObject);
        Debug.Log("Getting pool list...");
    }

    private void OnEnable()
    {
        // if(pool == null) pool = GetComponentInParent<ObjectPoolerList>();
        StartCoroutine(PoolObject(poolDelay));
    }

    private void OnDisable()
    {
        // if(pool != null) pool.ReturnObject(gameObject);
    }

    IEnumerator PoolObject(float duration)
    {
        if(pool == null) pool = GetComponentInParent<ObjectPoolerList>();
        yield return new WaitForSeconds(duration);
        pool.ReturnObject(gameObject);
    }
}
