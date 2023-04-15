using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public int goldAmount;

    [Header("References")]
    [SerializeField] TextMeshProUGUI goldCount;

    private void Start()
    {
        if (goldCount != null) UpdateGold();
    }

    public void GiveGold(int amount)
    {
        goldAmount += amount;
        if (goldCount != null) UpdateGold();
    }

    private void UpdateGold()
    {
        goldCount.text = goldAmount.ToString();
    }
}
