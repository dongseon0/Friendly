using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavKeypad { 
    public class KeypadInteractionFPV : MonoBehaviour
    {
        private Camera cam;

        [Header("Optional: assign a modal/controller that knows how to return to explore mode")]
        [SerializeField] private KeypadModalController modal;
        [SerializeField] private Keypad keypad;

        private void Awake() => cam = Camera.main;

        private void Update()
        {
            //1) when the keypad is open, pressing 'esc' to return
            if(modal != null && modal.IsOpen)
            {
                if(Input.GetKeyDown(KeyCode.Escape)){   //press esc
                    if(keypad !=null) keypad.RequestCancel();
                    //json S06_N3 분기 연결 위해 cancel event -> RequestCancel()
                    else modal.Close(); //return to the state before
                    return; //end the scripts.
                }
            }
            //2) by left mouse clicking, keypad numbers are entered
            var ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.TryGetComponent(out KeypadButton keypadButton))
                    {
                        keypadButton.PressButton();
                    }
                }
            }
        }
    }
}