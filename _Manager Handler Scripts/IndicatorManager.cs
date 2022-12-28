using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject IndicatorPrefab;
    [SerializeField] private float indicatorAnimTime;
    

    public void ChargeUp(Vector3 position, Transform parent)
    {
        if (IndicatorPrefab == null) return;
        GameObject prefab = Instantiate(IndicatorPrefab, position, Quaternion.identity, parent);
        StartCoroutine(DeleteObject(prefab));
    }

    IEnumerator DeleteObject(GameObject prefab)
    {
        yield return new WaitForSeconds(indicatorAnimTime);
        Destroy(prefab);
    }
}
