using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnchorHandler : MonoBehaviour
{
    void Start()
    {
        //GameManager scene is loaded before this scene, set anchor one this scene is loaded
        GameManager.Instance.cameraRoomManager.SetCameraAnchor();
    }
}
