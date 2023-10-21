using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlatformToggle : MonoBehaviour
{
    [Header("- Indicator Setup -")]
    // [SerializeField] Animator anim;
    // [SerializeField] string animName;
    [SerializeField] SpriteRenderer mainSR;
    private Material mDefault;
    [SerializeField] private Material mWhiteFlash;
    [SerializeField] Transform centerOffset;
    [SerializeField] float spawnFXScaleX = 1;
    [SerializeField] float spawnFXScaleY = 1;
    [SerializeField] float spawnDelay = .75f;



    void OnEnable()
    {
        mDefault = mainSR.material;
        if(centerOffset == null) return;
        
        mainSR.enabled = false; //Toggle sprite renderer until spawn is done
        StartCoroutine(SpawnCO());
    }

    IEnumerator SpawnCO()
    {

        InstantiateManager.Instance.Indicator.PlayIndicator(centerOffset.position, 0, spawnFXScaleX, spawnFXScaleY);
        yield return new WaitForSeconds(0.1667f);
        mainSR.enabled = true;
        HitFlash(spawnDelay); //Display platform as white to blend in with spawning animation

    }

    private void HitFlash(float resetDelay)
    {
        mainSR.material = mWhiteFlash;
        Invoke("ResetMaterial", resetDelay);
    }

    private void ResetMaterial()
    {
        mainSR.material = mDefault;
    }
}
