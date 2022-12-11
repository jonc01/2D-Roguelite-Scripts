using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private GameObject RunVFX;
    [SerializeField] private GameObject LandingVFX; //reusing for Jump
    [SerializeField] private GameObject JumpVFX; //^
    [SerializeField] private GameObject DashVFX;
    [SerializeField] private GameObject StopVFX;

    float jumpVFXDuration = .417f;
    float runVFXDuration = .417f;
    float dashVFXDuration = .5f;
    float stopVFXDuration = .417f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunFX(Transform spawnPos, bool facingRight = true)
    {
        //Needs to spawn in the direction the player is facing.
        //P -> Spawn normal, <- P Spawn Flipped
        if (RunVFX == null) return;
        GameObject g;
        if(facingRight) g = Instantiate(RunVFX, spawnPos.position, Quaternion.identity, transform);
        else g = Instantiate(RunVFX, spawnPos.position, spawnPos.rotation * Quaternion.Euler(0, 180f, 0), transform);

        StartCoroutine(DeleteObject(g, runVFXDuration));
    }

    public void JumpFX(Transform spawnPos)
    {
        if (JumpVFX == null) return;
        GameObject g = Instantiate(JumpVFX, spawnPos.position, Quaternion.identity, transform);
        StartCoroutine(DeleteObject(g, jumpVFXDuration));
    }

    public void StopFX(Transform spawnPos, bool facingRight = true)
    {
        //Needs to spawn in the direction the player is facing.
        //P -> Spawn normal, <- P Spawn Flipped
        if(StopVFX == null) return;
        GameObject g;
        if(facingRight) g = Instantiate(StopVFX, spawnPos.position, Quaternion.identity, transform);
        else g = Instantiate(StopVFX, spawnPos.position, spawnPos.rotation * Quaternion.Euler(0, 180f, 0), transform);

        StartCoroutine(DeleteObject(g, stopVFXDuration));
    }

    public void DashFX(Transform spawnPos, bool facingRight = true)
    {
        //Needs to spawn in the direction the player is facing.
        //P -> Spawn normal, <- P Spawn Flipped
        if (DashVFX == null) return;
        GameObject g;
        if (facingRight) g = Instantiate(DashVFX, spawnPos.position, Quaternion.identity, transform);
        else g = Instantiate(DashVFX, spawnPos.position, spawnPos.rotation * Quaternion.Euler(0, 180f, 0), transform);

        StartCoroutine(DeleteObject(g, dashVFXDuration));
    }

    IEnumerator DeleteObject(GameObject g, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(g);
    }
}
