using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    [Header("Generator Setup")]
    [SerializeField] float xDistance = 5.5f;
    [SerializeField] float yDistance = 3f;

    [Header("Variables")]
    [SerializeField] private bool DEBUGGING;
    [SerializeField] public bool wallGenRunning;
    [SerializeField] bool buildingWallsDoors;
    [SerializeField] public bool wallGenDone;
    [SerializeField] Transform currOrigin;

    [Header("References")]
    [SerializeField] LayerMask wallLayer;
    [SerializeField] LevelBuilder Builder;

    [Header("--- Components (*Setup) ---")]
    [Header("0 - Horizontal, 1 - Vertical")]
    [SerializeField] GameObject[] Walls; //0: Top/Bot, 1: Left/Right
    [SerializeField] GameObject[] VerticalDoors; //0: Top/Bot, 1: Left/Right
    [SerializeField] GameObject[] HorizontalDoors; //0: Top/Bot, 1: Left/Right

    [Header("Raycasts")]
    [SerializeField] bool wallFoundUp;
    [SerializeField] bool wallFoundLeft;
    [SerializeField] bool wallFoundDown;
    [SerializeField] bool wallFoundRight;

    RaycastHit2D getWallUp;
    RaycastHit2D getWallLeft;
    RaycastHit2D getWallDown;
    RaycastHit2D getWallRight;

    //Coroutine yield variables
    [SerializeField] private bool upChecked;
    [SerializeField] private bool leftChecked;
    [SerializeField] private bool downChecked;
    [SerializeField] private bool rightChecked;

    private void Start()
    {
        Builder = GetComponent<LevelBuilder>();
        wallGenDone = false;
    }

    private void Update()
    {
        if (!wallGenRunning) return; //Stop updating raycasts if not needed
        RoomConnectRaycastCheck();
        DebugRaycast();
    }

    public void GenerateWallDoors()
    {
        Debug.Log("Generating Walls...");
        wallGenDone = false;
        StartCoroutine(GenerateWallDoorsCO());
    }

    IEnumerator GenerateWallDoorsCO()
    {
        wallGenRunning = true;
        for (int i = 0; i < Builder.GeneratedOrigins.Length; i++)
        {
            buildingWallsDoors = true;

            //Move to origin at index
            currOrigin = Builder.GeneratedOrigins[i].transform;
            transform.position = currOrigin.position;
            yield return new WaitForSecondsRealtime(.01f); //Delay needed for raycasts to update to new position

            WallDoorCheck(); //Start building CO

            //Prevent loop until done walls are built
            while (buildingWallsDoors) yield return null;
        }
        wallGenRunning = false;

        yield return new WaitForSecondsRealtime(.01f);
        Debug.Log("Walls Generated");
        wallGenDone = true;
    }

    void WallDoorCheck()
    {
        StartCoroutine(WallDoorCheckCO());
    }

    IEnumerator WallDoorCheckCO()
    {
        buildingWallsDoors = true;

        upChecked = false;
        leftChecked = false;
        downChecked = false;
        rightChecked = false;

        //Check each direction for a Wall/Door and Origin
        //If the raycast hits a Wall/Door, do nothing
        //If the raycast hits an Origin, build a Door
        //Else build a Wall

        if (!wallFoundUp) ChooseWallDoor(0, !Builder.originFoundUp);
        else upChecked = true;
        if (!wallFoundLeft) ChooseWallDoor(1, !Builder.originFoundLeft);
        else leftChecked = true;
        if (!wallFoundDown) ChooseWallDoor(2, !Builder.originFoundDown);
        else downChecked = true;
        if (!wallFoundRight) ChooseWallDoor(3, !Builder.originFoundRight);
        else rightChecked = true;

        while (!upChecked && !leftChecked && !downChecked && !rightChecked) yield return null;

        //yield return new WaitForSecondsRealtime(.01f);
        buildingWallsDoors = false;
    }

    void ChooseWallDoor(int direction, bool isWall = true)
    {
        //Get current position, adjust offset and Instantiate Wall
        float x = transform.position.x;
        float y = transform.position.y;
        int index = 0;

        switch (direction)
        {
            case 0: //Up
                y += yDistance;
                index = 0;
                upChecked = true;
                break;
            case 1: //Left
                x -= xDistance;
                index = 1;
                leftChecked = true;
                break;
            case 2: //Down
                y -= yDistance;
                index = 0;
                downChecked = true;
                break;
            case 3: //Right
                x += xDistance;
                index = 1;
                rightChecked = true;
                break;
            default:
                break;
        }
        Vector3 newPos = new Vector3(x, y, 0);

        if (isWall) Instantiate(Walls[index], newPos, Quaternion.identity, currOrigin); //0: Top/Bot, 1: Left/Right
        else 
        {
            //Generate vertical or horizontal doors based on the direction being built
            if(direction == 0 || direction == 2) GenerateDoor(newPos, false);
            else GenerateDoor(newPos, true);
        }
    }

    void GenerateDoor(Vector3 newPos, bool isVertical)
    {
        int index = Random.Range(0, 3); //Change 3 to size var if making more variations

        if(isVertical) Instantiate(VerticalDoors[index], newPos, Quaternion.identity, currOrigin);
        else Instantiate(HorizontalDoors[index], newPos, Quaternion.identity, currOrigin);
    }

    #region Raycasts
    private void RoomConnectRaycastCheck()
    {
        wallFoundUp = Physics2D.Raycast(transform.position, Vector3.up, yDistance, wallLayer);
        wallFoundLeft = Physics2D.Raycast(transform.position, Vector3.left, xDistance, wallLayer);
        wallFoundDown = Physics2D.Raycast(transform.position, Vector3.down, yDistance, wallLayer);
        wallFoundRight = Physics2D.Raycast(transform.position, Vector3.right, xDistance, wallLayer);
    }

    private void DebugRaycast()
    {
        Vector3 up = transform.TransformDirection(Vector3.up) * yDistance;
        Vector3 left = transform.TransformDirection(Vector3.left) * xDistance;
        Vector3 down = transform.TransformDirection(Vector3.down) * yDistance;
        Vector3 right = transform.TransformDirection(Vector3.right) * xDistance;

        Debug.DrawRay(transform.position, up, Color.green);
        Debug.DrawRay(transform.position, left, Color.green);
        Debug.DrawRay(transform.position, down, Color.green);
        Debug.DrawRay(transform.position, right, Color.green);
    }
    #endregion
}
