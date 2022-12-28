using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectsHandler : MonoBehaviour
{
    // Enables objects stored in pools
    // ObjectPoolerList will enable objects if available
    //  otherwise Instantiates a new object
    [Header("Pooled Objects")]
    [SerializeField]
    private ObjectPoolerList HitEffectsPool;

    [Header("Kill Effects")]
    [SerializeField] GameObject[] KillEffectPrefabs;

    public void ShowHitEffect(Vector3 pos)
    {
        GameObject showHit = HitEffectsPool.GetObject();
        showHit.transform.position = pos;
        showHit.transform.rotation = Quaternion.identity;
        showHit.SetActive(true);
    }

    public void ShowKillEffect(Vector3 pos, int index = 0)
    {
        if (KillEffectPrefabs.Length == 0) return;
        Instantiate(KillEffectPrefabs[index], pos, Quaternion.identity);
    }
}