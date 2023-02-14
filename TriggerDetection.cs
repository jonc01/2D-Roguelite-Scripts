using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetection : MonoBehaviour
{
    //! - Set this to the "DetectPlayer" Layer
    public bool objectDetected;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) //TODO: this might be redundant with Layer collision
        {
            objectDetected = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        //
        objectDetected = false;
    }
}
