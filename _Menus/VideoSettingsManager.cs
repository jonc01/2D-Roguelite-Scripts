using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VideoSettingsManager : MonoBehaviour
{
    public static VideoSettingsManager Instance { get; private set; }

    public bool fullscreenEnabled;

    //Resolutions
    public int[] resWidth = { 960, 1280, 1920 };
    public int[] resHeight = { 540, 720, 1080 };
    // public TextMeshProUGUI resolutionDisplayed;
    public int currentResIdx = 1; //TODO: TEMP, setting default to 720, should use savefile

    private void Awake()
    {
        Instance = this;
        // UpdateResDisplayed(currentResolution);
    }

    public void SaveResolution(int resIdx)
    {
        currentResIdx = resIdx;
    }
    
    public void SaveFullscreen(bool fullscreen)
    {
        fullscreenEnabled = fullscreen;
    }
}
