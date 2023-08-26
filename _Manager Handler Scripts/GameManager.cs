using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Space(10)]
    public bool inputAllowed;
    public bool shopOpen;
    public bool rewardOpen;
    public PauseMenu Pause;

    [Space(10)]
    [Header("- Player References -")]
    public Transform playerTransform;
    public Base_PlayerMovement PlayerMovement;
    public Base_PlayerCombat PlayerCombat;
    public Transform playerTargetOffset;
    public int PlayerCurrPlatform;

    [Space(20)]
    [Header("- Augments -")]
    public AugmentInventory AugmentInventory;
    public Inventory Inventory;
    public AugmentPool AugmentPool;

    private void Awake()
    {
        Instance = this;

        if (Pause == null) Pause = GameObject.Find("Menu UI Canvas").GetComponent<PauseMenu>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerMovement = playerTransform.GetComponent<Base_PlayerMovement>();
        PlayerCombat = playerTransform.GetComponent<Base_PlayerCombat>();
        playerTargetOffset = GameObject.FindGameObjectWithTag("PlayerTargetOffset").transform;
        if (Inventory == null) Inventory = GetComponent<Inventory>();

        shopOpen = false;
        rewardOpen = false;
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
        playerTransform.position = newPos;
    }

    public void TogglePlayerInput(bool toggle)
    {
        inputAllowed = toggle; //Referenced by Shop and Pause menus
        PlayerMovement.allowInput = toggle; //Direct set in Player scripts
        PlayerCombat.allowInput = toggle;
    }
}
