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
    [SerializeField] private AudioClip[] deathSound;
    [SerializeField] private AudioClip[] jumpSound;


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

    public void PlayJump()
    {
        if(jumpSound.Length == 0) return;
        int randIndex = Random.Range(0, jumpSound.Length);
        audioManager.PlayAtkSound_Player(jumpSound[randIndex]);
    }

#endregion
//
#region Hit Audio

    public void PlayHitAudio()
    {
        if(hitAudio.Length == 0) return;
        int randIndex = Random.Range(0, hitAudio.Length);

        if(isPlayer) audioManager.PlayHitSound_Player(hitAudio[randIndex]);
        else audioManager.PlayHitSound_Enemy(hitAudio[randIndex]);
    }

    public void PlayBlockedAudio()
    {
        if(blockedHit.Length == 0) return;
        int randIndex = Random.Range(0, blockedHit.Length);

        if(isPlayer) audioManager.PlayBlockSound_Player(blockedHit[randIndex]);
        else audioManager.PlayBlockSound_Enemy(blockedHit[randIndex]);
    }

    public void PlayDeathSound()
    {
        if(deathSound.Length == 0) return;
        int randIndex = Random.Range(0, deathSound.Length);

        if(isPlayer) audioManager.PlayDeathSound_Player(deathSound[randIndex]);
        else audioManager.PlayDeathSound_Enemy(deathSound[randIndex]);
    }


#endregion

}
