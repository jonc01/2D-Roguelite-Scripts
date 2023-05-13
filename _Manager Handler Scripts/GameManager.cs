using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool inputAllowed;
    public PauseMenu Pause;
    public Transform Player;

    public Base_PlayerMovement PlayerMovement;
    public Base_PlayerCombat PlayerCombat;
    public Transform PlayerTargetOffset;
    public AugmentInventory AugmentInventory;
    public Inventory Inventory;
    public int PlayerCurrPlatform;
    public AugmentPool AugmentPool;

    private void Awake()
    {
        Instance = this;

        if (Pause == null) Pause = GameObject.Find("Menu UI Canvas").GetComponent<PauseMenu>();
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerMovement = Player.GetComponent<Base_PlayerMovement>();
        PlayerCombat = Player.GetComponent<Base_PlayerCombat>();
        PlayerTargetOffset = GameObject.FindGameObjectWithTag("PlayerTargetOffset").transform;
        if (Inventory == null) Inventory = GetComponent<Inventory>();

        inputAllowed = true;
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

    public void TogglePlayerInput(bool toggle)
    {
        inputAllowed = toggle; //Referenced by Shop and Pause menus
        PlayerMovement.allowInput = toggle; //Direct set in Player scripts
        PlayerCombat.allowInput = toggle;
    }
}
