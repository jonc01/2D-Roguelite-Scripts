using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject[] IndicatorPrefab;
    [SerializeField] private float[] indicatorAnimTime;
    //defaults: .5167f, .5f

    public void ChargeUp(Vector3 position, Transform parent, int index = 0)
    {
        if (IndicatorPrefab == null) return;
        GameObject prefab = Instantiate(IndicatorPrefab[index], position, Quaternion.identity, parent);
        StartCoroutine(DeleteObject(prefab, index));
    }

    IEnumerator DeleteObject(GameObject prefab, int index = 0)
    {
        yield return new WaitForSeconds(indicatorAnimTime[index]);
        Destroy(prefab);
    }
}
