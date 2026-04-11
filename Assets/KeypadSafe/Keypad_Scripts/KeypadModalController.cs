//Carefully uploading this file, I should open the unity project first and making new cs file.
//address : Asset > Keypad > Scripts
using UnityEngine;
using System.Collections.Generic;

namespace NavKeypad
{
    public class KeypadModalController : MonoBehaviour
    {
        [SerializeField] private Keypad keypad; // refer to(need connecting) keypad prefab
        //Watching Event, Make closing automatically.(proper pw)
        [SerializeField] private GameObject keypadRoot; // connect keypad root(parent) prefab
        //Open/Close the Keypad(make it Visible/Invisible)
        [SerializeField] private KeypadInteractionFPV interaction; // connect FPV scripts
        //handling the validity of clicking(true/false)
        [SerializeField] private MonoBehaviour[] disableInKeypadMode; // connect disable scripts
                                                                      //managing on/off state

        //adjust the camera
        [SerializeField] private Transform viewTarget;      // keypad 앞 카메라 위치
        [SerializeField] private Transform playerCamera;    // 플레이어 카메라

        private Vector3 _savedPos;
        private Quaternion _savedRot;

        public bool IsOpen { get; private set; }
        //flag representing validity of keypad mode

        private void Awake()
        {
            AutoBindRuntimeRefs();
        }

        private void AutoBindRuntimeRefs()
        {
            if (playerCamera == null)
            {
                if (Camera.main != null) playerCamera = Camera.main.transform;
                else Debug.LogWarning("Player Camera not found");
            }

            if (disableInKeypadMode == null || disableInKeypadMode.Length == 0)
            {
                var list = new List<MonoBehaviour>();

                var playerController = FindFirstObjectByType<PlayerController>();
                if (playerController != null) list.Add(playerController);

                var inputBridge = FindFirstObjectByType<PlayerInputBridge>();
                if (inputBridge != null) list.Add(inputBridge);

                var camLook = FindFirstObjectByType<Camera>(); // 또는 CameraController
                if (camLook != null) list.Add(camLook.GetComponent<MonoBehaviour>());

                disableInKeypadMode = list.ToArray();
            }
        }

        private void OnEnable()
        {
            if(!keypad) return;
            keypad.OnAccessGranted.AddListener(Close); 
            //When entering valid pw : closing the keypad
            keypad.OnAccessDenied.AddListener(()=>{}); 
            // managing when entering invalid pw
            keypad.OnCanceled.AddListener(Close);
            //On KeypadInteractionFPV.cs, entering the esc button, return the state before.
        }

        private void OnDisable()
        {
            if(!keypad) return;
            keypad.OnAccessGranted.RemoveListener(Close);
            keypad.OnAccessDenied.RemoveListener(Close);
            keypad.OnCanceled.RemoveListener(Close);
            //Preventing overlapping Watching, Memory/Reference Problem
        }

        public void Open()  //open the keypad
        {
            IsOpen = true;

            // save the camera location
            if (playerCamera != null && viewTarget != null)
            {
                _savedPos = playerCamera.position;
                _savedRot = playerCamera.rotation;

                // move forward to camera
                playerCamera.position = viewTarget.position;
                playerCamera.rotation = viewTarget.rotation;
            }

            if (keypadRoot != null)
                keypadRoot.SetActive(true);

            if (interaction) interaction.enabled = true;

            if (disableInKeypadMode != null)
            {
                foreach (var mb in disableInKeypadMode)
                    if (mb) mb.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            IsOpen = false;

            if (keypadRoot != null)
                keypadRoot.SetActive(false);

            if (interaction) interaction.enabled = false;

            if (disableInKeypadMode != null)
            {
                foreach (var mb in disableInKeypadMode)
                    if (mb) mb.enabled = true;
            }

            // return to the location of camera before
            if (playerCamera != null)
            {
                playerCamera.position = _savedPos;
                playerCamera.rotation = _savedRot;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (keypad) keypad.ClearAndResetVisual();
        }
    }
}