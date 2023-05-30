using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObjHandler : MonoBehaviour
{
    // Enables objects stored in pools
    // ObjectPoolerList will enable objects if available
    //  otherwise Instantiates a new object
    [Header("Pooled Objects")]
    [SerializeField]
    private ObjectPoolerList IndicatorPool;

    private void Start()
    {
        if (IndicatorPool == null) IndicatorPool = GetComponent<ObjectPoolerList>();
    }

    public void PlayAnim(Vector3 pos)
    {
        GameObject effect = IndicatorPool.GetObject();
        effect.transform.position = pos;
        //Set rotation
        effect.transform.rotation = Quaternion.identity;
        effect.SetActive(true);
    }

    public void PlayAttachedAnim(Vector3 pos, float scale = 2.5f)
    {
        GameObject effect = IndicatorPool.GetObject();
        // effect.transform.SetParent(parentObj);
        effect.transform.position = pos;
        //Setting parent and scale
        effect.transform.localScale = new Vector3(scale, scale, 1);

        effect.SetActive(true);
    }
}
