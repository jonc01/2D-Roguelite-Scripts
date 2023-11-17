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

    [Space(20)]
    [Header("- Music Manager Variables -")]
    //https://www.youtube.com/watch?v=3yKcrig3bU0
    [SerializeField] double musicDuration; //1 name
    [SerializeField] double goalTime; //1 name
    [SerializeField] AudioClip currentClip;

    [Space(20)]
    [Header("- Stage Music -")]
    [SerializeField] AudioClip[] StageMusicIntro;
    [SerializeField] float[] StageMusicIntroLength;
    [SerializeField] AudioClip[] StageMusicLoop;

    [Space(10)]
    [Header("- Boss Music -")]
    [SerializeField] AudioClip[] BossMusicIntro;
    [SerializeField] float[] BossMusicIntroLength;
    [SerializeField] AudioClip[] BossMusicLoop;
    Coroutine TransitionCoroutine;

    void Start()
    {
        if(audioManager == null) audioManager = AudioManager.Instance;
        fadeDuration = audioManager.fadeDuration;
        if(audioManager.playMusic == null) audioManager.playMusic = this;

        // AudioIngameSetup(); //TODO: NOT NEEDED IF FIX WORKS


    }

    void Update()
    {
        //1
        if(AudioSettings.dspTime > goalTime - 1) //1 what is '1'?
        {
            PlayScheduledClip();
        }
    }

    void PlayScheduledClip() //1
    {
        musicAudioSource.clip = currentClip;
        musicAudioSource.PlayScheduled(goalTime);

        musicDuration = (double)currentClip.samples / currentClip.frequency;
        goalTime = goalTime + musicDuration;

        // audioToggle = 1 - audioToggle; //??? is probably just index? of next clip
    }

    void SetCurrentClip(AudioClip clip) //1
    {
        currentClip = clip;
    }

    // //DEBUGGING
    // void Update()
    // {
    //     if(Input.GetKeyDown(KeyCode.Q))
    //     {
    //         PlayBossMusic();
    //     }
    // }

    public void AudioIngameSetup(bool menuStart = true)
    {
        //prevents audio cut when transitioning from intro to loop
        // StartCoroutine(TransitionSetup(menuStart));

        //TODO: NOT NEEDED IF FIX WORKS
    }

    IEnumerator TransitionSetup(bool playMenuMusic)
    {
        //Swap audio clips, prevents transition clipping
        audioManager.musicSource.mute = true;
        SwapClip(BossMusicIntro[0]);
        yield return new WaitForSeconds(.3f);
        SwapClip(BossMusicLoop[0]);
        yield return new WaitForSeconds(.3f);
        SwapClip(StageMusicLoop[0]);
        yield return new WaitForSeconds(.3f);

        audioManager.FadeOutAudio();
        
        if(playMenuMusic) SwapClip(MenuMusic);
        else PlayStageMusic();

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
    private void SwapIntroLoopMusic(AudioClip introClip, float introLength, AudioClip loopClip)
    {
        TransitionCoroutine = StartCoroutine(TransitionMusic_IntroLoop(introClip, introLength, loopClip));
    }

    IEnumerator TransitionMusic_IntroLoop(AudioClip newMusic, float introLength, AudioClip loopClip)
    {
        audioManager.FadeOutAudio();

        yield return new WaitForSeconds(fadeDuration);

        // musicAudioSource.loop = true;
        SwapClip(newMusic);

        audioManager.FadeInAudio();
        // Invoke("StartBossMusicLoop", BossMusicIntro[0].length); //.length not returning correct value
        // yield return new WaitForSecondsRealtime(17.145f);
        // yield return new WaitForSecondsRealtime(BossMusicIntroLength[index]);
        yield return new WaitForSeconds(introLength);
        // StartBossMusicLoop();
        SwapClip(loopClip);
    }
#endregion

    public void PlayBossMusic()
    {
        SwapIntroLoopMusic(BossMusicIntro[0], BossMusicIntroLength[0], BossMusicLoop[0]);
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

    public void PlayStageMusic()
    {
        // SwapClip(MenuMusic); //TODO: MenuMusic placeholder until separate InGame music
        // SwapClip(GameMusicIntro[0]);
        SwapIntroLoopMusic(StageMusicIntro[0], StageMusicIntroLength[0], StageMusicLoop[0]);
    }

    public void TransitionBossMusicToNormal()
    {
        SwapMusic(StageMusicLoop[0]);
        // PlayStageMusic();
        if(TransitionCoroutine != null) StopCoroutine(TransitionCoroutine);
    }
    
}
