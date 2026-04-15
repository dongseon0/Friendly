using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SafeModalController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button backgroundCloseButton;
    [SerializeField] private TMP_Text selectedText;

    [Header("Digit Buttons")]
    [SerializeField] private Button[] digitButtons; // 0~9 또는 필요한 숫자만
    [SerializeField] private Image[] digitHighlights; // 선택 시 강조용, 없으면 비워도 됨

    [Header("Pause / Disable")]
    [SerializeField] private MonoBehaviour[] disableWhileOpen;

    [Header("Optional - Camera Focus")]
    [SerializeField] private Transform viewTarget;
    [SerializeField] private Transform playerCamera;

    private Vector3 _savedPos;
    private Quaternion _savedRot;

    private readonly HashSet<int> _selectedDigits = new();
    private string _expectedCode;
    private bool _isOpen;

    private Action _onSuccess;
    private Action _onFail;
    private Action _onCancel;

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        AutoBindPanelRefs();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (backgroundCloseButton != null)
            backgroundCloseButton.onClick.AddListener(Cancel);

        RefreshVisual();
    }

    private void AutoBindRuntimeRefs()
    {
        if (playerCamera == null)
        {
            playerCamera = FindRuntimePlayerCamera();

            if (playerCamera == null)
                Debug.LogWarning("[SafeModalController] Player camera not found.");
        }

        if (disableWhileOpen == null || disableWhileOpen.Length == 0)
        {
            var list = new List<MonoBehaviour>();

            var playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
                list.Add(playerController);

            var inputBridge = FindFirstObjectByType<PlayerInputBridge>();
            if (inputBridge != null)
                list.Add(inputBridge);

            // 현재 오브젝트 자신은 넣지 않음
            disableWhileOpen = list.ToArray();
        }
    }

    private Transform FindRuntimePlayerCamera()
    {
        if (playerCamera != null)
            return playerCamera;

        var playerController = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (playerController != null && playerController.cameraTransform != null)
            return playerController.cameraTransform;

        var cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in cameras)
        {
            if (c == null) continue;
            if (!c.gameObject.scene.isLoaded) continue;
            if (!c.isActiveAndEnabled) continue;
            return c.transform;
        }

        if (Camera.main != null)
            return Camera.main.transform;

        return null;
    }

    private void Update()
    {
        if (!_isOpen) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Submit();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
            return;
        }
    }

    public void Open(string expectedCode, Action onSuccess, Action onFail, Action onCancel)
    {
        if(_isOpen) return;

        AutoBindPanelRefs();
        AutoBindRuntimeRefs();

        _expectedCode = NormalizeCode(expectedCode);
        _onSuccess = onSuccess;
        _onFail = onFail;
        _onCancel = onCancel;

        _selectedDigits.Clear();
        _isOpen = true;

        if (playerCamera != null && viewTarget != null)
        {
            _savedPos = playerCamera.position;
            _savedRot = playerCamera.rotation;

            Debug.Log($"[SafeModalController] Open -> playerCamera={playerCamera}, viewTarget={viewTarget}");

            playerCamera.position = viewTarget.position;
            playerCamera.rotation = viewTarget.rotation;
        }

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (disableWhileOpen != null)
        {
            foreach (var mb in disableWhileOpen)
                if (mb != null)
                    mb.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshVisual();
    }

    public void Close()
    {
        _isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (disableWhileOpen != null)
        {
            foreach (var mb in disableWhileOpen)
                if (mb != null)
                    mb.enabled = true;
        }

        if (playerCamera != null)
        {
            playerCamera.position = _savedPos;
            playerCamera.rotation = _savedRot;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _selectedDigits.Clear();
        RefreshVisual();
    }

    public void ToggleDigit(int digit)
    {
        if (!_isOpen) return;
        if (digit < 0 || digit > 9) return;

        if (_selectedDigits.Contains(digit))
            _selectedDigits.Remove(digit);
        else
            _selectedDigits.Add(digit);

        RefreshVisual();
    }

    private void Submit()
    {
        if (!_isOpen) return;

        string current = BuildCurrentCode();
        bool success = string.Equals(current, _expectedCode, StringComparison.Ordinal);

        var successCb = _onSuccess;
        var failCb = _onFail;

        Close();

        if (success) successCb?.Invoke();
        else failCb?.Invoke();
    }

    private void Cancel()
    {
        if (!_isOpen) return;

        var cancelCb = _onCancel;
        Close();
        cancelCb?.Invoke();
    }

    private string BuildCurrentCode()
    {
        // on/off 조합형: 현재 켜진 숫자를 정렬해서 이어붙임
        return string.Concat(_selectedDigits.OrderBy(x => x));
    }

    private string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;

        var digits = new List<int>();
        foreach (char c in code)
        {
            if (char.IsDigit(c))
                digits.Add(c - '0');
        }

        digits = digits.Distinct().OrderBy(x => x).ToList();
        return string.Concat(digits);
    }

    private void RefreshVisual()
    {
        if (selectedText != null)
        {
            string current = BuildCurrentCode();
            selectedText.text = string.IsNullOrEmpty(current) ? "-" : current;
        }

        if (digitHighlights != null)
        {
            for (int i = 0; i < digitHighlights.Length; i++)
            {
                if (digitHighlights[i] == null) continue;
                digitHighlights[i].enabled = _selectedDigits.Contains(i);
            }
        }
    }

    private void AutoBindPanelRefs()
    {
        if (panelRoot != null &&
            backgroundCloseButton != null &&
            selectedText != null &&
            digitButtons != null && digitButtons.Length > 0)
            return;

        var persistentRoot = GameObject.Find("[PersistentRoot]");
        if (persistentRoot == null)
        {
            Debug.LogWarning("[SafeModalController] [PersistentRoot] not found.");
            return;
        }

        var uiCanvas = persistentRoot.transform.Find("UICanvas");
        if (uiCanvas == null)
        {
            Debug.LogWarning("[SafeModalController] UICanvas not found under [PersistentRoot].");
            return;
        }

        var safePanelRootTf = uiCanvas.Find("SafePanelRoot");
        if (safePanelRootTf == null)
        {
            Debug.LogWarning("[SafeModalController] SafePanelRoot not found under UICanvas.");
            return;
        }

        panelRoot = safePanelRootTf.gameObject;

        var panelTf = safePanelRootTf.Find("Panel");
        if (panelTf == null)
        {
            Debug.LogWarning("[SafeModalController] Panel not found under SafePanelRoot.");
            return;
        }

        if (backgroundCloseButton == null)
        {
            var bgTf = safePanelRootTf.Find("DimBackground");
            if (bgTf != null)
                backgroundCloseButton = bgTf.GetComponent<Button>();
        }

        if (selectedText == null)
        {
            var selectedTf = panelTf.Find("SelectedCodeText");
            if (selectedTf != null)
                selectedText = selectedTf.GetComponent<TMP_Text>();
        }

        if (digitButtons == null || digitButtons.Length == 0)
        {
            var digitsTf = panelTf.Find("Digits");
            if (digitsTf != null)
            {
                digitButtons = digitsTf.GetComponentsInChildren<Button>(true);
            }
        }

        if (digitHighlights == null || digitHighlights.Length == 0)
        {
            var digitsTf = panelTf.Find("Digits");
            if (digitsTf != null)
            {
                var highlights = new List<Image>();
                for (int i = 1; i <= 9; i++)
                {
                    var btnTf = digitsTf.Find($"Btn{i}");
                    if (btnTf == null) continue;

                    var hlTf = btnTf.Find($"Highlight{i}");
                    if (hlTf != null)
                    {
                        var img = hlTf.GetComponent<Image>();
                        if (img != null) highlights.Add(img);
                    }
                }
                digitHighlights = highlights.ToArray();
            }
        }
    }
}