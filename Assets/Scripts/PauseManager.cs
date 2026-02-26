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
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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
        // PausePanel 바인딩은 씬 상관없이 한번 잡아두기(비활성 포함)
        if (pauseCanvasRoot == null)
        {
            var panel = Resources.FindObjectsOfTypeAll<Transform>()
                .FirstOrDefault(t => t.name == "PausePanel" && t.gameObject.scene.isLoaded);
            if (panel != null) pauseCanvasRoot = panel.gameObject;
        }

        // - 게 임플레이 씬 -
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) playerInput = player.GetComponent<PlayerInput>();

        // 씬 들어올 때는 무조건 정상화
        SetPaused(false);
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        SetPaused(!isPaused);
    }

    
    // 게임 플레이 씬인지 확인하는 함수
    // 리턴 : 불리언 값
    bool IsGameplayScene()
    {
        var n = SceneManager.GetActiveScene().name;
        return n != "TitleScene" && n != "BootstrapScene";
    }


    void SetPaused(bool paused)
    {
        isPaused = paused;

        // 게임 플레이 씬인지 확인
        bool gameplay = IsGameplayScene();

        if (pauseCanvasRoot)
            pauseCanvasRoot.SetActive(paused && gameplay);

        Time.timeScale = paused && gameplay ? 0f : 1f;

        if (!gameplay)
        {
            // 타이틀 씬이라면 커서 보이게, 커서 락 안 걸기
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        // 게임 플레이 중엔
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
