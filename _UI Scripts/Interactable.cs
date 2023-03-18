using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] public GameObject textPrompt;
    [SerializeField] public bool isInRange;
    [SerializeField] public UnityEvent interactAction;

    void Start()
    {
        textPrompt.SetActive(false);
    }

    void Update()
    {
        if (!isInRange) return;
        
        if (Input.GetButtonDown("Interact"))
        {
            interactAction.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isInRange = true;
            //Debug.Log("Interactable script: in range");
            textPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isInRange = false;
            //Debug.Log("Interactable script: not in range");
            textPrompt.SetActive(false);
        }
    }

}
