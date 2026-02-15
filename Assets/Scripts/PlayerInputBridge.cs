using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBridge : MonoBehaviour
{
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.OnPause(ctx);
    }
}
