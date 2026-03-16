using UnityEngine;
using NavKeypad;   // KeypadModalController namespace

public class KeypadOpen : MonoBehaviour
{
    [SerializeField] private KeypadModalController keypadModal;
    [SerializeField] private Transform keypadTransform;

    [SerializeField] private GameObject promptUI;

    [SerializeField] private float openDistance = 1.5f; // Distance within which the player can open the keypad

    //Based on the distance to the keypad, you can decide to open the keypad or not.
    //For simplicity, we will just open it when the player presses the "Z" key.

    void Update()
    {
        float dist = Vector3.Distance(transform.position, keypadTransform.position);
        if (dist <= openDistance)
        {
            promptUI.SetActive(true); // Show the prompt to the player
            if(Input.GetKeyDown(KeyCode.Z)) keypadModal.Open();
        }
        else
        {
            promptUI.SetActive(false); // Hide the prompt when the player is too far
        }
    }
}