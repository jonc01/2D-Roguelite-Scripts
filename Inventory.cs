using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int goldAmount;
    

    public void GiveGold(int amount)
    {
        goldAmount += amount;
    }
}
