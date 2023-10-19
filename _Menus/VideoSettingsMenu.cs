using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSettingsMenu : MonoBehaviour
{
    //DISPLAY ONLY
    //  Used for Settings Menu, actual settings are saved in VideoSettingsManager.cs


    // //Resolutions
    public int[] resWidth = { 960, 1280, 1920 };
    public int[] resHeight = { 540, 720, 1080 };
    public TextMeshProUGUI resolutionDisplayed;
    public int currentResIdx = 1; //TODO: TEMP, setting default to 720, should use savefile
    
    [Space(10)]
    public bool fullscreenEnabled;

    [Header("Toggle Buttons")]
    public GameObject fullscreenOnButton;
    // public GameObject fullscreenOffButton;

    // //Temp Variables
    public int selectedResolution;
    public bool selectedFullscreen;
    

    void OnEnable()
    {
        //refresh displayed settings
        if(VideoSettingsManager.Instance != null)
        {
            //Getting saved settings
            currentResIdx = VideoSettingsManager.Instance.GetSavedResIdx();
            fullscreenEnabled = VideoSettingsManager.Instance.GetSavedFullscreen();
        }

        //Update displayed settings with saved settings
        selectedResolution = currentResIdx;
        selectedFullscreen = fullscreenEnabled;

        UpdateSettings();
    }

    public void UpdateSettings() //called on initial Player scene load
    {
        fullscreenOnButton.SetActive(selectedFullscreen);
        UpdateResDisplayed(selectedResolution);
    }

    void UpdateResDisplayed(int selected)
    {
        resolutionDisplayed.text = resWidth[selected].ToString() + " x " + resHeight[selected].ToString();
    }

    public void MoveThroughResOptions(bool forwards) //On button press
    {
        //Works with more resolutions
        if (forwards)
        {
            if (selectedResolution < resHeight.Length - 1)
                selectedResolution++;
        }
        else
        {
            if (selectedResolution > 0)
                selectedResolution--;
        }
        UpdateResDisplayed(selectedResolution);
    }

    public void ApplySettings() //On Apply button press
    {
        //Update settings to the selected
        fullscreenEnabled = selectedFullscreen;
        SetRes();

        //Save settings
        if(VideoSettingsManager.Instance != null)
        {
            VideoSettingsManager.Instance.SaveResolution(currentResIdx);
            VideoSettingsManager.Instance.SaveFullscreen(fullscreenEnabled);
        }
    }

    void SetRes()
    {
        FullScreenMode fullscreenMode;
        if(fullscreenEnabled) fullscreenMode = FullScreenMode.FullScreenWindow;
        else fullscreenMode = FullScreenMode.Windowed;

        Screen.SetResolution(resWidth[selectedResolution], resHeight[selectedResolution],
        fullscreenMode, new RefreshRate(){numerator = 60, denominator = 1});

        currentResIdx = selectedResolution;
    }

    public void ToggleFullscreenButton()
    {
        //Called when button is pressed, will display the opposite of the previous selection
        if (selectedFullscreen)
        {
            selectedFullscreen = false;
        }
        else
        {
            selectedFullscreen = true;
        }

        fullscreenOnButton.SetActive(selectedFullscreen);
    }
}
