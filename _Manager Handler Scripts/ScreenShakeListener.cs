using UnityEngine;
using Cinemachine;

public class ScreenShakeListener : MonoBehaviour
{
    //https://youtu.be/ACf1I27I6Tk
    //Code Monkey screenshake tutorial

    public static ScreenShakeListener Instance { get; private set; }

    [Header("*This uses Cinemachine, camera bounds will affect the screenshake.")]
    public string placeholderVariable = "";

    public static bool enableScreenshake = true;

    [SerializeField] float[] presetsIntensity = { .2f, .3f, .5f, 1f, 1.2f };
    [SerializeField] float[] presetsTime = { .15f, .15f, .2f, .2f, .2f }; //.15f - hits, .2f - death

    private float shakeTimer;
    private float startingShakeTimer;
    private float startingIntensity;

    private CinemachineVirtualCamera CMvcam;

    private void Awake()
    {
        Instance = this;
        CMvcam = GetComponent<CinemachineVirtualCamera>();
    }

    public void Shake(int choice = 0)
    {
        if(choice > presetsTime.Length) { Debug.Log("Out of range"); return; }

        CinemachineBasicMultiChannelPerlin cmBMCP = 
            CMvcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (enableScreenshake)
        {
            cmBMCP.m_AmplitudeGain = presetsIntensity[choice];

            startingIntensity = presetsIntensity[choice];
            shakeTimer = presetsTime[choice];
            startingShakeTimer = presetsTime[choice];
        }
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin cmBMCP =
                CMvcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            cmBMCP.m_AmplitudeGain = 
                Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / startingShakeTimer));
        }
    }
}
