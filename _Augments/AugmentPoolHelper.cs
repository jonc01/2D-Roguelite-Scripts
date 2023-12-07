using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentPoolHelper : MonoBehaviour
{
    [Header("All Augments in Tier")]
    [SerializeField] public List<AugmentScript> augmentsList;
    // public int totalAugments;

    void Start()
    {
        //Get initial count from child Augment objects
        UpdateCount();
    }

    public void UpdateCount()
    {
        // totalAugments = augmentsList.Count;
    }
    
    public AugmentScript GetRandomAugment()
    {
        UpdateCount();
        AugmentScript augment;
        int randIndex = Random.Range(0, augmentsList.Count); //maxExclusive, works for index
        augment = augmentsList[randIndex];
        return augment;
    }

    public bool IsEmpty()
    {
        // UpdateCount();
        return augmentsList.Count == 0;
    }

    public bool ListContains(AugmentScript augment)
    {
        return augmentsList.Contains(augment);
    }

//TODO: might not be needed, SwapAugmentList handles this
    public void RemoveAugment(AugmentScript augment)
    {
        augmentsList.Remove(augment);
        UpdateCount();
    }

    public void ReturnAugment(AugmentScript augment)
    {
        augmentsList.Add(augment);
        UpdateCount();
    }
}