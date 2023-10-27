using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Settings")]
    [SerializeField] public bool toggleSFX;
    [SerializeField] public bool toggleMusic;
    [Header("Audio Sources")]
    [SerializeField] public AudioSource musicSource, ambientSource, generalSfxSource;

    [Space(10)]
    [Header("Player SFX")]
    [SerializeField] public AudioSource source_PlayerAtkAudio;
    [SerializeField] public AudioSource source_PlayerHitAudio, source_PlayerBlockAudio, source_PlayerDeath;
    [Space(10)]
    [Header("Enemy SFX")]
    [SerializeField] public AudioSource source_EnemyAtkAudio;
    [SerializeField] public AudioSource source_EnemyHitSound, source_EnemyBlock, source_EnemyDeath;
    //TODO: add Ambient sound source
    [Space(15)]
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
            Debug.Log("Destroyed " + name + " in " + SceneManager.GetActiveScene().name);
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

    public void PlaySound()
    {
        return; //TEMP until replaced
    }

#region Player SFX
    public void PlayAtkSound_Player(AudioClip clip)
    {
        //Check if sound is already playing, stop current sound then play new one
        if(source_PlayerAtkAudio.isPlaying) source_PlayerAtkAudio.Stop();
        source_PlayerAtkAudio.PlayOneShot(clip);
    }
    
    public void PlayHitSound_Player(AudioClip clip)
    {
        if(source_PlayerHitAudio.isPlaying) source_PlayerHitAudio.Stop();
        source_PlayerHitAudio.PlayOneShot(clip);
    }

    public void PlayBlockSound_Player(AudioClip clip)
    {
        if(source_PlayerBlockAudio.isPlaying) source_PlayerBlockAudio.Stop();
        source_PlayerBlockAudio.PlayOneShot(clip);
    }

    public void PlayDeathSound_Player(AudioClip clip)
    {
        if(source_PlayerDeath.isPlaying) source_PlayerDeath.Stop();
        source_PlayerDeath.PlayOneShot(clip);
    }

#endregion

#region Enemy SFX
    public void PlayAtkSound_Enemy(AudioClip clip)
    {
        if(source_EnemyAtkAudio.isPlaying) source_EnemyAtkAudio.Stop();
        source_EnemyAtkAudio.PlayOneShot(clip);
    }

    public void PlayHitSound_Enemy(AudioClip clip)
    {
        if(source_EnemyHitSound.isPlaying) source_EnemyHitSound.Stop();
        source_EnemyHitSound.PlayOneShot(clip);
    }

    public void PlayBlockSound_Enemy(AudioClip clip)
    {
        if(source_EnemyBlock.isPlaying) source_EnemyBlock.Stop();
        source_EnemyBlock.PlayOneShot(clip);
    }

    public void PlayDeathSound_Enemy(AudioClip clip)
    {
        if(source_EnemyDeath.isPlaying) source_EnemyDeath.Stop();
        source_EnemyDeath.PlayOneShot(clip);
    }

#endregion

#region Audio Settings
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
        // sfxSource.volume = value;
        // SettingsManager.Instance.sfxVolume = value;
        generalSfxSource.volume = value;

        source_PlayerAtkAudio.volume = value;
        source_PlayerHitAudio.volume = value;
        source_PlayerBlockAudio.volume = value;
        source_PlayerDeath.volume = value;

        source_EnemyAtkAudio.volume = value;
        source_EnemyHitSound.volume = value;
        source_EnemyBlock.volume = value;
        source_EnemyDeath.volume = value;
    }

    public void ToggleSFX(bool toggle)
    {
        // sfxSource.mute = toggle;

        generalSfxSource.mute = toggle;

        source_PlayerAtkAudio.mute = toggle;
        source_PlayerHitAudio.mute = toggle;
        source_PlayerBlockAudio.mute = toggle;
        source_PlayerDeath.mute = toggle;

        source_EnemyAtkAudio.mute = toggle;
        source_EnemyHitSound.mute = toggle;
        source_EnemyBlock.mute = toggle;
        source_EnemyDeath.mute = toggle;
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
#endregion
}
