using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BlockDropThrough : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Base_PlayerMovement>() != null)
            collision.GetComponent<Base_PlayerMovement>().canDropThrough = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Base_PlayerMovement>() != null)
            collision.GetComponent<Base_PlayerMovement>().canDropThrough = true;
    }
}
