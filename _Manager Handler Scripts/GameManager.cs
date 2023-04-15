using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PauseMenu Pause;
    public Transform Player;

    public Base_PlayerMovement PlayerMovement;
    private Base_PlayerCombat PlayerCombat;
    public Transform PlayerTargetOffset;
    public Inventory Inventory;
    public int PlayerCurrPlatform;
    public bool inputAllowed;

    private void Awake()
    {
        Instance = this;

        if (Pause == null) Pause = GameObject.Find("Menu UI Canvas").GetComponent<PauseMenu>();
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerMovement = Player.GetComponent<Base_PlayerMovement>();
        PlayerCombat = Player.GetComponent<Base_PlayerCombat>();
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

    public void TogglePlayerInput(bool toggle)
    {
        inputAllowed = toggle; //Referenced by Shop and Pause menus
        PlayerMovement.allowInput = toggle; //Direct set in Player scripts
        PlayerCombat.allowInput = toggle;
    }
}
