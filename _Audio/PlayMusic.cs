using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioSource musicIntroAudioSource;
    [SerializeField] AudioManager audioManager;
    [SerializeField] float fadeDuration;

    [Space(20)]
    [SerializeField] AudioClip MenuMusic;

    [Space(20)]
    [Header("- Music Manager Variables -")]
    [SerializeField] double musicDuration; //1 name
    [SerializeField] double scheduledTime; //1 name

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

        SwapMusic(MenuMusic);
    }

    private void SwapMusic(AudioClip newMusic, bool looping = false)
    {
        if(TransitionCoroutine != null) StopCoroutine(TransitionCoroutine);
        TransitionCoroutine = StartCoroutine(TransitionMusic(newMusic));
    }

    IEnumerator TransitionMusic(AudioClip newMusic)
    {
        audioManager.FadeOutAudio();

        yield return new WaitForSeconds(fadeDuration);
        musicIntroAudioSource.Stop();

        SwapClip(newMusic);

        audioManager.FadeInAudio();
        musicAudioSource.loop = true;
    }

    private void SwapClip(AudioClip newMusic)
    {
        musicAudioSource.clip = newMusic;
        musicAudioSource.Play();
    }


#region Boss Music: Intro -> Loop
    private void SwapIntroLoopMusic(AudioClip introClip, float introLength, AudioClip loopClip)
    {
        if(TransitionCoroutine != null) StopCoroutine(TransitionCoroutine);
        TransitionCoroutine = StartCoroutine(TransitionMusic_IntroLoop(introClip, introLength, loopClip));
    }

    IEnumerator TransitionMusic_IntroLoop(AudioClip introClip, float introLength, AudioClip loopClip)
    {
        audioManager.FadeOutAudio();

        yield return new WaitForSeconds(fadeDuration);

        musicAudioSource.loop = false;
        musicAudioSource.Stop();
        musicIntroAudioSource.Stop();

        yield return new WaitForSeconds(.1f); //short delay before setting loop to true
        
        //Switch to intro clip
        musicIntroAudioSource.clip = introClip;

        //Get duration of intro clip
        musicDuration = (double)introClip.samples / introClip.frequency;

        musicIntroAudioSource.PlayScheduled(0);//Starting intro clip
        scheduledTime = AudioSettings.dspTime + musicDuration; //Get current time + duration
        

        musicAudioSource.clip = loopClip; //load loop clip
        musicAudioSource.PlayScheduled(scheduledTime); //wait for intro to end to play
        musicAudioSource.loop = true;

        yield return new WaitForSeconds(.1f);
        audioManager.FadeInAudio();
    }
#endregion

#region Public Calls
    public void PlayBossMusic()
    {
        SwapIntroLoopMusic(BossMusicIntro[0], BossMusicIntroLength[0], BossMusicLoop[0]);
    }

    private void StartBossMusicLoop()
    {
        PlayBossMusic();
    }

    public void PlayMenuMusic()
    {
        // musicIntroAudioSource.Stop();
        AudioListener.pause = false;
        if(TransitionCoroutine != null) StopCoroutine(TransitionCoroutine);
        SwapMusic(MenuMusic);
    }

    public void PlayStageMusic()
    {
        AudioListener.pause = false;
        SwapIntroLoopMusic(StageMusicIntro[0], StageMusicIntroLength[0], StageMusicLoop[0]);
    }

    public void TransitionBossMusicToNormal()
    {
        PlayStageMusic();
    }
#endregion
}
