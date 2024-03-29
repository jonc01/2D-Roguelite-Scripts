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
    public AugmentPoolHelper[] augmentPoolHelpers; //Tiers 0, 1, 2, 3, 4
    
    [Header("Augment Lists")]
    public List<AugmentScript> ownedAugments; //picked augments and can be pulled from with a higher level
    public List<AugmentScript> listedAugmentsTEMP; //augments being listed in the Shop, temporarily filled to prevent duplicates
    public List<AugmentScript> currentListedAugments; //holds all currently listed augments
    [SerializeField] private AugmentInventory augmentInventory;
    Coroutine ChoosingAugmentCO;

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

        Debug.Log("pool.GetAugmentFromPool()");
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

    public AugmentScript GetDuplicateAugment() //Duplicate
    {
        //Only used with upgradeShop
        // AugmentScript augmentFromPool;
        if(ownedAugments.Count < 3)
        {
            Debug.Log("Not enough owned augments!");
            return null;
        }

        int randIdx = Random.Range(0, ownedAugments.Count);
        while(listedAugmentsTEMP.Contains(ownedAugments[randIdx]))
        {
            randIdx = Random.Range(0, ownedAugments.Count);
        }

        return ownedAugments[randIdx];
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
        if(ownedAugments.Count >= 10) { Debug.Log("Reached 10 Augments"); return; }
        
        if(chosenAugment == null) Debug.Log("AugmentPool.chosenAugment NULL at function call");

        if(ownedAugments.Contains(chosenAugment))
        {
            augmentInventory.DuplicateAugment(chosenAugment);
        }
        else
        {
            // chosenAugment.UpdateLevel(augmentLevel); //TODO: test: Manually updating level in case of duplicates
            
            if(chosenAugment == null)
            {
                Debug.Log("AugmentPool.chosenAugment NULL ref");
                // return;
            }

            if(ownedAugments == null) Debug.Log("AugmentPool.ChooseAugment ERROR - ownedAugments is null");
            if(GetAugmentList(chosenAugment) == null) Debug.Log("AugmentPool.ChooseAugment ERROR - GetAugmentList is null");

            SwapAugmentList(chosenAugment, GetAugmentList(chosenAugment), ownedAugments);
            augmentInventory.AddAugment(chosenAugment);
            augmentInventory.AddConditionalAugment(chosenAugment);
        }
        //Augment was chosen, move the remaining listed augments back to unowned
        //EmptyStock() is called in FillStock()
    }

    public IEnumerator FillStock(List<AugmentScript> augmentsInStock, bool upgradeShop = false, int totalAugments = 3)
    {
        //Fill all augment slots in the Shop
        for(int i=0; i<totalAugments; i++)
        {
            AugmentScript currAugment;

            //Upgrade shop only pulls from ownedAugments pool
            if(upgradeShop) currAugment = GetDuplicateAugment();
            else currAugment = GetAugmentFromPool();
            
            augmentsInStock.Add(currAugment); //AugmentSelectMenu //TODO:
            
            yield return new WaitForSecondsRealtime(.01f);

            StockShop(currAugment); //Pool list
        }
        yield return new WaitForSecondsRealtime(.01f);
        EmptyStock();
    }

    public void StockShop(AugmentScript augment, bool sellingDuplicates = true)
    {
        //Move augment from unownedPool into shopListed
        //'sellingDuplicates' allow multiple shops to list the same unowned Augment
        if(sellingDuplicates) SwapAugmentList(augment, GetAugmentList(augment), listedAugmentsTEMP);
        else SwapAugmentList(augment, ownedAugments, listedAugmentsTEMP);
    }

    private List<AugmentScript> GetAugmentList(AugmentScript augment)
    {
        if(augment == null) 
        {
            Debug.Log("chosenAugment list not found: Tier " + augment.Tier);
            return null;
        }

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
        // augment.UpdateLevel(5); //Updates stats to Level //DEBUGGING
    }

    private int RandomAugmentTier()
    {
        int augmentTier; //1-5
        // float rand = Random.Range(0f, 1.01f); //additional 1% higher

        // float rand = Random.value;
        float rand = Random.value;

        ///////////////////// Testing drop rates ////////////////////////

        // int t5Count=0, t4Count=0, t3Count=0, t2Count=0, t1Count=0;

        // for(int i=0; i<100; i++)
        // {
        //     rand = Random.value;
        //     if(rand <= .02f){
        //     //Overcharged or Unstable - 1%
        //     // rand = Random.Range(0f, 1.0f);
        //         t5Count++;
        //     } 
        //     else if(rand <= .08f) { t4Count++; } //Legendary - 3%
        //     else if(rand <= .25f) { t3Count++; }//Epic - 16%
        //     else if(rand <= .55f) { t2Count++;}//Rare - 30%
        //     else { t1Count++; }//Common - 50%
        // }
        
        // Debug.Log("T1: " + t1Count);
        // Debug.Log("T2: " + t2Count);
        // Debug.Log("T3: " + t3Count);
        // Debug.Log("T4: " + t4Count);
        // Debug.Log("T5: " + t5Count);

        /////////////////////////////////////////////////////////


        if(rand <= .02f) augmentTier = 4; //Unstable - 2%
        else if(rand <= .06f) augmentTier = 3; //Legendary - 4%
        else if(rand <= .20f) augmentTier = 2; //Epic - 14%
        else if(rand <= .50f) augmentTier = 1; //Rare - 30%
        else augmentTier = 0; //Common - 50%

        return augmentTier;
    }

    private int RandomAugmentLevel()
    {
        int augmentLevel; //1-5
        // float rand = Random.Range(0f, 1.0f);
        float rand = Random.value;

        // Debug.Log("Random Level: " + rand);
        
        // if(rand >= .50f) augmentLevel = 1; //- 50%
        // else if(rand >= .20f) augmentLevel = 2; //- 30%
        // else if(rand >= .04f) augmentLevel = 3; //- 16%
        // else if(rand >= .01f) augmentLevel = 4; //- 3%
        // else augmentLevel = 5; //- 1%
        //
        if(rand <= .02f) augmentLevel = 5; //- 2%
        else if(rand <= .09f) augmentLevel = 4; //- 7%
        else if(rand <= .25f) augmentLevel = 3; //- 16%
        else if(rand <= .55f) augmentLevel = 2; //- 30%
        else augmentLevel = 1; //- 50%

        return augmentLevel;
    }
#endregion

    public void EmptyStock()
    {
        // if(listedAugmentsTEMP.Count == 0) return;
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
        //currentListedAugments is updated with listed Augments
        //Purchased shops will be removed from this, not-purchased shop augments remain
        for(int i=0; i<augmentsInStock.Count; i++)
        {
            currentListedAugments.Remove(augmentsInStock[i]);
            yield return new WaitForSecondsRealtime(.01f); //Realtime since game is paused
        }
        UpdatePoolSizes();
        yield return new WaitForSecondsRealtime(.1f);
    }

    public void StartClearPurchasedStock(List<AugmentScript> augmentsInStock)
    {
        //Temp not using
        StartCoroutine(ClearPurchasedStock(augmentsInStock));
    }

    private IEnumerator ClearPurchasedStock(List<AugmentScript> augmentsInStock)
    {
        //After a Shop or Reward augment is selected, remove augments from 'currentListedAugments'
            //Shops that are closed, but not purchased from yet will keep Augments in ^ list
        yield return new WaitForSecondsRealtime(.1f);

        currentListedAugments.RemoveAll(aug => augmentsInStock.Contains(aug));

        // currentlyListedItems.RemoveAll(item => listedItemsTemp.Contains(item));


        // for(int i=0; i<augmentsInStock.Count; i++)
        // {
        //     //Remove augment from augmentsInStock passed list
        //     Debug.Log("Removing " + augmentsInStock[i].name);
        //     currentListedAugments.Remove(augmentsInStock[i]);
        //     yield return new WaitForSecondsRealtime(.1f); //Delay needed to prevent missing references
        // }

        yield return new WaitForSecondsRealtime(.1f);
        UpdatePoolSizes();
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
