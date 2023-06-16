using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIconHandler : MonoBehaviour
{
    //Similar to DoorConnectCheck.cs
    [SerializeField] GameObject icon;
    [SerializeField] LayerMask originLayer;
    [SerializeField] bool originUp, originLeft, originDown, originRight;
    private float xDistance = 5.5f;
    private float yDistance = 3f;
    [SerializeField] private bool connectedToStart = false;

    void Start()
    {
        GetIcons();
    }

    // public void ToggleIcon(bool toggle)
    // {
    //     icon.SetActive(toggle);
    // }

    void Update()
    {
        RaycastChecks();
    }

    void GetIcons()
    {
        Invoke("GetIconsCO", .01f);
    }

    void GetIconsCO()
    {
        if(originLeft)
        {
            RaycastHit2D originFoundLeft = Physics2D.Raycast(transform.position, Vector3.left, xDistance, originLayer);
            var doorManage = originFoundLeft.collider.GetComponent<DoorManager>();
            if(doorManage != null)
            {
                doorManage.AddIconToList(icon);
                if(doorManage.startingRoom) connectedToStart = true; //If wall is connected to start, never disable
            }
        }
        if(originRight)
        {
            RaycastHit2D originFoundRight = Physics2D.Raycast(transform.position, Vector3.right, xDistance, originLayer);
            var doorManage = originFoundRight.collider.GetComponent<DoorManager>();
            if(doorManage != null) 
            {
                doorManage.AddIconToList(icon);
                if(doorManage.startingRoom) connectedToStart = true;
            }
        }
        if(originUp)
        {
            RaycastHit2D originFoundUp = Physics2D.Raycast(transform.position, Vector3.up, yDistance, originLayer);
            var doorManage = originFoundUp.collider.GetComponent<DoorManager>(); 
            if(doorManage != null)
            {
                doorManage.AddIconToList(icon);
                if(doorManage.startingRoom) connectedToStart = true;
            }
        }
        if(originDown)
        {
            RaycastHit2D originFoundDown = Physics2D.Raycast(transform.position, Vector3.down, yDistance, originLayer);
            var doorManage = originFoundDown.collider.GetComponent<DoorManager>(); 
            if(doorManage != null) 
            {
                doorManage.AddIconToList(icon);
                if(doorManage.startingRoom) connectedToStart = true;
            }
        }
        Invoke("Disable", .5f); //Disable script, as it's no longer needed
    }

    private void Disable()
    {
        if(!connectedToStart) icon.SetActive(false);
        Invoke("DisableScript", .1f);
    }

    private void DisableScript()
    {
        enabled = false;
    }

    private void RaycastChecks()
    {
        //Bools to check for connecting Doors (including shared doors)
        originLeft = Physics2D.Raycast(transform.position, Vector3.left, xDistance, originLayer);
        originRight = Physics2D.Raycast(transform.position, Vector3.right, xDistance, originLayer);
        originUp = Physics2D.Raycast(transform.position, Vector3.up, yDistance, originLayer);
        originDown = Physics2D.Raycast(transform.position, Vector3.down, yDistance, originLayer);
    }
}
