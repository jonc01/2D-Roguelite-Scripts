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
    [SerializeField] GameObject selectedOverlay;
    [SerializeField] private TextMeshProUGUI selectedOverlayText;
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

    void Start()
    {
        if(augmentScript == null)
        {
            Debug.Log("No Augment Scriptable Object referenced!");
            return;
        }
        allowInput = false;

        // RefreshInfo();

        if(!alwaysDisplay) ToggleDescriptionDisplay(false);
        else ToggleDescriptionDisplay(true);

        if(selectedOverlayText == null && selectedOverlay != null) selectedOverlayText = selectedOverlay.GetComponentInChildren<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if(selectMenu == null) selectMenu = GetComponentInParent<AugmentSelectMenu>();
        ToggleOverlay(false);
        RefreshInfo();
        StartCoroutine(RevealAugment());
    }
    
    public void ToggleOverlay(bool toggle, bool purchased = true)
    {
        if(selectedOverlay == null) return;

        // selectedOverlay.SetActive(toggle);
        
        if(selectedOverlayText == null) return;
        if(selectedOverlayText.IsActive())
        {
            if(purchased) selectedOverlayText.text = "Purchased"; //default
            else selectedOverlayText.text = "Max Level";
        }
    }

    IEnumerator RevealAugment()
    {
        // yield return new WaitForSecondsRealtime(.2f);
        //TODO: start animation here
        // yield return new WaitForSecondsRealtime(.2f); //Animation time
        yield return new WaitForSecondsRealtime(.1f); //Animation time
        ToggleOverlay(false);
        allowInput = true;
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
        selectMenu.SelectAugment(augmentScript);
        ToggleOverlay(true);
    }

    public void RefreshInfo(bool upgrade = false)
    {
        if(augmentScript == null) return;

        ToggleOverlay(false);
        DisplayName.text = augmentScript.Name;
        AugmentIcon_Image.sprite = augmentScript.Icon_Image;
        DisplayDescription.text = augmentScript.Description;

        // DisplayLevel.text = "Lv" + augmentScript.AugmentLevel; //default

        if(upgrade) DisplayLevel.text = "Lv" + ((int)augmentScript.AugmentLevel+1); //TODO: testing, needs update/refresh
        else DisplayLevel.text = "Lv" + augmentScript.AugmentLevel;

        if(PriceDisplay != null) PriceDisplay.text = Price.ToString();
        GetBorderColor();
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
            case 4: //Overcharged
                Border_Image.color = Color.cyan;
                break;
            case 5: //Unstable
                Border_Image.color = Color.red;
                break;
            default:
                break;
        }
    }
}
