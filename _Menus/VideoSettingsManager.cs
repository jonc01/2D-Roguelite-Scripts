using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VideoSettingsManager : MonoBehaviour
{
    public static VideoSettingsManager Instance { get; private set; }

    [SerializeField] private bool fullscreenEnabledSaved;

    //Resolutions
    public int[] resWidth = { 960, 1280, 1920 };
    public int[] resHeight = { 540, 720, 1080 };
    // public TextMeshProUGUI resolutionDisplayed;
    [SerializeField] private int currentResIdxSaved = 1; //TODO: TEMP, setting default to 720, should use savefile

    private void Awake()
    {
        // Instance = this;
        // UpdateResDisplayed(currentResolution);

        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveFullscreen(bool fullscreen)
    {
        fullscreenEnabledSaved = fullscreen;
    }

    public void SaveResolution(int resIdx)
    {
        currentResIdxSaved = resIdx;
    }
    
    public bool GetSavedFullscreen()
    {
        return fullscreenEnabledSaved;
    }

    public int GetSavedResIdx()
    {
        return currentResIdxSaved;
    }
}
