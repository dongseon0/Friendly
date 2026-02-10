using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseCanvasRoot;

    [Header("Input")]
    public PlayerInput playerInput;  

    bool isPaused;

    void Start()
    {
        SetPaused(false);
    }

    // Pause 상태 (esc 누르는 경우)
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;   // Press Only면 performed 한 번만
        SetPaused(!isPaused);
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pauseCanvasRoot) pauseCanvasRoot.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;

        // 맵 스위치 (Player 맵과 UI맵)
        if (playerInput)
        {
            playerInput.SwitchCurrentActionMap(paused ? "UI" : "Player");
        }
    }

    public void OnClickResume() => SetPaused(false);

    public void OnClickToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}
