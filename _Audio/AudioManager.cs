using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Settings")]
    [SerializeField] public bool toggleSFX;
    [SerializeField] public bool toggleMusic;
    [SerializeField] public PlayMusic playMusic;
    [Header("Audio Sources")]
    [SerializeField] public AudioSource musicSource, musicSource2, ambientSource, generalSfxSource;

    [Space(10)]
    [Header("Player SFX")]
    [SerializeField] public AudioSource source_PlayerAtkAudio;
    [SerializeField] public AudioSource source_PlayerHitAudio, source_PlayerBlockAudio, source_PlayerDeath;
    [Space(10)]
    [Header("Enemy SFX")]
    [SerializeField] public AudioSource source_EnemyAtkAudio;
    [SerializeField] public AudioSource source_EnemyHitSound, source_EnemyBlock, source_EnemyDeath;
    //TODO: add Ambient sound source
    [SerializeField] private float player_hitAudioCD = .1f;
    [SerializeField] private float player_blockAudioCD = .1f;
    [SerializeField] private float hitAudioCD = .1f;
    [SerializeField] private float deathAudioCD = .1f;
    [SerializeField] private float blockAudioCD = .1f;
    // [Space(5)]
    // [Header("Debugging SFX")]
    private float player_hitAudioCDTimer;
    private float player_blockAudioCDTimer;
    private float hitAudioCDTimer;
    private float deathAudioCDTimer;
    private float blockAudioCDTimer;

    [Space(20)]
    [Header("Audio Fade Settings")]
    [SerializeField] public float fadeDuration = 2f; //1.5

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
        musicSource2.volume = musicSource.volume;
        startVolumeAmbient = ambientSource.volume;

        startingSetVolumeMusic = startVolumeMusic;

        fadeTimer = 0;

        player_hitAudioCDTimer = 0;
        player_blockAudioCDTimer = 0;

        hitAudioCDTimer = 0;
        deathAudioCDTimer = 0;
        blockAudioCDTimer = 0;
    }

    void Update()
    {
        AudioTimers();

        if (fadingAudio)
        {
            if (fadeTimer < fadeDuration)
            {
                musicSource.volume = Mathf.Lerp(startVolumeMusic, targetVolume, fadeTimer / fadeDuration);
                musicSource2.volume = Mathf.Lerp(startVolumeMusic, targetVolume, fadeTimer / fadeDuration);
                fadeTimer += Time.unscaledDeltaTime;
            }
            else
            {
                musicSource.volume = targetVolume;
                musicSource2.volume = targetVolume;
                fadingAudio = false;
                fadeTimer = 0f;
            }
        }
    }

    private void AudioTimers()
    {
        //Added cooldown timers to prevent audio from stacking
        if(player_hitAudioCDTimer > 0) player_hitAudioCDTimer -= Time.deltaTime;
        if(player_blockAudioCDTimer > 0) player_blockAudioCDTimer -= Time.deltaTime;
        if(hitAudioCDTimer > 0) hitAudioCDTimer -= Time.deltaTime;
        if(deathAudioCDTimer > 0) deathAudioCDTimer -= Time.deltaTime;
        if(blockAudioCDTimer > 0) blockAudioCDTimer -= Time.deltaTime;
    }

    public void PlaySound()
    {
        return; //TEMP until replaced
    }

#region Player SFX
    public void PlayAtkSound_Player(AudioClip clip)
    {
        //Check if sound is already playing, stop current sound then play new one
        // if(source_PlayerAtkAudio.isPlaying) source_PlayerAtkAudio.Stop();
        source_PlayerAtkAudio.PlayOneShot(clip);
    }
    
    public void PlayHitSound_Player(AudioClip clip)
    {
        if(player_hitAudioCDTimer > 0) return;
        player_hitAudioCDTimer = player_hitAudioCD;
        source_PlayerHitAudio.PlayOneShot(clip);
    }

    public void PlayBlockSound_Player(AudioClip clip)
    {
        if(player_blockAudioCDTimer > 0) return;
        player_blockAudioCDTimer = player_blockAudioCD;
        source_PlayerBlockAudio.PlayOneShot(clip);
    }

    public void PlayDeathSound_Player(AudioClip clip)
    {
        source_PlayerDeath.PlayOneShot(clip);
    }

#endregion

#region Enemy SFX
    public void PlayAtkSound_Enemy(AudioClip clip)
    {
        source_EnemyAtkAudio.PlayOneShot(clip);
    }

    public void PlayHitSound_Enemy(AudioClip clip)
    {
        if(hitAudioCDTimer > 0) return;
        hitAudioCDTimer = hitAudioCD;
        source_EnemyHitSound.PlayOneShot(clip);
    }

    public void PlayBlockSound_Enemy(AudioClip clip)
    {
        if(blockAudioCDTimer > 0) return;
        blockAudioCDTimer = blockAudioCD;
        source_EnemyBlock.PlayOneShot(clip);
    }

    public void PlayDeathSound_Enemy(AudioClip clip)
    {
        if(deathAudioCDTimer > 0) return;
        deathAudioCDTimer = deathAudioCD;
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
        musicSource2.volume = value;
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
        musicSource2.mute = toggle;
    }

    public void FadeOutAudio()
    {
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
            startVolumeMusic = 0f;
            targetVolume = startingSetVolumeMusic; //setVolume is set in volume settings
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

    public void ResetAudioFade()
    {
        // fadingAudio = false;
        // startVolumeMusic = 0;
        // fadeTimer = 0f;
    }
#endregion
}
