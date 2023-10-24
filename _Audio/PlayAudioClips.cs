using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioClips : MonoBehaviour
{
    private AudioManager audioManager;
    [Header("Setup")]
    [SerializeField] private bool isPlayer = false;

    // [Space(20)]
    // [SerializeField] private AudioClip[] clip;

    [Space(10)]
    [Header("- Hit Audio -")]
    [SerializeField] private AudioClip[] blockedHit;
    [SerializeField] private AudioClip[] hitAudio;

    [Space(10)]
    [Header("- Character Audio -")]
    [SerializeField] private AudioClip[] charAttackSwing;
    [SerializeField] private AudioClip[] charFootsteps;


    void Start()
    {
        audioManager = AudioManager.Instance;
    }

    public void PlayClip(int index)
    {
        // audioManager.PlaySound(clip[index]);
    }

    public void PlayRandomClip()
    {
        // if(clip.Length == 0) { Debug.Log(gameObject + " has no Audio Clips"); return; }
        // int randIndex = Random.Range(0, clip.Length);
        // audioManager.PlaySound(clip[randIndex]);
    }

#region Character Audio

    public void PlayAttackSwing()
    {
        if(charAttackSwing.Length == 0) return;
        int randIndex = Random.Range(0, charAttackSwing.Length);

        if(isPlayer) audioManager.PlayAtkSound_Player(charAttackSwing[randIndex]);
        else audioManager.PlayAtkSound_Enemy(charAttackSwing[randIndex]);
    }

    public void PlayFootsteps()
    {
        if(charFootsteps.Length == 0) return;
        int randIndex = Random.Range(0, charFootsteps.Length);
        audioManager.PlayAtkSound_Player(charFootsteps[randIndex]);
    }

#endregion
//
#region Hit Audio

    public void PlayHitAudio()
    {
        if(hitAudio.Length == 0) return;
        int randIndex = Random.Range(0, hitAudio.Length);
        audioManager.PlayAtkSound_Player(hitAudio[randIndex]);

        if(isPlayer) audioManager.PlayAtkSound_Player(hitAudio[randIndex]);
        else audioManager.PlayAtkSound_Enemy(hitAudio[randIndex]);
    }

    public void PlayBlockedAudio()
    {
        if(blockedHit.Length == 0) return;
        int randIndex = Random.Range(0, blockedHit.Length);
        audioManager.PlayBlockSound_Player(blockedHit[randIndex]);

        if(isPlayer) audioManager.PlayAtkSound_Player(blockedHit[randIndex]);
        else audioManager.PlayAtkSound_Enemy(blockedHit[randIndex]);
    }

#endregion

}
