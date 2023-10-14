using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AugmentSelectMenu_Blood : AugmentSelectMenu
{
    
    public override void SelectAugment(AugmentScript augment, bool randomizeLevel)
    {
        if(!allowInput) return;
        if(augmentInventory == null) augmentInventory = GameManager.Instance.AugmentInventory;
        // int chosenIndex = augmentsInStock.IndexOf(augment);

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

    protected override int GetPrice(AugmentScript augment)
    {
        int totalHealthCost;

        switch(augment.Tier)
        {
            case 0: totalHealthCost = 2; break;
            case 1: totalHealthCost = 4; break;
            case 2: totalHealthCost = 6; break;
            case 3: totalHealthCost = 8; break;
            case 4: totalHealthCost = 10; break;
            case 5: totalHealthCost = 12; break;
            default: totalHealthCost = 0; break;
        }

        switch(augment.AugmentLevel)
        {
            case 0: totalHealthCost += 1; break;
            case 1: totalHealthCost += 2; break;
            case 2: totalHealthCost += 3; break;
            case 3: totalHealthCost += 4; break;
            case 4: totalHealthCost += 5; break;
            case 5: totalHealthCost += 6; break;
            default: break;
        }

        return totalHealthCost;
    }
}
