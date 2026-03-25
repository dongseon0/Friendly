using UnityEngine;
using UnityEngine.SceneManagement;
using NavKeypad;

public class KeypadOpen : MonoBehaviour
{
    [Header("Only active in this scene")]
    [SerializeField] private string keypadSceneName = "10_F1_Main";

    [Header("Auto References")]
    [SerializeField] private KeypadModalController keypadModal;
    [SerializeField] private Transform keypadTransform;
    [SerializeField] private GameObject promptUI;

    [Header("Settings")]
    [SerializeField] private float openDistance = 1.5f;

    private bool isSceneWithKeypad;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RefreshSceneState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneState();
    }

    private void RefreshSceneState()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isSceneWithKeypad = currentScene == keypadSceneName;

        // 씬 바뀔 때마다 초기화
        keypadModal = null;
        keypadTransform = null;

        FindPromptUI();
        SetPrompt(false);

        // keypad가 없는 씬이면 여기서 끝
        if (!isSceneWithKeypad)
            return;

        FindKeypadReferences();
    }

    private void FindPromptUI()
    {
        if (promptUI != null) return;

        GameObject persistentRoot = GameObject.Find("[PersistentRoot]");
        if (persistentRoot != null)
        {
            Transform t = persistentRoot.transform.Find("UICanvas/InteractText");
            if (t != null)
                promptUI = t.gameObject;
        }

        if (promptUI == null)
            Debug.LogWarning("Prompt UI could not be found at [PersistentRoot]/UICanvas/InteractText");
    }

    private void FindKeypadReferences()
    {
        if (keypadModal == null)
        {
            var modals = Object.FindObjectsByType<KeypadModalController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            if (modals.Length > 0)
                keypadModal = modals[0];
        }

        if (keypadTransform == null && keypadModal != null)
            keypadTransform = keypadModal.transform;

        if (keypadModal == null)
            Debug.LogWarning($"No KeypadModalController found in scene: {SceneManager.GetActiveScene().name}");

        if (keypadTransform == null)
            Debug.LogWarning($"No keypad transform found in scene: {SceneManager.GetActiveScene().name}");
    }

    private void Update()
    {
        if (!isSceneWithKeypad)
        {
            SetPrompt(false);
            return;
        }

        if (keypadModal == null || keypadTransform == null || promptUI == null)
            return;

        float dist = Vector3.Distance(transform.position, keypadTransform.position);

        if (dist <= openDistance)
        {
            SetPrompt(true);

            if (Input.GetKeyDown(KeyCode.Z))
                keypadModal.Open();
        }
        else
        {
            SetPrompt(false);
        }
    }

    private void SetPrompt(bool active)
    {
        if (promptUI != null && promptUI.activeSelf != active)
            promptUI.SetActive(active);
    }
}