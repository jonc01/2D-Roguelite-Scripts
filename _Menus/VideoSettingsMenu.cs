using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSettingsMenu : MonoBehaviour
{
    // public static VideoSettingsMenu Instance { get; private set; }


    // //Resolutions
    public int[] resWidth = { 960, 1280, 1920 };
    public int[] resHeight = { 540, 720, 1080 };
    public TextMeshProUGUI resolutionDisplayed;
    public int currentResolution = 1; //TODO: TEMP, setting default to 720, should use savefile
    
    [Space(10)]
    public bool fullscreenEnabled;

    [Header("Toggle Buttons")]
    public GameObject fullscreenOnButton;
    public GameObject fullscreenOffButton;

    // //Temp Variables
    public int selectedResolution;
    public bool selectedFullscreen;

    private void Start()
    {
        if(VideoSettingsManager.Instance != null)
        {
            currentResolution = VideoSettingsManager.Instance.currentResIdx;
            fullscreenEnabled = VideoSettingsManager.Instance.fullscreenEnabled;
        }

        UpdateResDisplayed(currentResolution);
        
        fullscreenOnButton.SetActive(fullscreenEnabled);
        fullscreenOffButton.SetActive(!fullscreenEnabled);

        selectedResolution = currentResolution;
        selectedFullscreen = fullscreenEnabled;
    }

    void OnEnable()
    {
        Debug.Log("Current Res: " + Screen.currentResolution); //TODO: remove
        UpdateSettings();
    }

    public void UpdateSettings() //called on initial Player scene load
    {
        fullscreenOnButton.SetActive(fullscreenEnabled);
        UpdateResDisplayed(currentResolution);
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

    public void ApplyRes() //On Apply button press
    {
        SetRes(selectedResolution);

        if(VideoSettingsManager.Instance != null)
        {
            VideoSettingsManager.Instance.SaveResolution(selectedResolution);
            VideoSettingsManager.Instance.SaveFullscreen(fullscreenEnabled);
        }
    }

    void SetRes(int selectRes)
    {
        Screen.SetResolution(resWidth[selectRes], resHeight[selectRes], selectedFullscreen, 60);
        currentResolution = selectRes;
        // fullscreenEnabled = selectedFullscreen; //TODO: testing, set elsewhere
    }

    public void ToggleFullscreenButton()
    {
        if (selectedFullscreen)
        {
            selectedFullscreen = false;
        }
        else
        {
            selectedFullscreen = true;
        }

        fullscreenOnButton.SetActive(selectedFullscreen);
        fullscreenOffButton.SetActive(!selectedFullscreen);
    }
}
