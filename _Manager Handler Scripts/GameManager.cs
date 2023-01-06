using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform Player;

    private Base_PlayerMovement PlayerMovement;
    public Transform PlayerTargetOffset;
    public Inventory Inventory;
    public int PlayerCurrPlatform;

    private void Awake()
    {
        Instance = this;

        Player = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerMovement = Player.GetComponent<Base_PlayerMovement>();
        PlayerTargetOffset = GameObject.FindGameObjectWithTag("PlayerTargetOffset").transform;
        if (Inventory == null) Inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        if(PlayerMovement.currPlatform != PlayerCurrPlatform)
        {
            PlayerCurrPlatform = PlayerMovement.currPlatform;
        }
    }

    public void MovePlayer(Vector3 newPos)
    {
        Player.position = newPos;
    }
}
