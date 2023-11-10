using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncLevelLoader : MonoBehaviour
{
    public static AsyncLevelLoader asyncLevelLoader;
    bool playerLoaded;

    [Header("Loading Screen")]
    [SerializeField] private GameObject LoadScreen;
    [SerializeField] private GameObject LoadingText;
    [SerializeField] private Animator LoadScreenAnim;
    [SerializeField] private string loadingStartAnim = "LoadingFadeStart";
    [SerializeField] private float loadScreenFadeOutDuration;
    [SerializeField] private string loadingEndAnim = "LoadingFadeEnd";
    [SerializeField] private float loadScreenFadeInDuration;
    public bool allowLoad;
    

    [Space(10)]
    [SerializeField] private GameObject playerObject;
    //TODO: needs testing with new transforms
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, -3.35f, 0f ); //default: 

    private void Awake()
    {
        //Create new Instance if no existing, otherwise delete this object
        if(asyncLevelLoader == null)
        {
            asyncLevelLoader = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        //

        if(playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");

        if (LoadScreen == null)
            LoadScreen = GameObject.Find("FadeIn");

        if (LoadScreenAnim == null)
            if (LoadScreen != null)
                LoadScreenAnim = LoadScreen.GetComponent<Animator>();

        if (LoadScreenAnim != null)
            InitialLoadScene();
    }

    void Update()
    {
        //TODO: TESTING LEVEL RESET
        if(Input.GetKeyDown(KeyCode.N))
        {
            // StartCoroutine(ResetRun());
            ResetRun();
        }
    }

#region Reset Run
    public void ResetRun()
    {
        AudioManager.Instance.FadeOutAudio();
        GameManager.Instance.TogglePlayerInput(false);
        StartCoroutine(ResetRunCO());
    }

    IEnumerator ResetRunCO()
    {
        yield return StartLoadingCO();
        LoadingText.SetActive(true);

        //Loading in Single mode unloads all other scenes
        var mainScene = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        
        while(!mainScene.isDone)
        {
            Debug.Log("Loading MainMenu...");
            yield return null;
        }

        AudioManager.Instance.playMusic.AudioIngameSetup();
        // AudioManager.Instance.FadeInAudio();
        GameManager.Instance.TogglePlayerInput(true);

        StartGame("Tileset1_LevelGen", "MainMenu");
    }

#endregion

    void InitialLoadScene()
    {
        StartCoroutine("InitialLoadCO");
    }

    IEnumerator InitialLoadCO()
    {
        // LoadScreenAnim.SetTrigger("StartLoop");
        yield return new WaitForSeconds(.1f);
        yield return EndLoadingCO();
    }

    public void LoadScene(string sceneName, string sceneToUnload) //TODO: manual call from menu Play(), can combine this into one script (LevelLoader)
    {
        //EndPortal script is in the scene we are leaving
        //is passes the name of the scene we want to load, and the current scene to unload after
        
        StartCoroutine(LoadSceneCO(sceneName, sceneToUnload));

        // Unload prev scene async
        //UnloadScene(sceneToUnload);
    }

    IEnumerator LoadSceneCO(string sceneName, string sceneToUnload)
    {
        //LoadScreen.SetActive(true);
        yield return StartLoadingCO();
        yield return new WaitForSeconds(.1f); //short delay to allow fade out animation to fully fade to load screen 
        //Without this delay, the player would see scenes being moved around

        AsyncOperation sceneToLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!sceneToLoad.isDone)
        {
            yield return null;
        }

        if(playerObject != null)
        {
            if(playerObject.transform.position != spawnPosition)
                playerObject.transform.position = spawnPosition; //moves player to spawnPlatform while stages unload
        }

        UnloadScene(sceneToUnload);

        yield return EndLoadingCO();
    }

    public void LoadScene(string sceneName)
    {
        //SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        StartCoroutine(LoadSceneCO(sceneName));
    }

    IEnumerator LoadSceneCO(string sceneName)
    {
        // LoadScreenAnim.SetTrigger("StartLoop");
        AsyncOperation sceneToLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!sceneToLoad.isDone)
        {
            yield return null;
        }

        yield return EndLoadingCO();
        //LoadScreen.SetActive(false);
    }

    public void UnloadScene(string sceneName)
    {
        StartCoroutine(Unload(sceneName));
    }

    IEnumerator Unload(string sceneName)
    {
        yield return null; //delay before starting scene unload, otherwise crash
        SceneManager.UnloadSceneAsync(sceneName); //UnloadScene()
    }

    public void LoadPlayer()
    {
        StartCoroutine(StartLoadPlayer());
    }

    IEnumerator StartLoadPlayer()
    {
        playerLoaded = false;
        //In case we already have a Player object loaded, this unloads the old Player scene and loads a new one
        //Only call this when starting a game, or restarting.
        //TODO: needs testing for Restart //TODO: can workaround by sending player to MainMenu on death
        for (int i=0; i<SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name == "_PlayerScene")
            {
                UnloadScene("_PlayerScene");
            }
        }

        yield return null;
        var playerScene = SceneManager.LoadSceneAsync("_PlayerScene", LoadSceneMode.Additive);

        while (!playerScene.isDone)
        {
            Debug.Log("loading player");
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("_PlayerScene"));

        playerLoaded = true;
    }

    public void StartGame(string startStage, string unloadStage)
    {
        //1) Load Player scene (_PlayerScene)
        //2) When loaded, set Player scene as active scene (allows us to unload MainMenu scene)
        //3) Unload MainMenu
        //4) Load TutorialStage

        AudioManager.Instance.FadeOutAudio();

        StartCoroutine(StartGameCO(startStage, unloadStage));
    }

    IEnumerator StartGameCO(string startStage, string unloadStage)
    {
        // LoadScreenAnim.SetTrigger("Start");
        yield return StartLoadingCO();
        yield return new WaitForSeconds(.1f); //TODO: placeholder, LevelLoader is being deleted when loading
        LoadingText.SetActive(true);

        LoadPlayer();
        allowLoad = false;

        while (!playerLoaded)
        {
            Debug.Log("loading player");
            yield return null;
        }

        GameManager.Instance.TogglePlayerInput(false);

        SceneManager.UnloadSceneAsync(unloadStage); //Unload MainMenu
        LoadScene(startStage);
        // allowLoad = true;

        while(!allowLoad)
        {
            //allowLoad set to true once level generation is done
            Debug.Log("Building Level...");
            yield return null;
        }

        yield return EndLoadingCO();
        // LoadingText.SetActive(false);

        // GameManager.Instance.TogglePlayerInput(true);
        AudioManager.Instance.FadeInAudio();
    }

    //
    public void LoadMainMenu(string sceneToUnload) //String param not needed
    {
        AudioManager.Instance.FadeOutAudio();
        GameManager.Instance.TogglePlayerInput(false);
        StartCoroutine(LoadMainMenuCO(sceneToUnload));
    }

    IEnumerator LoadMainMenuCO(string sceneToUnload)
    {
        yield return StartLoadingCO();
        // yield return new WaitForSecondsRealtime(loadScreenFadeOutDuration);
        LoadingText.SetActive(true);
        // yield return Unload(sceneToUnload);
        // yield return Unload("_PlayerScene");

        //Loading in Single mode unloads all other scenes
        var mainScene = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        
        while(!mainScene.isDone)
        {
            Debug.Log("Loading MainMenu...");
            yield return null;
        }

        // SceneManager.LoadScene("MainMenu");

        // LoadingText.SetActive(false); //toggled in EndLoadingCO
        AudioManager.Instance.playMusic.PlayMenuMusic();
        AudioManager.Instance.FadeInAudio();

        yield return EndLoadingCO();
        GameManager.Instance.TogglePlayerInput(true);
        // yield return new WaitForSecondsRealtime(loadScreenFadeOutDuration);
    }

    //
    // private void StartLoadingAnim()
    // {
    //     StartCoroutine(StartLoadingCO());
    // }

    IEnumerator StartLoadingCO()
    {
        LoadScreenAnim.Play(loadingStartAnim);
        LoadingText.SetActive(true);
        yield return new WaitForSecondsRealtime(loadScreenFadeOutDuration);
    }

    // private void EndLoadingAnim()
    // {
    //     StartCoroutine(EndLoadingCO());
    // }

    IEnumerator EndLoadingCO()
    {
        // yield return new WaitForSecondsRealtime(1.5f);
        LoadScreenAnim.Play(loadingEndAnim);
        LoadingText.SetActive(false);
        GameManager.Instance.TogglePlayerInput(true);
        yield return new WaitForSecondsRealtime(loadScreenFadeInDuration);
    }
}
