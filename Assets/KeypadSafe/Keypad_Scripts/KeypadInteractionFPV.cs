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

        private void Awake()
        {
            cam = FindRuntimeCamera();
        }

        private Camera FindRuntimeCamera()
        {
            if (cam != null && cam.isActiveAndEnabled)
                return cam;

            var playerController = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (playerController != null && playerController.cameraTransform != null)
            {
                var pcCam = playerController.cameraTransform.GetComponent<Camera>();
                if (pcCam != null) return pcCam;
            }

            var cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in cameras)
            {
                if (c == null) continue;
                if (!c.gameObject.scene.isLoaded) continue;
                if (!c.isActiveAndEnabled) continue;
                return c;
            }

            return Camera.main;
        }

        private void OnEnable()
        {
            if (cam == null)
                cam = FindRuntimeCamera();
        }

        private void Update()
        {
            if (cam == null || !cam.isActiveAndEnabled)
            {
                cam = FindRuntimeCamera();
                if (cam == null) return;
            }

            if (modal == null || !modal.IsOpen) return;

            //1) when the keypad is open, pressing 'esc' to return
            if(Input.GetKeyDown(KeyCode.Escape))
            {   //press esc
                if(keypad !=null) keypad.RequestCancel();
                //json S06_N3 분기 연결 위해 cancel event -> RequestCancel()
                else modal.Close(); //return to the state before
                return; //end the scripts.
            }

            if (Input.GetMouseButtonDown(0))
            {
                //2) by left mouse clicking, keypad numbers are entered
                var ray = cam.ScreenPointToRay(Input.mousePosition);

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