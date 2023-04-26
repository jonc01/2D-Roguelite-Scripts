using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentPool : MonoBehaviour
{
    //Pool of possible augments that can be found, can changed based on tileset

    public int totalUnownedAugments;
    public int totalOwnedAugments;
    
    [Header("Augment pools")]
    public List<AugmentScript> unownedAugments; //available augments that haven't been picked yet
    public List<AugmentScript> ownedAugments; //picked augments and can be pulled from with a higher level
    public List<AugmentScript> shopListedAugments; //augments being listed in the Shop, temporarily filled to prevent duplicates
    
    public List<AugmentScript> tempPool; //TODO: when adding augments to the Shop or Rewards menu, hold here to prevent duplicates

    void Awake()
    {
        UpdatePoolSizes();
    }

    private void UpdatePoolSizes()
    {
        totalUnownedAugments = unownedAugments.Count;
        totalOwnedAugments = ownedAugments.Count;
    }

    public AugmentScript GetNewAugment()
    {
        UpdatePoolSizes(); //Update count for randIndex

        if(totalUnownedAugments == 0 && totalOwnedAugments == 0)
        {
            Debug.Log("AugmentPool is empty!");
            return null;
        }

        //Get a duplicate augment if the Player has all unique augments
        if(totalUnownedAugments == 0)
        {
            return GetNewDuplicateAugment();
        }
        else //Get random index of unowned augments, and randomize level
        {
            int randIndex = Random.Range(0, unownedAugments.Count);//totalUnownedAugments);
            var newAugment = unownedAugments[randIndex];
            newAugment.AugmentLevel = Random.Range(0, 6); //0-5
            SwapAugmentList(newAugment, unownedAugments, tempPool);
            UpdatePoolSizes(); //Update again for referenced counts
            return newAugment;
        }
    }

    public AugmentScript GetAugmentFromPool()//List<AugmentScript> augmentList)
    {
        //Add augments at random index into tempPool
        List<AugmentScript> poolList;
        if(unownedAugments.Count == 0) poolList = ownedAugments;
        else poolList = unownedAugments;

        int randIndex = Random.Range(0, poolList.Count);
        AugmentScript augmentFromPool = poolList[randIndex];
        RandomizeAugmentStats(augmentFromPool);

        return augmentFromPool;
    }

    public void RandomizeAugmentStats(AugmentScript augment)
    {
        //Can be called from other scripts if the Player wants to reroll the Level
        augment.AugmentLevel = Random.Range(1, 6); //maxExclusive
        //TODO: Need weights for each Level
    }

    public void SwapAugmentList(AugmentScript addedAugment, List<AugmentScript> newList, List<AugmentScript> prevList, bool selected = true)
    {
        //Moves an Augment from one list to another
        newList.Add(addedAugment);
        prevList.Remove(addedAugment);
    }

    public void ChooseAugment(AugmentScript chosenAugment)
    {
        //Check location of chosen augment
        //Move to ownedAugments if not already owned (duplicate)
        if(unownedAugments.Contains(chosenAugment))
            SwapAugmentList(chosenAugment, ownedAugments, unownedAugments);
        
        //Augment was chosen, move the remaining listed augments back to unowned
        //EmptyStock() is called in AugmentSelectMenu.SelectAugment()
    }

    public void StockShop(AugmentScript augment, bool unowned = true)
    {
        //Move augment from unownedPool into shopListedAugments
        //This prevents duplicates
        if(unowned) SwapAugmentList(augment, shopListedAugments, unownedAugments);
        else SwapAugmentList(augment, shopListedAugments, ownedAugments);
    }

    //////////////////// TODO: TESTING
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            for(int i=0; i<100; i++)
            {
                RandomAugmentTier();
            }
        }
    }

    ///////////////////////////////////////////////////

    public int RandomAugmentTier()
    {
        int augmentTier;
        float rand = Random.Range(0f, 1.01f);
        // float rand = Random.Range(0f, 1f);
        
        
        if(rand >= .58f) {augmentTier = 1; Debug.Log("Common - 42%");}
        else if(rand >= .27f) {augmentTier = 2; Debug.Log("Rare - 32%");}
        else if(rand >= .8f) {augmentTier = 3; Debug.Log("Epic - 19%");}
        else if(rand >= .1f) {augmentTier = 4; Debug.Log("Legendary - 5%");} //6
        else {augmentTier = 5; Debug.Log("Oc/Un - 2%");} //1%

        return augmentTier;
        // Common 42%
        // Rare 32%
        // Epic	19%
        // Legendary 5%
        // Overcharged 1% (50/50 to be Unstable)
        // Unstable 1% (50/50 to be Overcharged)
    }

    public int RandomAugmentLevel()
    {
        int augmentLevel = 1; //1-5
        float rand = Random.Range(0f, 1.01f);
        
        if(rand >= .58f) augmentLevel = 1; //TODO: change weights
        else if(rand >= .27f) augmentLevel = 2;
        else if(rand >= .8f) augmentLevel = 3;
        else if(rand >= .1f) augmentLevel = 4;
        else augmentLevel = 5;

        return augmentLevel;
        // Level 1 42%
        // Level 2 32%
        // Level 3	19%
        // Level 4 5%
        // Level 5 2%
    }

    ////////////////////

    public void EmptyStock()
    {
        StartCoroutine(EmptyStockCO());
    }

    public IEnumerator EmptyStockCO()
    {
        // if(shopListedAugments.Count == 0) Debug.Log("shoplistedaugments is empty"); //TODO: !!
        // else Debug.Log("shopListed Count: " + shopListedAugments.Count);

        while(shopListedAugments.Count > 0)
        {
            //Move from shopListed to unowned
            SwapAugmentList(shopListedAugments[0], unownedAugments, shopListedAugments, false);
            yield return new WaitForSecondsRealtime(.01f);
        }
        UpdatePoolSizes();
        yield return new WaitForSecondsRealtime(.1f);
    }

    public AugmentScript GetNewDuplicateAugment()
    {
        UpdatePoolSizes();

        if(totalOwnedAugments == 0) return GetNewAugment();

        int randIndex = Random.Range(0, ownedAugments.Count);//totalOwnedAugments);
        var duplicateAugment = ownedAugments[randIndex];

        duplicateAugment.AugmentLevel = Random.Range(0, 6); //0-5, maxExclusive
        //TODO: add weights, should be more likely to roll lower
        SwapAugmentList(duplicateAugment, ownedAugments, tempPool);
        
        UpdatePoolSizes();
        return duplicateAugment;
    }
}
