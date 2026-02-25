//Carefully uploading this file, I should open the unity project first and making new cs file.
//address : Asset > Keypad > Scripts
using UnityEngine;

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

        public bool IsOpen { get; private set; }
        //flag representing validity of keypad mode

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

        public void Open()  //Open the Keypad Mode
        {
            IsOpen = true;
            keypadRoot.SetActive(true); //Show keypad object
            if (interaction) interaction.enabled = true;

            foreach (var mb in disableInKeypadMode) if (mb) mb.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            IsOpen = false;
            keypadRoot.SetActive(false);
            if (interaction) interaction.enabled = false;

            foreach (var mb in disableInKeypadMode) if (mb) mb.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (keypad) keypad.ClearAndResetVisual();   //initializing
        }
    }
}