using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvasRoot;
    public PlayerInput playerInput;

    private bool isPaused;
    public static PauseManager Instance;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
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
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) playerInput = player.GetComponent<PlayerInput>();

        Time.timeScale = 1f;
        SetPaused(false);

        if (pauseCanvasRoot == null)
        {
            var panel = Resources.FindObjectsOfTypeAll<Transform>()
                .FirstOrDefault(t => t.name == "PausePanel" && t.gameObject.scene.isLoaded);
            if (panel != null) pauseCanvasRoot = panel.gameObject;
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
