using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomClear : MonoBehaviour
{
    public bool roomCleared; //for reference from Item Selection

    [Header("References")]
    public DoorManager DoorManager;

    void Start()
    {
        roomCleared = false;
        if (DoorManager == null) DoorManager = GetComponent<DoorManager>();
        //if (DoorManager == null) DoorManager = GameObject.FindGameObjectWithTag("DoorManager").GetComponent<DoorManager>();
            //This only gets the number of children under "Enemies", doesn't count children's children
            //In this case, we don't want to count the raycast transforms, healthbars, etc
    }

    public void Cleared()
    {
        //TimeManager.Instance.DoFreezeTime(.15f, .05f);
        StartCoroutine(DelayClear());
        //if this breaks, update enemyCount with enemyCount = EnemyList.transform.childCount
    }

    IEnumerator DelaySlowMo()
    {
        yield return new WaitForSeconds(0.1f);
        //TimeManager.Instance.DoSlowMotion();
    }

    IEnumerator DelayClear()
    {
        yield return new WaitForSeconds(1f);
        DoorManager.ToggleAllDoors(true);
        StartCoroutine(DelaySlowMo());
        //TimeManager.Instance.DoSlowMotion();
        roomCleared = true;
    }
}
