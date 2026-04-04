using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;

    [Header("Animation")]
    public Animator animator;

    [Header("Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.12f;

    [Header("Interact")]
    public float interactDistance = 4f;
    public LayerMask interactLayer;
    public GameObject interactUI;          // InteractText(DDOL) 자동 바인딩 대상
    public InventoryManager inventory;     // 자동 바인딩 대상(없으면 인벤 입력 스킵)

    Rigidbody rb;
    Vector2 moveInput;
    Vector2 lookInput;
    float cameraPitch;
    bool isGrounded;
    bool isSprinting;

    bool _uiBound;
    bool _inventoryBound;
    bool _loggedMissingInteractUI;
    bool _loggedMissingCamera;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // 카메라 자동 바인딩(없으면 나중에 경고 1회)
        if (cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>(true);
            if (cam != null) cameraTransform = cam.transform;
        }

        // UI/인벤 자동 바인딩 시도
        BindDependenciesIfNeeded();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void FixedUpdate()
    {
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        float currentSpeed = isSprinting ? runSpeed : moveSpeed;

        rb.linearVelocity = new Vector3(
            move.x * moveSpeed,
            rb.linearVelocity.y,
            move.z * currentSpeed
        );

        //Animator에 Speed 값 전달
        if(animator != null)
        {
            float animSpeed = 0f; // 기본은 Idle(숨쉬기 속도 0)

            if (moveInput.magnitude > 0.1f)
            {
                animSpeed = isSprinting ? 2f : 1f;
            }
            animator.SetFloat("Speed", animSpeed);
        }
    }

    void Update()
    {
        if (cameraTransform == null)
        {
            if (!_loggedMissingCamera)
            {
                Debug.LogError("[PlayerController] cameraTransform missing. Assign in Inspector or ensure a Camera exists under Player.");
                _loggedMissingCamera = true;
            }
            return;
        }

        HandleLook();
        CheckInteractable();
    }

    // ========== Input Events ==========

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext ctx)
    {
        isSprinting = ctx.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;

        if (animator != null)
        {
            animator.SetTrigger("JumpTrigger");
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (cameraTransform == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
                interactable.Interact();
        }
    }

    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (InventoryManager.Instance == null) return;

        var input = GetComponent<PlayerInput>();
        string currentMap = input != null && input.currentActionMap != null
            ? input.currentActionMap.name
            : "";

        Debug.Log($"[PlayerController] OnInventory performed map={currentMap}");

        // Player 맵에서는 "닫혀 있을 때만" 열기 허용
        if (currentMap == "Player")
        {
            if (!InventoryManager.Instance.IsOpen)
                InventoryManager.Instance.SetOpen(true);

            return;
        }

        // UI 맵에서는 "열려 있을 때만" 닫기 허용
        if (currentMap == "UI")
        {
            if (InventoryManager.Instance.IsOpen)
                InventoryManager.Instance.SetOpen(false);

            return;
        }
    }

    // ========== Core Logic ==========

    void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void CheckInteractable()
    {
        // interactUI가 비어 있으면 자동 바인딩 재시도
        if (!_uiBound || interactUI == null)
            BindInteractUIIfNeeded();

        // 아직도 없으면 폭주 없이 스킵
        if (interactUI == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            if (hit.collider.GetComponent<IInteractable>() != null)
            {
                if (!interactUI.activeSelf) interactUI.SetActive(true);
                return;
            }
        }

        if (interactUI.activeSelf) interactUI.SetActive(false);
    }

    // ========== Dependency Binding (정석) ==========

    void BindDependenciesIfNeeded()
    {
        BindInteractUIIfNeeded();
        BindInventoryIfNeeded();
    }

    void BindInteractUIIfNeeded()
    {
        if (_uiBound && interactUI != null) return;

        // 비활성 포함해서 마커 탐색 (DDOL UICanvas 아래 InteractText에 InteractUIMarker 붙어 있어야 함)
        var marker = FindInteractUIMarkerEvenIfInactive();
        if (marker != null)
        {
            interactUI = marker.gameObject;
            _uiBound = true;
            return;
        }

        // 못 찾으면 1회만 로그(폭주 금지)
        if (!_loggedMissingInteractUI)
        {
            Debug.LogError("[PlayerController] InteractUIMarker not found. Add InteractUIMarker to PersistentRoot/UICanvas/InteractText.");
            _loggedMissingInteractUI = true;
        }
    }

    // 핵심: Resources.FindObjectsOfTypeAll => 비활성/HideInHierarchy 포함해서 찾음
    InteractUIMarker FindInteractUIMarkerEvenIfInactive()
    {
        var markers = Resources.FindObjectsOfTypeAll<InteractUIMarker>();

        for (int i = 0; i < markers.Length; i++)
        {
            var m = markers[i];
            if (m == null) continue;

            // 에셋/프리팹(씬에 없는 것) 배제: scene이 로드된 오브젝트만 사용
            if (!m.gameObject.scene.isLoaded) continue;

            return m; // 첫 번째 마커 사용
        }

        return null;
    }

    void BindInventoryIfNeeded()
    {
        if (_inventoryBound && inventory != null) return;

        // 네 프로젝트에 InventoryManager 싱글톤이 있으면 Instance로 바꿔도 됨
        inventory = FindFirstObjectByType<InventoryManager>();
        _inventoryBound = inventory != null;
    }

    // ========== Ground Check ==========

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Ground")) isGrounded = true;
    }

    void OnCollisionStay(Collision col)
    {
        if (col.collider.CompareTag("Ground")) isGrounded = true;
    }

    void OnCollisionExit(Collision col)
    {
        if (col.collider.CompareTag("Ground")) isGrounded = false;
    }
}
