using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioManager audioManager;
    [SerializeField] float fadeDuration;

    [Space(20)]

    [SerializeField] AudioClip MenuMusic;
    [SerializeField] AudioClip[] GameMusic;
    [SerializeField] AudioClip[] BossMusicIntro;
    [SerializeField] AudioClip[] BossMusicLoop;
    Coroutine TransitionCoroutine;

    void Start()
    {
        if(audioManager == null) audioManager = AudioManager.Instance;
        fadeDuration = audioManager.fadeDuration;
        if(audioManager.playMusic == null) audioManager.playMusic = this;

        AudioIngameSetup();
    }

    // //DEBUGGING
    // void Update()
    // {
    //     if(Input.GetKeyDown(KeyCode.Q))
    //     {
    //         PlayBossMusic();
    //     }
    // }

    public void AudioIngameSetup()
    {
        //prevents audio cut when transitioning from intro to loop
        StartCoroutine(TransitionSetup());
    }

    IEnumerator TransitionSetup()
    {
        //Swap audio clips, prevents transition clipping
        audioManager.musicSource.mute = true;
        SwapClip(BossMusicIntro[0]);
        yield return new WaitForSeconds(.5f);
        SwapClip(BossMusicLoop[0]);
        yield return new WaitForSeconds(.5f);

        audioManager.FadeOutAudio();
        SwapClip(MenuMusic);
        yield return new WaitForSeconds(fadeDuration);
        audioManager.musicSource.mute = false;
        audioManager.FadeInAudio();
    }


    private void SwapMusic(AudioClip newMusic)
    {
        StartCoroutine(TransitionMusic(newMusic));
    }

    IEnumerator TransitionMusic(AudioClip newMusic)
    {
        audioManager.FadeOutAudio();

        yield return new WaitForSeconds(fadeDuration);

        musicAudioSource.loop = true;
        SwapClip(newMusic);

        audioManager.FadeInAudio();
    }

    private void SwapClip(AudioClip newMusic)
    {
        musicAudioSource.clip = newMusic;
        musicAudioSource.Play();
    }


#region Boss Music: Intro -> Loop
    private void SwapIntroLoopMusic(AudioClip bossMusic)
    {
        TransitionCoroutine = StartCoroutine(TransitionMusic_IntroLoop(bossMusic));
    }

    IEnumerator TransitionMusic_IntroLoop(AudioClip newMusic)
    {
        audioManager.FadeOutAudio();

        yield return new WaitForSecondsRealtime(fadeDuration);

        // musicAudioSource.loop = true;
        SwapClip(newMusic);

        audioManager.FadeInAudio();
        // Invoke("StartBossMusicLoop", BossMusicIntro[0].length); //.length not returning correct value
        yield return new WaitForSecondsRealtime(17.145f);
        StartBossMusicLoop();
    }
#endregion

    public void PlayBossMusic()
    {
        SwapIntroLoopMusic(BossMusicIntro[0]);
        // Invoke("StartBossMusicLoop", BossMusicIntro[0].length);
    }

    private void StartBossMusicLoop()
    {
        SwapClip(BossMusicLoop[0]);
    }

    public void PlayMenuMusic()
    {
        SwapClip(MenuMusic);
    }

    public void PlayInGameMusic()
    {
        SwapClip(MenuMusic); //TODO: MenuMusic placeholder until separate InGame music
    }

    public void TransitionBossMusicToNormal()
    {
        SwapMusic(MenuMusic);
        if(TransitionCoroutine != null) StopCoroutine(TransitionCoroutine);
    }
    
}
