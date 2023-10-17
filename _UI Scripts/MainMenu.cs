using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject settingsMenuUI;
    public GameObject settingsMenuObj;
    public SettingsMenu settingsMenu;
    public GameObject gameSettings;
    public GameObject videoSettings;
    public GameObject soundSettings;

    public GameObject creditsMenuUI;
    [SerializeField] string[] StartingScenes;
    
    public void Play()
    {
        mainMenuUI.SetActive(false);

        if(StartingScenes.Length > 1)
        {
            int randIdx = Random.Range(0, StartingScenes.Length);
            AsyncLevelLoader.asyncLevelLoader.StartGame(StartingScenes[randIdx], "MainMenu");
        }
        else
        {
            AsyncLevelLoader.asyncLevelLoader.StartGame(StartingScenes[0], "MainMenu");
        }

    }

    /*public void Play()
    {
        //load next scene in build index
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }*/

    public void OpenSettings()
    {
        mainMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
        settingsMenu.UpdateSettings();
    }
    public void SettingsToMainMenu()
    {
        mainMenuUI.SetActive(true);
        settingsMenuUI.SetActive(false);
    }
    
    public void SettingsToGameSettings()
    {
        settingsMenuObj.SetActive(false);
        gameSettings.SetActive(true);
    }

    public void SettingsToVideoSettings()
    {
        settingsMenuObj.SetActive(false);
        videoSettings.SetActive(true);
    }

    public void GameOrVideoSettingsToSettingsMenu()
    {
        settingsMenuObj.SetActive(true);
        videoSettings.SetActive(false);
        gameSettings.SetActive(false);
        soundSettings.SetActive(false);
    }

    public void SettingsToSoundSettings()
    {
        settingsMenuObj.SetActive(false);
        soundSettings.SetActive(true);
    }
//

    public void QuitGame()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }


    public void OpenCredits()
    {
        mainMenuUI.SetActive(false);
        creditsMenuUI.SetActive(true);
    }

    public void CreditsToMainMenu()
    {
        mainMenuUI.SetActive(true);
        creditsMenuUI.SetActive(false);
    }
}
