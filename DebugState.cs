using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugState : MonoBehaviour
{
    [Header("Scripts to Debug")]
    Base_BossController bossController;
    [SerializeField] TextMeshProUGUI textObj;
    [SerializeField] TextMeshProUGUI textObj2;
    
    void Start()
    {
        bossController = GetComponentInParent<Base_BossController>();
        textObj = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        if(bossController == null) return;
        TextFlip();
        UpdateText();
    }

    void UpdateText()
    {
        string textUpdate;
        
        if(bossController.movement.canFlip)
        {
            textUpdate = "Flip True";
            textObj.color = Color.green;
        } 
        else
        {
            textUpdate = "Flip False";
            textObj.color = Color.red;
        }
        
        textObj.text = textUpdate;

        if(textObj2 != null)
        {
            //
        }
    }

    void TextFlip()
    {
        //Flipping Text so it remains in correct orientation as character sprite flips
        if (bossController.movement.isFacingRight) textObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        else textObj.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }
}
