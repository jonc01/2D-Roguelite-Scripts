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
        int randLevel = RandomAugmentLevel(); //maxExclusive
        augment.UpdateLevel(randLevel);

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

    public int RandomAugmentTier()
    {
        int augmentTier; //1-5
        float rand = Random.Range(0f, 1.01f);
        
        if(rand >= .50f) augmentTier = 0; //Common - 50%
        else if(rand >= .20f) augmentTier = 1; //Rare - 30%
        else if(rand >= .04f) augmentTier = 2; //Epic - 16%
        else if(rand >= .01f) augmentTier = 3; //Legendary - 3%
        else{ //Overcharged or Unstable - 1%
            rand = Random.Range(0f, 1.0f);
            if(rand >= .5f) augmentTier = 4;
            else augmentTier = 5;
        }

        return augmentTier;
    }

    public int RandomAugmentLevel()
    {
        int augmentLevel = 1; //1-5
        float rand = Random.Range(0f, 1.01f);
        
        if(rand >= .50f) augmentLevel = 1; //- 50%
        else if(rand >= .20f) augmentLevel = 2; //- 30%
        else if(rand >= .04f) augmentLevel = 3; //- 16%
        else if(rand >= .01f) augmentLevel = 4; //- 3%
        else augmentLevel = 5; //- 1%

        return augmentLevel;
    }

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
