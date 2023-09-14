using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentDisplay : MonoBehaviour
{
    [SerializeField] public AugmentScript augmentScript;
    [SerializeField] private AugmentSelectMenu selectMenu;
    public bool allowInput;

    [Space(10)]
    [Header("Display Toggles")]
    [SerializeField] bool inInventory = false;
    [SerializeField] GameObject selectedOverlay;
    [SerializeField] private TextMeshProUGUI selectedOverlayText;
    [SerializeField] private GameObject ownedText;
    [SerializeField] GameObject FullDisplayParent;

    [Space(10)]
    [Header("== Needed Setup ==")]
    [Header("Icons")]
    [SerializeField] public Image AugmentIcon_Image;
    [SerializeField] public Image Border_Image;

    [SerializeField] public bool alwaysDisplay = false;
    [Space(10)]
    [Header("Display on Hover")]
    [SerializeField] GameObject ToggleDescParent;
    [SerializeField] public TextMeshProUGUI DisplayName;
    [SerializeField] public TextMeshProUGUI DisplayDescription;
    [SerializeField] public TextMeshProUGUI DisplayLevel;
    //
    [Header("Price")]
    [SerializeField] public TextMeshProUGUI PriceDisplay;
    [SerializeField] public int Price;
    private Button button;
    
    [Header("Duplicate")]
    [SerializeField] private bool randomizeLevel = false;
    [SerializeField] public bool upgradeShop = false;

    void Start()
    {
        randomizeLevel = false;
        if(augmentScript == null)
        {
            Debug.Log("No Augment Scriptable Object referenced!");
            return;
        }
        allowInput = false;

        if(!alwaysDisplay) ToggleDescriptionDisplay(false);
        else ToggleDescriptionDisplay(true);

        if(ownedText != null) ownedText.SetActive(false);
    }

    void OnEnable()
    {
        if(selectMenu == null) selectMenu = GetComponentInParent<AugmentSelectMenu>();
        RefreshInfo();

        if(selectedOverlayText == null && selectedOverlay != null) selectedOverlayText = selectedOverlay.GetComponentInChildren<TextMeshProUGUI>();

        StartCoroutine(RevealAugment());
    }
    
    public void ToggleOverlay(bool toggle, bool maxLevel = false)
    {
        if(selectedOverlay == null) return;

        if(maxLevel) //&& selectMenu.IsMaxLevel(augmentScript))
        {
            ChangeOverlayText("Max Level");
        }
        else ChangeOverlayText("Purchased");

        selectedOverlay.SetActive(toggle);
    }

    public void ChangeOverlayText(string overlayText)
    {
        if(selectedOverlayText == null) return;
        selectedOverlayText.text = overlayText;
    }

    IEnumerator RevealAugment()
    {
        allowInput = false;
        // yield return new WaitForSecondsRealtime(.2f);
        //TODO: start animation here
        // yield return new WaitForSecondsRealtime(.2f); //Animation time
        yield return new WaitForSecondsRealtime(.1f); //Animation time

        allowInput = true;
        if(augmentScript != null && selectMenu != null)
        {
            if(selectMenu.IsOwnedAndListed(augmentScript))
            {
                if(selectMenu.IsMaxLevel(augmentScript))
                {
                    //Block purchase if max level
                    ToggleOverlay(true, true);
                    allowInput = false;
                }
                else
                {
                    //Only allow duplicate purchase if Upgrade Shop
                    allowInput = upgradeShop;
                    if(upgradeShop){
                        ToggleOverlay(false);
                    }else{
                        ToggleOverlay(true, false);
                    }
                }
            }
            

            // if(selectMenu.IsOwnedAndListed(augmentScript) && selectMenu.IsMaxLevel(augmentScript))
            // {
            //     //Overlay toggled if Augment is owned and listed, and is maxLevel
            //     ToggleOverlay(true, true);
            //     allowInput = false;
            // }
            // else
            // {
            //     ToggleOverlay(false);
            //     allowInput = true;
            // }
        }
    }

    public void SelectAugment()
    {
        if(!allowInput) return;
        if(selectMenu.isShop)
        {
            if(Price > GameManager.Instance.Inventory.goldAmount) return; //Player can't afford
            GameManager.Instance.Inventory.UpdateGold(-Price); //Take gold from player, update display
        }

        allowInput = false;
        selectMenu.SelectAugment(augmentScript, randomizeLevel);
        ToggleOverlay(true);
    }

    public void RefreshInfo()
    {
        if(augmentScript == null) return;

        DisplayName.text = augmentScript.Name;
        AugmentIcon_Image.sprite = augmentScript.Icon_Image;
        DisplayDescription.text = augmentScript.Description;

        bool isOwned;
        if(augmentScript != null && selectMenu != null)
        {
            if(selectMenu.IsOwned(augmentScript)) isOwned = true;
            else isOwned = false;

            bool toggleOverlay; //TODO: or true?
            bool isMaxLevel;

            //Overlay toggled if duplicate AND maxLevel

            if(upgradeShop)
            {
                if(selectMenu.IsMaxLevel(augmentScript))
                {
                    augmentScript.UpdateDescription();
                    randomizeLevel = false;
                    toggleOverlay = true;
                    isMaxLevel = true;
                }
                else
                {
                    DisplayLevel.text = "Lv ??";
                    augmentScript.UpdateDescription(true);

                    if(ownedText != null) ownedText.SetActive(true);
                    randomizeLevel = true;
                    toggleOverlay = false;
                    isMaxLevel = false;
                }
            }
            else
            {
                // DisplayLevel.text = "Lv" + augmentScript.AugmentLevel;

                if(selectMenu.IsMaxLevel(augmentScript)) //max Level no upgrade
                {
                    augmentScript.UpdateDescription();
                    randomizeLevel = false;

                    if(isOwned) toggleOverlay = true;
                    else toggleOverlay = false;

                    isMaxLevel = true;
                }
                else //Not Max Level, not in upgrade shop
                {
                    DisplayLevel.text = "Lv" + augmentScript.AugmentLevel;
                    augmentScript.UpdateDescription();

                    isMaxLevel = false;
                    if(ownedText != null) ownedText.SetActive(false);
                    randomizeLevel = false;
                    toggleOverlay = false;
                }
            }


            ToggleOverlay(toggleOverlay, isMaxLevel); //maxLevel false | "Purchased" overlay
        }
        else DisplayLevel.text = "Lv" + augmentScript.AugmentLevel;
        
        if(PriceDisplay != null) PriceDisplay.text = Price.ToString();
        GetBorderColor();
    }

    public void RefreshDisplayInfo()
    {
        if(augmentScript == null) return;

        DisplayName.text = augmentScript.Name;
        AugmentIcon_Image.sprite = augmentScript.Icon_Image;
        DisplayDescription.text = augmentScript.Description;
        DisplayLevel.text = "Lv" + augmentScript.AugmentLevel;
    }

    public void UpdateColor(bool playerCanAfford)
    {
        if(playerCanAfford) PriceDisplay.color = Color.white;
        else PriceDisplay.color = Color.red;
    }

    public void TogglePrice(bool toggle)
    {
        PriceDisplay.gameObject.SetActive(toggle);
    }

    public void ToggleDescriptionDisplay(bool toggle)
    {
        //Always display when in a Shop
        //Only display on mouse hover when in inventory
        ToggleDescParent.SetActive(toggle);
    }

    public void ToggleAugmentDisplay(bool toggle)
    {
        if(FullDisplayParent == null) return;
        FullDisplayParent.SetActive(toggle);
    }

    private void GetBorderColor()
    {
        switch(augmentScript.Tier)
        {
            case 0: //Common
                Border_Image.color = Color.white;
                break;
            case 1: //Rare
                Border_Image.color = new Color(0, 1, 0.2991796f); //Green
                break;
            case 2: //Epic
                Border_Image.color = new Color(0.6988888f, 0, 1); //Purple
                break;
            case 3: //Legendary
                Border_Image.color = new Color(1, 0.4445357f, 0.1745283f); //Orange
                break;
            case 4: //Unstable
                Border_Image.color = Color.red;
                break;
            default:
                break;
        }
    }
}
