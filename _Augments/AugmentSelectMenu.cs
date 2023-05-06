using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AugmentSelectMenu : MonoBehaviour
{
    //Menu displayed when the Player can choose an Augment
    //This is used for either 
    //  the Reward to [Choose] a free Augment
    //  the Shop menu to [Buy] an Augment
    [SerializeField] ShopController shopController;
    [SerializeField] AugmentInventory augmentInventory;
    [SerializeField] AugmentPool pool;
    [SerializeField] GameObject refreshingStockOverlay;
    private bool allowInput;

    [Header("SHOP or REWARD")]
    [SerializeField] public bool isShop = false;
    [SerializeField] public bool duplicates = false;

    [Header("Refresh Cost")]
    [SerializeField] public bool refreshAllowed = false;
    [SerializeField] public int refreshCost = 100;
    [SerializeField] TextMeshProUGUI refreshButtonText;

    [Header("Augment Slots")]
    [SerializeField] AugmentDisplay[] menuSlots;
    [SerializeField] private int[] augmentLevels;
    [SerializeField] public List<AugmentScript> augmentsInStock;

    [Header("=== Shop ===")]
    [SerializeField] int[] prices; //Includes prices for all augments based on Tier

    private int totalAugments = 3;
    private bool refreshingStock;
    public bool augmentSelected;
    private bool initialStockDone = false;
    
    void Awake()
    {
        augmentSelected = false;
        initialStockDone = false;
        totalAugments = menuSlots.Length;
        augmentLevels = new int[totalAugments];
        prices = new int[totalAugments];
        refreshingStock = false;
        allowInput = true;
        if(shopController == null) shopController = transform.parent.GetComponentInParent<ShopController>();
        if(pool == null) pool = shopController.augmentPool;
    }

    void OnEnable()
    {
        allowInput = true;

        if(!initialStockDone) InitialStock();
        else UpdateDisplay();

        GameManager.Instance.Pause.PauseTimeOnly();

        //If refresh isn't allowed, disable the button, otherwise update the displayed Refresh Cost text
        if(!refreshAllowed) refreshButtonText.transform.parent.gameObject.SetActive(false);
        else if(refreshButtonText != null) refreshButtonText.text = refreshCost.ToString();

        // if(augmentsInStock.Count == 3) UpdateDisplay(); //TODO:
    }

    void OnDisable()
    {
        GameManager.Instance.Pause.Resume();
    }

    private void InitialStock()
    {
        if(initialStockDone) return;
        if(pool == null) return;
        if(refreshingStock) return;
        StartCoroutine(RefreshStockCO());
        initialStockDone = true;
    }

    public void RefreshStock()
    {
        if(pool == null) return;
        if(!allowInput) return;
        if(!refreshAllowed || GameManager.Instance.Inventory.goldAmount < refreshCost) return;
        GameManager.Instance.Inventory.UpdateGold(-refreshCost);

        StartCoroutine(RefreshStockCO());
    }

    IEnumerator RefreshStockCO()
    {
        refreshingStock = true;
        refreshingStockOverlay.SetActive(true);

        for(int i=0; i<totalAugments; i++)
        {
            menuSlots[i].ToggleAugmentDisplay(false);
        }

        //Check if pool is filled before calling EmptyStockCO to remove augmentsInStock only
        if(pool.listedAugmentsTEMP.Count > 0) yield return pool.EmptyStockCO();
        yield return pool.EmptyStockCO(augmentsInStock);

        augmentsInStock.Clear();

        yield return pool.FillStock(augmentsInStock);

        for(int i=0; i<totalAugments; i++)
        {
            // augmentLevels[i] = currAugSlot.augmentScript.AugmentLevel;
            augmentLevels[i] = augmentsInStock[i].AugmentLevel; //TODO: test, 
            if(isShop) prices[i] = GetPrice(augmentsInStock[i]);
        }

        yield return new WaitForSecondsRealtime(.1f);
        
        UpdateDisplay();

        for(int i=0; i<totalAugments; i++)
        {
            menuSlots[i].ToggleAugmentDisplay(true);
        }
        
        refreshingStock = false;
        refreshingStockOverlay.SetActive(false);
    }

    public void SelectAugment(AugmentScript augment)
    {
        if(!allowInput) return;
        if(augmentInventory == null) augmentInventory = GameManager.Instance.AugmentInventory;
        int chosenIndex = augmentsInStock.IndexOf(augment);
        pool.ChooseAugment(augment);

        //Disable input for selecting Augments
        for(int i=0; i<totalAugments; i++) menuSlots[i].allowInput = false;

        // UpdateDisplay(true); //Updates price colors if player buys something //*Only need this if allowing multiple purchases

        //Disable inputs to close the Menu after one Augment was selected
        allowInput = false;
        augmentSelected = true;
        shopController.oneTimePurchaseDone = true;

        StartCoroutine(DisableSelectMenu(.5f));
    }

    IEnumerator DisableSelectMenu(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        transform.parent.gameObject.SetActive(false);
    }

    void UpdateDisplay(bool purchased = false)
    {
        //only using 'purchased' if allowing multiple purchases
        for(int i=0; i<menuSlots.Length; i++)
        {
            var currAugSlot = menuSlots[i].GetComponent<AugmentDisplay>();

            currAugSlot.alwaysDisplay = isShop;
            currAugSlot.augmentScript = augmentsInStock[i];

            if(isShop)
            {
                //Display Price
                currAugSlot.Price = prices[i];
                //Price is red if Player can't afford
                if(prices[i] > GameManager.Instance.Inventory.goldAmount)
                    currAugSlot.UpdateColor(false);
                else
                    currAugSlot.UpdateColor(true);
            }
            else //Hide Prices (0)
            {
                currAugSlot.TogglePrice(false);
            }

            AugmentScript currOwnedAugment = currAugSlot.augmentScript;
            bool duplicateListedOwned = IsOwnedAndListed(currOwnedAugment);
            
            if(duplicateListedOwned)
            {
                if(IsMaxLevel(currOwnedAugment))
                {
                    currAugSlot.allowInput = false;
                }
            }
            currAugSlot.RefreshInfo();
        }
    }

    public bool IsOwnedAndListed(AugmentScript augment)
    {
        bool duplicateListing = pool.currentListedAugments.Contains(augment);

        if(duplicateListing && IsOwned(augment)) return true;
        else return false;
    }

    public bool IsOwned(AugmentScript augment)
    {
        bool alreadyOwned = pool.ownedAugments.Contains(augment);
        return alreadyOwned;
    }

    public bool IsMaxLevel(AugmentScript augment)
    {
        if(augment.AugmentLevel >= augment.MaxLevel) return true;
        else return false;
    }

    private int GetPrice(AugmentScript augment)
    {
        //TODO: None of these values are balanced, just placeholders
        int totalPrice = 0;

        switch(augment.Tier)
        {
            case 0: totalPrice = 15; break;
            case 1: totalPrice = 30; break;
            case 2: totalPrice = 60; break;
            case 3: totalPrice = 120; break;
            case 4: totalPrice = 240; break;
            case 5: totalPrice = 360; break;
            default: totalPrice = 0; break;
        }

        switch(augment.AugmentLevel)
        {
            case 0: totalPrice += 5; break;
            case 1: totalPrice += 10; break;
            case 2: totalPrice += 20; break;
            case 3: totalPrice += 40; break;
            case 4: totalPrice += 80; break;
            case 5: totalPrice += 160; break;
            default: break;
        }

        return totalPrice;
    }
}
