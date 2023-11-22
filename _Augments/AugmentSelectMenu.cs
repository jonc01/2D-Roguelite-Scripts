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
    [SerializeField] protected ShopController shopController;
    [SerializeField] protected AugmentInventory augmentInventory;
    [SerializeField] protected AugmentPool pool;
    [SerializeField] protected GameObject refreshingStockOverlay;
    protected bool allowInput;

    [Header("SHOP or REWARD")]
    [SerializeField] public bool isShop = false;
    // [SerializeField] public bool duplicates = false;
    [SerializeField] public bool upgradeShop = false;
    [SerializeField] public bool bloodShop = false;

    [Header("Refresh Cost")]
    [SerializeField] public bool refreshAllowed = false;
    [SerializeField] public int refreshCost = 100;
    [SerializeField] protected TextMeshProUGUI refreshButtonText;

    [Header("Augment Slots")]
    [SerializeField] protected AugmentDisplay[] menuSlots;
    [SerializeField] protected int[] augmentLevels;
    [SerializeField] public List<AugmentScript> augmentsInStock;

    [Header("=== Shop ===")]
    [SerializeField] protected int[] prices; //Includes prices for all augments based on Tier

    protected int totalAugments = 3;
    protected bool refreshingStock;
    public bool augmentSelected;
    protected bool initialStockDone = false;
    
    protected virtual void Awake()
    {
        augmentSelected = false;
        initialStockDone = false;
        totalAugments = menuSlots.Length;
        augmentLevels = new int[totalAugments];
        prices = new int[totalAugments];
        refreshingStock = false;
        allowInput = true;
        if(shopController == null) shopController = transform.parent.GetComponentInParent<ShopController>();
        if(shopController != null && pool == null) pool = shopController.augmentPool;
        if(pool == null) pool = GameManager.Instance.AugmentPool;
    }

    protected virtual void OnEnable()
    {
        allowInput = true;

        if(!initialStockDone) InitialStock();
        else UpdateDisplay();

        GameManager.Instance.shopOpen = true;
        GameManager.Instance.Pause.PauseTimeOnly();

        //If refresh isn't allowed, disable the button, otherwise update the displayed Refresh Cost text
        if(!refreshAllowed) refreshButtonText.transform.parent.gameObject.SetActive(false);
        else if(refreshButtonText != null) refreshButtonText.text = refreshCost.ToString();
    }

    protected void OnDisable()
    {
        GameManager.Instance.shopOpen = false;
        GameManager.Instance.rewardOpen = false;
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

    protected IEnumerator RefreshStockCO()
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

        yield return pool.FillStock(augmentsInStock, upgradeShop);

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

    public virtual void SelectAugment(AugmentScript augment, bool randomizeLevel)
    {
        if(!allowInput) return;

        //Check references
        if(augmentInventory == null) augmentInventory = GameManager.Instance.AugmentInventory;
        int chosenIndex = augmentsInStock.IndexOf(augment);

        if(pool == null) pool = GameManager.Instance.AugmentPool;

        //Get Random level if it is a duplicate Augment
        if(randomizeLevel) pool.RandomizeAugmentStats(augment, true);
        
        pool.ChooseAugment(augment); //Randomize augment before adding

        //Disable input for selecting Augments
        for(int i=0; i<totalAugments; i++) menuSlots[i].allowInput = false;

        // UpdateDisplay(true); //Updates price colors if player buys something //*Only need this if allowing multiple purchases

        //Disable inputs to close the Menu after one Augment was selected
        allowInput = false;
        augmentSelected = true;

        if(shopController != null)
            shopController.SetPurchaseDone();

        StartCoroutine(DisableSelectMenu(.5f));
    }

    public void CloseSelectMenu(float delay = .5f)
    {
        //For external calls
        StartCoroutine(DisableSelectMenu(delay));
    }

    protected IEnumerator DisableSelectMenu(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        transform.parent.gameObject.SetActive(false);
    }

    protected void UpdateDisplay(bool purchased = false)
    {
        //only using 'purchased' if allowing multiple purchases
        for(int i=0; i<menuSlots.Length; i++)
        {
            var currAugSlot = menuSlots[i].GetComponent<AugmentDisplay>();

            currAugSlot.alwaysDisplay = isShop;
            currAugSlot.upgradeShop = upgradeShop; //Toggle duplicate purchases
            currAugSlot.augmentScript = augmentsInStock[i];

            if(isShop)
            {
                //Display Price
                currAugSlot.Price = prices[i];
                //Price is red if Player can't afford
                if(bloodShop)
                {
                    if(prices[i] >= (int)GameManager.Instance.PlayerCombat.currentHP)
                        currAugSlot.UpdateColor(false);
                    else
                        currAugSlot.UpdateColor(true);
                }
                else //Normal Shop
                {
                    if(prices[i] > GameManager.Instance.Inventory.goldAmount)
                        currAugSlot.UpdateColor(false);
                    else
                        currAugSlot.UpdateColor(true);
                }

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

    protected virtual int GetPrice(AugmentScript augment)
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
            case 3: totalPrice += 30; break;
            case 4: totalPrice += 50; break;
            case 5: totalPrice += 60; break;
            default: break;
        }

        return totalPrice;
    }
}
