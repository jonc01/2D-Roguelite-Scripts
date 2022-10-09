using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrbHolder : MonoBehaviour
{
    [Header("Orb Objects")]
    [SerializeField] GameObject[] orbs;
    [SerializeField] int totalOrbs;

    [Header("Adjustable Variables")]
    public float orbSpeed;

    [Header("Debugging")]
    public float launchDirection;

    void Start()
    {
        totalOrbs = transform.childCount;
        orbs = new GameObject[totalOrbs];
        for(int i = 0; i < totalOrbs; i++)
        {
            orbs[i] = transform.GetChild(i).gameObject;
        }
    }

    public void Launch(bool playerToRight)
    {
        if (playerToRight) launchDirection = -1;
        else launchDirection = 1;

        for (int i = 0; i < totalOrbs; i++)
        {
            orbs[i].SetActive(true);
        }
    }
}
