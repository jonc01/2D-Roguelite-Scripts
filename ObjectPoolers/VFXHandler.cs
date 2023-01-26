using UnityEngine;

public class VFXHandler : MonoBehaviour
{
    // Enables objects stored in pools
    // ObjectPoolerList will enable objects if available
    //  otherwise Instantiates a new object
    [Header("Pooled Objects")]
    [SerializeField]
    private ObjectPoolerList VFXPool;

    private void Start()
    {
        if (VFXPool == null) VFXPool = GetComponent<ObjectPoolerList>();
    }

    public void ShowEffect(Vector3 pos)
    {
        GameObject effect = VFXPool.GetObject();
        effect.transform.position = pos;
        effect.transform.rotation = Quaternion.identity;
        effect.SetActive(true);
    }

    public void ShowEffect(Vector3 pos, float yRotation)
    {
        GameObject effect = VFXPool.GetObject();
        effect.transform.position = pos;
        effect.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        effect.SetActive(true);
    }
}
