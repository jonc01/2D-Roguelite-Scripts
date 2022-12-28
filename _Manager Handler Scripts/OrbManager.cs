using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbManager : MonoBehaviour
{
    //Spawns XP Orbs on enemy transform
    [Header("References")]
    [SerializeField] GameObject OrbPrefab;

    [Header("Adjustable Variables")]
    public float orbSpeed;

    public void SpawnOrbs(Vector3 pos, int totalOrbs)
    {
        for(int i = 0; i < totalOrbs; i++)
        {
            GameObject orb = Instantiate(OrbPrefab, pos, Quaternion.identity);
            orb.GetComponent<OrbController>();
        }
    }
}
