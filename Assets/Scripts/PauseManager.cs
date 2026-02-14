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
    public static PauseManager Instance; // 중복 방지용 싱글톤

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // 중복이면 삭제
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetPaused(false);
    }

     void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 바뀌면 새 PlayerInput 다시 잡기
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) playerInput = player.GetComponent<PlayerInput>();
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
