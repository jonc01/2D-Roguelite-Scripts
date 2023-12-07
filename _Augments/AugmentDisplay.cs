using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentDisplay : MonoBehaviour
{
    [SerializeField] public AugmentScript augmentScript;
    [SerializeField] protected AugmentSelectMenu selectMenu;
    public bool allowInput;

    [Space(10)]
    [Header("Display Toggles")]
    [SerializeField] protected bool inInventory = false;
    [SerializeField] protected GameObject selectedOverlay;
    [SerializeField] protected TextMeshProUGUI selectedOverlayText;
    [SerializeField] protected GameObject ownedText;
    [SerializeField] protected GameObject FullDisplayParent;

    [Header("Augment Status Display")]
    [SerializeField] protected GameObject timerParent;
    [SerializeField] protected TextMeshProUGUI timerDisplay;
    [SerializeField] float timer;

    [Space(10)]
    [Header("== Needed Setup ==")]
    [Header("Icons")]
    [SerializeField] public Image AugmentIcon_Image;
    [SerializeField] public Image Border_Image;

    [SerializeField] public bool alwaysDisplay = false;
    [Space(10)]
    [Header("Display on Hover")]
    [SerializeField] protected GameObject ToggleDescParent;
    [SerializeField] public TextMeshProUGUI DisplayName;
    [SerializeField] public TextMeshProUGUI DisplayDescription;
    [SerializeField] public TextMeshProUGUI DisplayLevel;
    //
    [Header("Price")]
    [SerializeField] public TextMeshProUGUI PriceDisplay;
    [SerializeField] public int Price;
    [SerializeField] protected Button button;
    
    [Header("Duplicate")]
    [SerializeField] protected bool randomizeLevel = false;
    [SerializeField] public bool upgradeShop = false;

    protected void Awake()
    {
        if(selectMenu == null) selectMenu = GetComponentInParent<AugmentSelectMenu>();
    }

    protected void Start()
    {
        randomizeLevel = false;
        if(augmentScript == null)
        {
            // Debug.Log("No Augment Scriptable Object referenced!");
            return;
        }
        allowInput = false;

        if(!alwaysDisplay) ToggleDescriptionDisplay(false);
        else ToggleDescriptionDisplay(true);

        //Info refresh
        // int level = augmentScript.AugmentLevel;
        // augmentScript.UpdateLevel(level);

        if(ownedText != null) ownedText.SetActive(false);
        if(timerParent != null) timerParent.SetActive(false);

        timer = 0;
    }

    protected void OnEnable()
    {
        // if(selectMenu == null) selectMenu = GetComponentInParent<AugmentSelectMenu>();
        RefreshInfo();

        if(selectedOverlayText == null && selectedOverlay != null) selectedOverlayText = selectedOverlay.GetComponentInChildren<TextMeshProUGUI>();

        StartCoroutine(RevealAugment());
    }

    void Update()
    {
        if(timerParent == null || timerDisplay == null) return;

        if(timer > 0)
        {
            timer -= Time.deltaTime;
            timerDisplay.text = timer.ToString("N1") + "s";
        }
        else
        {
            timerParent.SetActive(false);
        }
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

    protected IEnumerator RevealAugment()
    {
        if(button != null) button.interactable = false;
        allowInput = false;
        //start reveal animation here

        yield return new WaitForSecondsRealtime(.1f); //Animation time

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
        }

        //Wait for augment reference to be set before allowing input
        while(augmentScript == null)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(.2f); //short delay to prevent accidental buy when jumping

        if(button != null) button.interactable = true;
        allowInput = true;
    }

    public virtual void SelectAugment()
    {
        if(!allowInput) return;
        if(selectMenu.isShop)
        {
            //Take player health if blood Shop, else take gold
            if(selectMenu.bloodShop)
            {
                if(Price >= GameManager.Instance.PlayerCombat.currentHP) return;
                GameManager.Instance.PlayerCombat.TakeTrueDamage(Price);
                // GameManager.Instance.PlayerCombat.TakeDamage(Price);
            }
            else
            {
                if(Price > GameManager.Instance.Inventory.goldAmount) return; //Player can't afford
                GameManager.Instance.Inventory.UpdateGold(-Price); //Take gold from player, update display
            }
        }

        allowInput = false;
        if(augmentScript == null) Debug.Log("AUGMENT MISSING FROM DISPLAY");
        selectMenu.SelectAugment(augmentScript, randomizeLevel);
        ToggleOverlay(true);
    }

    public virtual void RefreshInfo()
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
                    DisplayLevel.text = "Lv" + augmentScript.AugmentLevel;
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

        GetBorderColor();
    }

    public void RefreshDisplayInfo()
    {
        if(augmentScript == null) return;

        RefreshInfo();
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

    protected void GetBorderColor()
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

    public void ToggleAugmentStatus(bool toggle, float timerDuration)
    {
        if(timerParent == null) return;

        //Toggle timer display and start timer
        timerParent.SetActive(toggle);
        if(toggle) timer = timerDuration;
    }
}
