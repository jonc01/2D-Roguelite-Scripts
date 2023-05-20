using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject augmentSelectMenu;

    void Start()
    {
        augmentSelectMenu.SetActive(false);
    }

    public void CheckAugmentReward()
    {
        //TODO: if conditions are set, call this instead in RoomClear



        // if() ToggleRewardSelect(true); //TODO: add other Reward conditions here
    }

    public void ToggleRewardSelect(bool toggle)
    {
        //Called on room clear, call again once Augment is selected
        GameManager.Instance.rewardOpen = toggle;
        augmentSelectMenu.SetActive(toggle);


        



        //TODO: player shouldn't be able to press Esc during this menu
        // Esc input should do nothingl, but game should be paused during this (handled in AugmentSelectMenu)


        //TODO: this might not be needed, just need a way to toggle Reward screen on room Clear, AugmentSelectMenu automatically pauses game and unpauses on choosing
    }
}
