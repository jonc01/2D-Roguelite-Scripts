using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentInventoryDisplay : MonoBehaviour
{
    [SerializeField] public AugmentDisplay[] AugmentSlots;
    public int numSlots;

    void Start()
    {
        // if(AugmentSlots.Length == 0) return;
        ToggleSlots();
    }

    public void AddAugmentToDisplay(List<AugmentScript> augmentList)
    {
        numSlots = augmentList.Count;

        for(int i=0; i<numSlots; i++)
        {
            AugmentSlots[i].augmentScript = augmentList[i];
            augmentList[i].displayedIndex = i; //Set index value for reference
        }

        ToggleSlots();
    }

    public void ToggleSlots()
    {
        numSlots = AugmentSlots.Length;
        for(int i=0; i<numSlots; i++)
        {
            var augSlot = AugmentSlots[i].GetComponent<AugmentDisplay>();
            if(augSlot.augmentScript != null) AugmentSlots[i].gameObject.SetActive(true);
            else AugmentSlots[i].gameObject.SetActive(false);
        }
    }

    public void ToggleAugmentStatus(int currIdx, float timerDuration)
    {
        //Get Augment index in displayed Augments
        AugmentSlots[currIdx].ToggleAugmentStatus(true, timerDuration);
    }
}
