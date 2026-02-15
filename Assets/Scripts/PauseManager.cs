using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseCanvasRoot;

    [Header("Input")]
    public PlayerInput playerInput;

    private bool isPaused;

    public static PauseManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // 부팅 때 무조건 정상화
        Time.timeScale = 1f;
        SetPaused(false);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 바뀌면 새 PlayerInput 다시 잡기
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) playerInput = player.GetComponent<PlayerInput>();

        // 씬 전환 직후 pause 해제
        Time.timeScale = 1f;
        SetPaused(false);

        // Pause UI 자동 연결
        if (pauseCanvasRoot == null)
        {
            var go = GameObject.Find("PausePanel");
            if (go != null) pauseCanvasRoot = go;
        }

    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        SetPaused(!isPaused);
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pauseCanvasRoot) pauseCanvasRoot.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;

        if (playerInput)
            playerInput.SwitchCurrentActionMap(paused ? "UI" : "Player");
    }

    public void OnClickResume() => SetPaused(false);

    public void OnClickToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}
