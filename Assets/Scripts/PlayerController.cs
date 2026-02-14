using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float jumpForce = 5f;

    [Header("Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.12f;

    [Header("Interact")]
    public float interactDistance = 4f;
    public LayerMask interactLayer;
    public GameObject interactUI;
    public InventoryManager inventory;


    Rigidbody rb;
    Vector2 moveInput;
    Vector2 lookInput;
    float cameraPitch;
    bool isGrounded;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        rb.linearVelocity = new Vector3(
        move.x * moveSpeed,
        rb.linearVelocity.y,
        move.z * moveSpeed
        );

    }

    void Update()
    {
        HandleLook(); // 매 프레임 이동 처리
        CheckInteractable();
    }

    // Input Events

    // WASD 움직이기
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    // 마우스로 시야 움직이기
    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    // Space바로 점프처리
    public void OnJump(InputAction.CallbackContext ctx)
    {
        Debug.Log($"Jump ctx.phase={ctx.phase} grounded={isGrounded}");
        if (!ctx.performed || !isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    // Z키로 상호작용하기
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    // X키로 인벤토리 열기
    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        inventory.ToggleInventory();
    }




    void HandleLook()
    {
        // 입력값에 따라 위치 변경 
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void CheckInteractable()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            if (hit.collider.GetComponent<IInteractable>() != null)
            {
                interactUI.SetActive(true);
                return;
            }
        }

        interactUI.SetActive(false);
    }



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
