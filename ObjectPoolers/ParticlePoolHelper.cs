using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePoolHelper : MonoBehaviour
{
    [SerializeField]
    private ObjectPoolerList pool; //Don't need to manual set

    void Start()
    {
        pool = transform.parent.GetComponent<ObjectPoolerList>();
    }

    // private void OnParticleSystemStopped() //! - not working 2022.3
    // {
    //     pool.ReturnObject(gameObject);
    // }

    void OnDisable()
    {
        if(pool == null) return;
        pool.ReturnObject(gameObject);
    }
}
