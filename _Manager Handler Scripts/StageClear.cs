using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageClear : MonoBehaviour
{
    public bool levelCleared; //for reference from Item Selection

    [Header("References")]
    public GameObject ArrowIndicator;
    public GameObject EndPortal; //opens portal to move player to next stage
    public DoorManager DoorManager;

    void Start()
    {
        levelCleared = false;
        EndPortal.SetActive(false);

        if (ArrowIndicator == null) ArrowIndicator = GameObject.Find("ArrowIndicatorCanvas");
        if (ArrowIndicator != null) ArrowIndicator.SetActive(false);

        DoorManager = GameObject.FindGameObjectWithTag("DoorManager").GetComponent<DoorManager>();
            //This only gets the number of children under "Enemies", doesn't count children's children
            //In this case, we don't want to count the raycast transforms, healthbars, etc
    }

    public void Cleared()
    {
        //TimeManager.Instance.DoFreezeTime(.15f, .05f);

        StartCoroutine(DelayClear());
        /*DoorManager.OpenDoors();
        StartCoroutine(DelaySlowMo());
        //TimeManager.Instance.DoSlowMotion();
        levelCleared = true;
        EndPortal.SetActive(true);
        if(ArrowIndicator != null) ArrowIndicator.SetActive(true);*/

        
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
        DoorManager.OpenDoors();
        StartCoroutine(DelaySlowMo());
        //TimeManager.Instance.DoSlowMotion();
        levelCleared = true;
        EndPortal.SetActive(true);
        if (ArrowIndicator != null) ArrowIndicator.SetActive(true);
    }
}
