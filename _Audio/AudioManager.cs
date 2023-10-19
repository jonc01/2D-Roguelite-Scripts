using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Settings")]
    [SerializeField] public bool toggleSFX;
    [SerializeField] public bool toggleMusic;
    [Header("Audio Sources")]
    [SerializeField] public AudioSource musicSource, ambientSource, sfxSource;
    //TODO: add Ambient sound source
    [Space(10)]
    [Header("Audio Fade Settings")]
    [SerializeField] float fadeDuration = 2f;

    [Header("Debugging")]
    [SerializeField] private float startVolumeMusic;
    [SerializeField] private float startVolumeAmbient;
    [SerializeField] private float fadeTimer;
    [SerializeField] private bool fadingAudio;
    [SerializeField] private bool fadingInAudio;
    private float targetVolume;
    
    private float startingSetVolumeMusic;


    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        fadingAudio = false;
        fadingInAudio = false;
    }

    void Start()
    {
        startVolumeMusic = musicSource.volume;
        startVolumeAmbient = ambientSource.volume;

        startingSetVolumeMusic = startVolumeMusic;

        fadeTimer = 0;
    }

    void Update()
    {   
        if (fadingAudio)
        {
            if (fadeTimer < fadeDuration)
            {
                musicSource.volume = Mathf.Lerp(startVolumeMusic, targetVolume, fadeTimer / fadeDuration);
                fadeTimer += Time.unscaledDeltaTime;
            }
            else
            {
                musicSource.volume = targetVolume;
                fadingAudio = false;
                fadeTimer = 0f;
            }
        }
    }

    public void PlaySound(AudioClip clip)
    {
        //Check if sound is already playing, stop current sound then play new one
        if(sfxSource.isPlaying) sfxSource.Stop();
        sfxSource.PlayOneShot(clip);
    }

    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value; //Not currenty in-use, setting Music/Effects separately
    }

    public void ChangeMusicVolume(float value)
    {
        musicSource.volume = value;
        startingSetVolumeMusic = value;
        // SettingsManager.Instance.musicVolume = value;
    }

    public void ChangeSFXVolume(float value)
    {
        sfxSource.volume = value;
        // SettingsManager.Instance.sfxVolume = value;
    }

    public void ToggleSFX(bool toggle)
    {
        sfxSource.mute = toggle;
    }

    public void ToggleMusic(bool toggle)
    {
        musicSource.mute = toggle;
    }

    public void FadeOutAudio()
    {
        // musicSource.
        // ambientSource
        if(!fadingAudio)
        {
            startVolumeMusic = musicSource.volume;
            targetVolume = 0f;
            fadingAudio = true;
            fadingInAudio = false;
        }
    }

    public void FadeInAudio()
    {
        if(!fadingAudio)
        {
            startVolumeMusic = musicSource.volume;
            targetVolume = startingSetVolumeMusic;
            fadingAudio = true;
            fadingInAudio = true;
        }
        else
        {
            //Cancel Fade out
            fadingAudio = false;
            FadeInAudio();
        }
    }
}
