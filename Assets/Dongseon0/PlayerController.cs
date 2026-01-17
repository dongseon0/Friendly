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
    }

    // Input Events

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || !isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }


    void HandleLook()
    {
        // 입력값에 따라 위치 변경 (X, Y 축 이동)
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
}
