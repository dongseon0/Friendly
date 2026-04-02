using UnityEngine;

public class RadioZoomController : MonoBehaviour, IInteractable
{
    private Camera mainCamera;
    public GameObject radioZoomCamera;

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

    void ToggleZoom()
    {
        isZoomed = !isZoomed;

        if (isZoomed)
        {
            // [줌 인]
            mainCamera.gameObject.SetActive(false);
            radioZoomCamera.SetActive(true);

            // 마우스 커서 해제
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // [추가] 줌 상태에서 플레이어가 움직이지 못하게 PlayerController를 잠시 끌 수도 있습니다.
            // 필요하다면: mainCamera.GetComponentInParent<PlayerController>().enabled = false;
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

            // 마우스 커서 다시 고정
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void FindBootstrapCamera()
    {
        // 씬 내의 모든 카메라를 검색
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCameras)
        {
            // 라디오 카메라가 아니고, 플레이어 시스템에서 사용될만한 카메라 찾기
            // (보통 부트스트랩 카메라는 이름에 'Camera'가 들어있거나 특정 태그를 가짐)
            if (cam.gameObject != radioZoomCamera)
            {
                mainCamera = cam;
                return;
            }
        }
    }
}