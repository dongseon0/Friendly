using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScareManager : MonoBehaviour
{
    public static ScareManager Instance;

    [Header("1. 마네킹 설정")]
    public MannequinTrigger mannequin; // 이전에 만든 마네킹 스크립트

    [Header("2. 얼빡샷(UI) 설정")]
    public Canvas jumpScareCanvas;
    public GameObject jumpScareImage;

    [Header("3. 사운드 설정")]
    public AudioSource scareAudio;

    [Header("4. 조명 설정")]
    public List<Light> redLights; // 빨간 조명 2개를 리스트로 관리

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 UI 비활성화 확인
        if (jumpScareImage != null) jumpScareImage.SetActive(false);

        // 카메라 자동 할당 시도
        AssignCameraToCanvas();
    }

    // 부트스트랩 대응: 카메라를 찾아서 캔버스에 꽂아주는 함수
    public void AssignCameraToCanvas()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && jumpScareCanvas != null)
        {
            jumpScareCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            jumpScareCanvas.worldCamera = mainCam;
            jumpScareCanvas.planeDistance = 0.3f;
            jumpScareCanvas.sortingOrder = 999; // 가장 앞에 나오게
        }
    }

    // --- AI가 호출할 4가지 액션 함수 ---

    // Action 1: 마네킹 움찔 (이미 짜둔 로직 호출)
    public void CallMannequin()
    {
        if (mannequin != null) mannequin.ActivateScare();
    }

    // Action 2: 빨간 조명 깜빡이기
    public void CallRedLights()
    {
        StartCoroutine(RedLightRoutine());
    }

    // Action 3: 사진 얼빡샷
    public void CallJumpScare()
    {
        // 실행 직전 카메라가 빠졌는지 다시 확인
        if (jumpScareCanvas.worldCamera == null) AssignCameraToCanvas();
        StartCoroutine(JumpScareRoutine());
    }

    // Action 4: 무서운 소리 재생
    public void CallScareSound()
    {
        if (scareAudio != null && !scareAudio.isPlaying) scareAudio.Play();
    }

    // --- 코루틴 연출 로직 ---

    IEnumerator RedLightRoutine()
    {
        foreach (var l in redLights) l.enabled = true;
        yield return new WaitForSeconds(1.0f); // 1초간 켬
        foreach (var l in redLights) l.enabled = false;
    }

    IEnumerator JumpScareRoutine()
    {
        jumpScareImage.SetActive(true);
        if (scareAudio != null) scareAudio.Play(); // 사진 뜰 때 소리도 같이
        yield return new WaitForSeconds(0.6f);
        jumpScareImage.SetActive(false);
    }
}