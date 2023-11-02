using System;
using System.Collections;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    //Not using, moved directly to enemy object to keep it attached

    [Header("References")]
    [SerializeField] PooledObjHandler[] Prefabs;
    //[SerializeField] private float[] indicatorAnimTime;
    //defaults: .5167f, .5f

    public void PlayIndicator(Vector3 position, int index = 0, float scale = 2.5f)
    {
        if (Prefabs.Length == 0) return;
        Prefabs[index].PlayAttachedAnim(position, scale);
    }

    public void PlayIndicator(Vector3 position, int index = 0)
    {
        if (Prefabs.Length == 0) return;
        Prefabs[index].PlayAttachedAnim(position, 1);
    }

    public void PlayIndicator(Vector3 position, int index = 0, float scaleX = 2f, float scaleY = 2f)
    {
        if (Prefabs.Length == 0) return;
        Prefabs[index].PlayAttachedAnimWide(position, scaleX, scaleY);
    }
}
