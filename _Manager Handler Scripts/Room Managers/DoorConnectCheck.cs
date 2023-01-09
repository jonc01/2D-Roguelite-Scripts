using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorConnectCheck : MonoBehaviour
{
    private float xDistance = 5.5f;
    private float yDistance = 3f;
    [Header("! - Setup, choose one")]
    [SerializeField] bool VerticalDoor;
    [SerializeField] bool HorizontalDoor;

    [Space]
    [SerializeField] bool DEBUGGING;
    [SerializeField] LayerMask originLayer;
    //
    [SerializeField] bool originUp, originLeft, originDown, originRight;

    void Start()
    {
        GetDoors();
    }

    void Update()
    {
        if(VerticalDoor)
        {
            LeftRightCheck();
            if(DEBUGGING) DebugRaycastLeftRight();
        }
        if(HorizontalDoor)
        {
            UpDownCheck();
            if(DEBUGGING) DebugRaycastUpDown();
        }
    }

    public void GetDoors()
    {
        Invoke("GetDoorsCO", .01f); //delay needed for raycasts
    }

    void GetDoorsCO()
    {
        if(VerticalDoor)
        {
            if(originLeft)
            {
                RaycastHit2D originFoundLeft = Physics2D.Raycast(transform.position, Vector3.left, xDistance, originLayer);
                var doorManage = originFoundLeft.collider.GetComponent<DoorManager>();
                if(doorManage != null) doorManage.AddToList(transform.GetComponent<DoorController>());
            }
            if(originRight)
            {
                RaycastHit2D originFoundRight = Physics2D.Raycast(transform.position, Vector3.right, xDistance, originLayer);
                var doorManage = originFoundRight.collider.GetComponent<DoorManager>();
                if(doorManage != null) doorManage.AddToList(transform.GetComponent<DoorController>());
            }
        }
        if(HorizontalDoor)
        {
            if(originUp)
            {
                RaycastHit2D originFoundUp = Physics2D.Raycast(transform.position, Vector3.up, yDistance, originLayer);
                var doorManage = originFoundUp.collider.GetComponent<DoorManager>(); 
                if(doorManage != null) doorManage.AddToList(transform.GetComponent<DoorController>());
            }
            if(originDown)
            {
                RaycastHit2D originFoundDown = Physics2D.Raycast(transform.position, Vector3.down, yDistance, originLayer);
                var doorManage = originFoundDown.collider.GetComponent<DoorManager>(); 
                if(doorManage != null) doorManage.AddToList(transform.GetComponent<DoorController>());
            }
        }
        Invoke("Disable", .5f); //Disable script, as it's no longer needed
    }

    private void Disable()
    {
        enabled = false;
    }

    private void LeftRightCheck()
    {
        //Bools to check for connecting Doors (including shared doors)
        originLeft = Physics2D.Raycast(transform.position, Vector3.left, xDistance, originLayer);
        originRight = Physics2D.Raycast(transform.position, Vector3.right, xDistance, originLayer);
    }

    private void UpDownCheck()
    {
        originUp = Physics2D.Raycast(transform.position, Vector3.up, yDistance, originLayer);
        originDown = Physics2D.Raycast(transform.position, Vector3.down, yDistance, originLayer);
    }

    private void DebugRaycastUpDown()
    {
        Vector3 up = transform.TransformDirection(Vector3.up) * yDistance;
        Vector3 down = transform.TransformDirection(Vector3.down) * yDistance;
        Debug.DrawRay(transform.position, up, Color.green);
        Debug.DrawRay(transform.position, down, Color.green);
    }

    private void DebugRaycastLeftRight()
    {
        Vector3 left = transform.TransformDirection(Vector3.left) * xDistance;
        Vector3 right = transform.TransformDirection(Vector3.right) * xDistance;
        Debug.DrawRay(transform.position, left, Color.green);
        Debug.DrawRay(transform.position, right, Color.green);
    }
}
