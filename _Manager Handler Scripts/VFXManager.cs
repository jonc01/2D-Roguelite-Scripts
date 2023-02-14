using System.Collections;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private VFXHandler RunVFX;
    [SerializeField] private VFXHandler JumpVFX;
    [SerializeField] private VFXHandler DashVFX;
    [SerializeField] private VFXHandler StopVFX;

    // float jumpVFXDuration = .417f;
    // float runVFXDuration = .417f;
    // float dashVFXDuration = .5f;
    // float stopVFXDuration = .417f;

    public void RunFX(Transform spawnTransform, bool facingRight = true)
    {
        //Needs to spawn in the direction the player is facing.
        //P -> Spawn normal, <- P Spawn Flipped
        if (RunVFX == null) return;

        if (facingRight) RunVFX.ShowEffect(spawnTransform.position, 0);
        else RunVFX.ShowEffect(spawnTransform.position, 180);
    }

    public void JumpFX(Transform spawnTransform)
    {
        if (JumpVFX == null) return;
        JumpVFX.ShowEffect(spawnTransform.position);
    }

    public void StopFX(Transform spawnTransform, bool facingRight = true)
    {
        //Needs to spawn in the direction the player is facing.
        //P -> Spawn normal, <- P Spawn Flipped
        if(StopVFX == null) return;

        if (facingRight) StopVFX.ShowEffect(spawnTransform.position, 0);
        else StopVFX.ShowEffect(spawnTransform.position, 180);
    }

    public void DashFX(Transform spawnTransform, bool facingRight = true)
    {
        //Needs to spawn in the direction the player is facing.
        //P -> Spawn normal, <- P Spawn Flipped
        if (DashVFX == null) return;

        if (facingRight) DashVFX.ShowEffect(spawnTransform.position, 0);
        else DashVFX.ShowEffect(spawnTransform.position, 180);
    }

    // IEnumerator DeleteObject(GameObject g, float duration)
    // {
    //     yield return new WaitForSeconds(duration);
    //     Destroy(g);
    // }
}
