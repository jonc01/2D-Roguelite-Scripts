using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    //Keep this object disabled by default, SetActive(true) with BossPhaseController or something else.
    //Play GameObject as a child of this object, when this object is enabled, it will play an animation
    //then enable the child object.
    [SerializeField] public Animator animator;
    [SerializeField] private float timeDelay = 1.0f; //delay before enabling object
    [SerializeField] private string animationName = "StartSpawn";
    [SerializeField] private GameObject EnemyObject;

    // Start is called before the first frame update
    void Start()
    {
        if (EnemyObject == null)
            EnemyObject = this.gameObject.transform.GetChild(0).gameObject;

        if (EnemyObject != null)
            EnemyObject.SetActive(false);

        if (animator == null)
            animator = this.gameObject.GetComponent<Animator>();

        StartCoroutine(DelaySpawn());
    }

    IEnumerator DelaySpawn()
    {
        animator.SetTrigger(animationName);
        yield return new WaitForSeconds(timeDelay);

        if (EnemyObject != null)
            EnemyObject.SetActive(true);
    }
}
