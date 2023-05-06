using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentPoolHelper : MonoBehaviour
{
    [Header("All Augment Pools")]
    [SerializeField] public List<AugmentScript> augmentsList;
    public int totalAugments;

    void Start()
    {
        UpdateCount();
    }

    private void UpdateCount()
    {
        totalAugments = augmentsList.Count;
    }
    
    public AugmentScript GetRandomAugment()
    {
        UpdateCount();
        AugmentScript augment;
        int randIndex = Random.Range(0, totalAugments); //maxExclusive, works for index
        augment = augmentsList[randIndex];
        return augment;
    }

    public bool IsEmpty()
    {
        UpdateCount();
        return totalAugments == 0;
    }

    public bool ListContains(AugmentScript augment)
    {
        return augmentsList.Contains(augment);
    }

//TODO: might not be needed, SwapAugmentList handles this
    public void RemoveAugment(AugmentScript augment)
    {
        augmentsList.Remove(augment);
    }

    public void ReturnAugment(AugmentScript augment)
    {
        augmentsList.Add(augment);
    }
}