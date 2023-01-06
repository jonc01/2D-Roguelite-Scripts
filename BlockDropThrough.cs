using UnityEngine;

public class BlockDropThrough : MonoBehaviour
{
    //! - Make sure this object layer is set to "Ignore Raycast"
    //! - Set Tag to "SolidPlatform"
    [Tooltip("Tag: 'SolidPlatform', Layer: 'Ignore Raycast'")]
    [SerializeField] string README = "! - Set tag to 'SolidPlatform'" +
     "Set Layer to 'Ignore Raycast'";


    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if(collision.GetComponent<Base_PlayerMovement>() != null)
    //         collision.GetComponent<Base_PlayerMovement>().dropThroughBlocked = true;
    // }

    // private void OnTriggerExit2D(Collider2D collision)
    // {
    //     if (collision.GetComponent<Base_PlayerMovement>() != null)
    //         collision.GetComponent<Base_PlayerMovement>().dropThroughBlocked = false;
    // }
}
