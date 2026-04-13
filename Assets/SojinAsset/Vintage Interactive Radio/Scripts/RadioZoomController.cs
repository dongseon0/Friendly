using UnityEngine;

public class RadioZoomController : MonoBehaviour, IInteractable
{
    private Camera mainCamera;
    public GameObject radioZoomCamera;
    private PlayerController playerController;

    [Header("Settings")]
    private bool isZoomed = false;

    public void Interact()
    {
        // 1. 메인 카메라가 없다면 찾기 (부트스트랩 대응)
        if (mainCamera == null)
        {
            FindBootstrapCamera();
        }

        // 2. 카메라가 준비되었다면 줌 전환
        if (mainCamera != null && radioZoomCamera != null)
        {
            ToggleZoom();
        }
    }

    // 줌 아웃 처리
    void Update()
    {
        if (isZoomed && Input.GetKeyDown(KeyCode.Z))
        {
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {
        isZoomed = !isZoomed; //토글

        if (isZoomed)
        {
            // [줌 인]
            mainCamera.gameObject.SetActive(false);
            radioZoomCamera.SetActive(true);

            // 마우스 커서 보임
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 플레이어 조작 끄기
            if (playerController != null) playerController.enabled = false;
        }
        else
        {
            // [줌 아웃]
            radioZoomCamera.SetActive(false);

            // 만약 플레이어 카메라가 비활성화된 상태라 못 찾는 경우를 대비해 다시 확인
            if (mainCamera == null || !mainCamera.gameObject.activeInHierarchy)
                FindBootstrapCamera();

            if (mainCamera != null)
            {
                mainCamera.gameObject.SetActive(true);
            }

            // 마우스 커서 다시 고정(lock)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 플레이어 조작 다시 켜기
            if (playerController != null) playerController.enabled = true;
        }
    }

    void FindBootstrapCamera()
    {
        // 씬 내의 모든 카메라를 검색
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject != radioZoomCamera)
            {
                mainCamera = cam;
                return;
            }
        }
    }
}