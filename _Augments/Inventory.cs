using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public int goldAmount; //XPAmount

    [Header("References")]
    [SerializeField] PlayAudioClips playAudioClips;
    [SerializeField] TextMeshProUGUI goldCount;

    private void Start()
    {
        if (goldCount != null) UpdateGold();
    }

    public void UpdateGold(int amount)
    {
        goldAmount += amount;
        if (playAudioClips != null) playAudioClips.PlayHitAudio();
        if (goldCount != null) UpdateGold();
    }

    private void UpdateGold()
    {
        goldCount.text = goldAmount.ToString();
    }
}
