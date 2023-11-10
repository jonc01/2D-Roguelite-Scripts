using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentDisplay_HealOnly : AugmentDisplay
{
    [Header("- Heal Shop Only -")]
    [SerializeField] ShopController shopController;
    [SerializeField] protected float healAmountLower = 10;
    [SerializeField] protected float healAmountUpper = 30;
    [SerializeField] string healItemName = "";
    [SerializeField] Sprite healIcon;
    [SerializeField] float healAmount;

    public override void RefreshInfo()
    {
        healAmount = RandomHealAmount();

        DisplayName.text = healItemName;
        AugmentIcon_Image.sprite = healIcon;
        DisplayDescription.text = "Heal " + healAmount.ToString("N0") + " HP";

        Price = (int)healAmount;
        
        if(PriceDisplay != null)
        {
            if(selectMenu.bloodShop)
            {
                PriceDisplay.text = "-" + Price.ToString() + " HP";
            }
            else
            {
                PriceDisplay.text = Price.ToString();
            }
        }

        if(Price > GameManager.Instance.Inventory.goldAmount)
            UpdateColor(false);
        else
            UpdateColor(true);

        // GetBorderColor();
        Border_Image.color = Color.green;
    }

    private float RandomHealAmount()
    {
        float healAmount; //1-5
        // float rand = Random.Range(0f, 1.0f);
        float rand = Random.value;

        // Debug.Log("Random Level: " + rand);
        
        // if(rand >= .50f) augmentLevel = 1; //- 50%
        // else if(rand >= .20f) augmentLevel = 2; //- 30%
        // else if(rand >= .04f) augmentLevel = 3; //- 16%
        // else if(rand >= .01f) augmentLevel = 4; //- 3%
        // else augmentLevel = 5; //- 1%
        //
        if(rand <= .02f) healAmount = 50; //- 2%
        else if(rand <= .09f) healAmount = 40; //- 7%
        else if(rand <= .25f) healAmount = 30; //- 16%
        else if(rand <= .55f) healAmount = 20; //- 30%
        else healAmount = 15; //- 50%

        return healAmount;
    }

    public override void SelectAugment()
    {
        if(!allowInput) return;
        if(selectMenu.isShop)
        {
            //Take player health if blood Shop, else take gold
            if(selectMenu.bloodShop)
            {
                if(Price > GameManager.Instance.PlayerCombat.currentHP) return;
                GameManager.Instance.PlayerCombat.TakeDamage(Price);
            }
            else
            {
                if(Price > GameManager.Instance.Inventory.goldAmount) return; //Player can't afford
                GameManager.Instance.Inventory.UpdateGold(-Price); //Take gold from player, update display
            }
        }

        GameManager.Instance.PlayerCombat.HealPlayer(healAmount);

        allowInput = false;
        // selectMenu.SelectAugment(augmentScript, randomizeLevel);
        ToggleOverlay(true);
        selectMenu.CloseSelectMenu();

        //Need to manually set purchase here, others are set through AugmentSelectMenu
        if(shopController != null)
            shopController.SetPurchaseDone();
    }

}
