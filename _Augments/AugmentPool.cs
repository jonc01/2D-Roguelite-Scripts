using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentPool : MonoBehaviour
{
    //Manager Script
    //Pool of possible augments that can be found, can changed based on tileset

    // public int totalUnownedAugments;
    public int totalOwnedAugments;

    [Header("Augment Lists by Tier")]
    public AugmentPoolHelper[] augmentPoolHelpers; //Tiers 0, 1, 2, 3, 4, 5
    
    [Header("Augment Lists")]
    public List<AugmentScript> ownedAugments; //picked augments and can be pulled from with a higher level
    public List<AugmentScript> listedAugmentsTEMP; //augments being listed in the Shop, temporarily filled to prevent duplicates
    public List<AugmentScript> currentListedAugments; //holds all currently listed augments
    [SerializeField] private AugmentInventory augmentInventory;

    void Start()
    {
        UpdatePoolSizes();
        if(augmentInventory == null) augmentInventory = GameManager.Instance.AugmentInventory;
    }

    private void UpdatePoolSizes()
    {
        totalOwnedAugments = ownedAugments.Count;
    }

    public AugmentScript GetNewAugment()
    {
        UpdatePoolSizes(); //Update count for randIndex

        //Get a duplicate augment if the Player has all unique augments
        // if(totalUnownedAugments == 0)
        // {
        //     return GetNewDuplicateAugment();
        // }
        // else
        // {
        //Get random index of unowned augments, and randomize level
        int currTier = RandomAugmentTier();
        AugmentScript newAugment = augmentPoolHelpers[currTier].GetRandomAugment();

        RandomizeAugmentStats(newAugment);
        SwapAugmentList(newAugment, GetAugmentList(newAugment), listedAugmentsTEMP);
        UpdatePoolSizes(); //Update again for referenced counts
        return newAugment;
    }

    public AugmentScript GetAugmentFromPool()//List<AugmentScript> augmentList)
    {
        //Add augments at random index
        AugmentScript augmentFromPool;
        int currTier = RandomAugmentTier();

        //Make sure selected pool isn't empty, reroll if so
        while(augmentPoolHelpers[currTier].IsEmpty())
        {
            currTier = RandomAugmentTier();
        }
        augmentFromPool = augmentPoolHelpers[currTier].GetRandomAugment();
        RandomizeAugmentStats(augmentFromPool);

        return augmentFromPool;
    }

    public void SwapAugmentList(AugmentScript addedAugment, List<AugmentScript> currList, List<AugmentScript> newList)
    {
        //Moves an Augment from one list to another
        newList.Add(addedAugment);
        currList.Remove(addedAugment);

        if(newList == listedAugmentsTEMP)
        {
            currentListedAugments.Add(addedAugment); //Add a copy to listedAugments
        }
    }

    public void ChooseAugment(AugmentScript chosenAugment)//, int augmentLevel) //TODO: testing
    {
        //Check if chosenAugment is already owned, remove old and reset stats
        // Debug.Log("AugLvl: " + chosenAugment.AugmentLevel + ", ShopLvl: " + augmentLevel);
        if(ownedAugments.Contains(chosenAugment))
        {
            augmentInventory.RemoveAugment(chosenAugment);
        }
        // chosenAugment.UpdateLevel(augmentLevel); //TODO: test: Manually updating level in case of duplicates
        SwapAugmentList(chosenAugment, GetAugmentList(chosenAugment), ownedAugments);

        augmentInventory.AddAugment(chosenAugment);
        
        //Augment was chosen, move the remaining listed augments back to unowned
        //EmptyStock() is called in FillStock()
    }

    public IEnumerator FillStock(List<AugmentScript> augmentsInStock, int totalAugments = 3)
    {
        for(int i=0; i<totalAugments; i++)
        {
            AugmentScript currAugment = GetAugmentFromPool();
            
            augmentsInStock.Add(currAugment); //AugmentSelectMenu //TODO:
            
            yield return new WaitForSecondsRealtime(.01f);
            StockShop(currAugment); //Pool list //TODO: this should work
        }
        yield return new WaitForSecondsRealtime(.01f);
        EmptyStock();
    }

    public void StockShop(AugmentScript augment, bool sellingDuplicates = true)
    {
        //Move augment from unownedPool into shopListed
        //This prevents duplicates
        if(sellingDuplicates) SwapAugmentList(augment, GetAugmentList(augment), listedAugmentsTEMP);
        else SwapAugmentList(augment, ownedAugments, listedAugmentsTEMP);
    }

    private List<AugmentScript> GetAugmentList(AugmentScript augment)
    {
        return augmentPoolHelpers[augment.Tier].augmentsList;
    }

#region Getting Augments and Stats
    private AugmentScript GetRandomAugment()
    {
        //Get a random augment from a random Tier, then randomize stats
        AugmentScript augment;
        int currTier = RandomAugmentTier();
        augment = augmentPoolHelpers[currTier].GetRandomAugment();
        //Only randomize if not a duplicate
        RandomizeAugmentStats(augment);

        return augment;
    }

    public void RandomizeAugmentStats(AugmentScript augment, bool skipListCheck = false)
    {
        //Check if Augment is already listed, this prevents listed duplicates from updating levels
        // if(CheckIfListed(augment)) return;
        if(CheckIfListed(augment) && !skipListCheck) return;

        //Can be called from other scripts if the Player wants to reroll the Level/stats
        int randLevel = RandomAugmentLevel();
        augment.UpdateLevel(randLevel); //Updates stats to Level
        // augment.UpdateLevel(5); //Updates stats to Level //TODO: DEBUGGING
    }

    private int RandomAugmentTier()
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

    private int RandomAugmentLevel()
    {
        int augmentLevel = 1; //1-5
        float rand = Random.Range(0f, 1.01f);

        // Debug.Log("Random Level: " + rand);
        
        if(rand >= .50f) augmentLevel = 1; //- 50%
        else if(rand >= .20f) augmentLevel = 2; //- 30%
        else if(rand >= .04f) augmentLevel = 3; //- 16%
        else if(rand >= .01f) augmentLevel = 4; //- 3%
        else augmentLevel = 5; //- 1%

        return augmentLevel;
    }
#endregion

    public void EmptyStock()
    {
        StartCoroutine(EmptyStockCO());
    }

    public IEnumerator EmptyStockCO()
    {
        //Returns Augments to their original lists, and removes from shopListedAugments
        while(listedAugmentsTEMP.Count > 0)
        {
            //Move from shopListed to unowned
            AugmentScript currAugment = listedAugmentsTEMP[0];
            int augmentTier = currAugment.Tier;
            SwapAugmentList(listedAugmentsTEMP[0], listedAugmentsTEMP, GetAugmentList(currAugment));
            yield return new WaitForSecondsRealtime(.01f); //Realtime since game is paused
        }
        UpdatePoolSizes();
        yield return new WaitForSecondsRealtime(.1f);
    }

    public IEnumerator EmptyStockCO(List<AugmentScript> augmentsInStock)
    {
        for(int i=0; i<augmentsInStock.Count; i++)
        {
            currentListedAugments.Remove(augmentsInStock[i]);
            yield return new WaitForSecondsRealtime(.01f); //Realtime since game is paused
        }
        UpdatePoolSizes();
        yield return new WaitForSecondsRealtime(.1f);
    }

    public bool CheckIfListed(AugmentScript augment)
    {
        return currentListedAugments.Contains(augment);
    }

    public AugmentScript GetNewDuplicateAugment()
    {
        UpdatePoolSizes();

        if(totalOwnedAugments == 0) return GetNewAugment();

        int randIndex = Random.Range(0, ownedAugments.Count);//totalOwnedAugments);
        var duplicateAugment = ownedAugments[randIndex];

        RandomizeAugmentStats(duplicateAugment);
        
        SwapAugmentList(duplicateAugment, ownedAugments, listedAugmentsTEMP);
        
        UpdatePoolSizes();
        return duplicateAugment;
    }
}
