using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    [SerializeField] bool waiting = false;


    public void Stop(float duration, float timeScale)
    {
        if (waiting) return;
        Time.timeScale = timeScale;
        StartCoroutine(Wait(duration));
    }

    public void Stop(float duration)
    {
        Stop(duration, 0.5f);
    }

    IEnumerator Wait(float duration)
    {
        waiting = true;
        // yield return new WaitForSecondsRealtime(duration);
        yield return new WaitForSeconds(duration);
        Time.timeScale = 1.0f;
        waiting = false;
    }
}
