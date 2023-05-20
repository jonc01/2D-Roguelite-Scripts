using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomClear : MonoBehaviour
{
    public bool roomCleared; //for reference from Item Selection
    public bool trialRoom; //gets set in RoomGenerator as Trial room is created

    [Header("References")]
    public DoorManager DoorManager;
    [SerializeField] public EnemyStageManager stageManager;
    [SerializeField] private RewardController augmentReward;

    void Start()
    {
        roomCleared = false;

        if (DoorManager == null) DoorManager = GetComponent<DoorManager>();
        //if (DoorManager == null) DoorManager = GameObject.FindGameObjectWithTag("DoorManager").GetComponent<DoorManager>();
            //This only gets the number of children under "Enemies", doesn't count children's children
            //In this case, we don't want to count the raycast transforms, healthbars, etc

        augmentReward = GetComponent<RewardController>();
        if(augmentReward != null) augmentReward.ToggleRewardSelect(false);
    }

    public void Cleared()
    {
        //TimeManager.Instance.DoFreezeTime(.15f, .05f);
        Debug.Log("Room Cleared");
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
        if(stageManager == null) augmentReward.ToggleRewardSelect(false);
        else
        {
            if(!stageManager.neutralRoom && augmentReward != null) augmentReward.ToggleRewardSelect(true);
            else if(augmentReward != null) augmentReward.ToggleRewardSelect(false);
        }
        yield return new WaitForSeconds(.5f);
        DoorManager.OpenAllDoors(true);
        StartCoroutine(DelaySlowMo());
        //TimeManager.Instance.DoSlowMotion();
        roomCleared = true;
    }

    public void CheckSpawn()
    {
        //Delay added to stageManager reference get, Start() is called before Room instantiated
        if (stageManager == null) stageManager = GetComponentInChildren<EnemyStageManager>();
        if (!roomCleared) Invoke("CheckSpawnDelay", 1f);
    }

    private void CheckSpawnDelay()
    {
        if(stageManager == null) return;
        stageManager.SpawnEnemies();
    }
}
