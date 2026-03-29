using UnityEngine;

public class RadioZoomController : MonoBehaviour
{
    private Camera mainCamera;
    public GameObject radioZoomCamera;

    [Header("Settings")]
    private bool isZoomed = false;
    private bool isPlayerNearby = false;

    void Update()
    {
        // 1. 플레이어가 근처에 있고 Z키를 눌렀을 때
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.Z))
        {
            // 2. 메인 카메라가 아직 없다면 강제로 찾아내기
            if (mainCamera == null)
            {
                FindBootstrapCamera();
            }

            if (mainCamera != null && radioZoomCamera != null)
            {
                ToggleZoom();
            }
        }

        if (isZoomed && !isPlayerNearby)
        {
            ToggleZoom();
        }
    }

    void FindBootstrapCamera()
    {
        // [방법 1] 씬 전체에서 Camera 컴포넌트 중 라디오 줌 카메라가 아닌 것 찾기
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCameras)
        {
            // 자기 자신(라디오 카메라)이 아니고, 활성화된 카메라라면 메인 카메라로 간주
            if (cam.gameObject != radioZoomCamera)
            {
                mainCamera = cam;
                Debug.Log("<color=cyan>라디오 줌: </color>" + cam.name + "-> RadioZoomCamera");
                return;
            }
        }
    }

    void ToggleZoom()
    {
        isZoomed = !isZoomed;

        if (isZoomed)
        {
            mainCamera.gameObject.SetActive(false);
            radioZoomCamera.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            radioZoomCamera.SetActive(false);
            mainCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}