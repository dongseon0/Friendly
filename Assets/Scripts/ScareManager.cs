using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScareManager : MonoBehaviour
{
    public static ScareManager Instance;

    // ──────────────────────────────────────
    // 1. 마네킹
    // ──────────────────────────────────────
    [Header("1. 마네킹")]
    public List<MannequinTrigger> mannequins;

    // ──────────────────────────────────────
    // 2. 조명 깜빡임
    // ──────────────────────────────────────
    [Header("2. 조명 깜빡임")]
    [Tooltip("ScareLight 태그 달린 조명들 자동 수집됨")]
    private List<Light> scareLights = new List<Light>();
    public float lightFlickerDuration = 1.5f;

    // ──────────────────────────────────────
    // 3. 점프스케어
    // ──────────────────────────────────────
    [Header("3. 점프스케어 - UI 이미지")]
    public Canvas jumpScareCanvas;
    public GameObject jumpScareImage; 

    [Header("3. 점프스케어 - 3D 오브젝트 (나중에 프리팹 연결)")]
    public GameObject jumpScare3D_1;
    public GameObject jumpScare3D_2;

    // ──────────────────────────────────────
    // 4. 소리
    // ──────────────────────────────────────
    [Header("4. 소리")]
    public AudioSource scareAudio;
    public AudioClip footstepSound;
    public AudioClip eerySound;
    public AudioClip laughSound;
    public AudioClip hospitalBeepSound;
    public AudioClip screamSound;

    // ──────────────────────────────────────
    // 5. 문
    // ──────────────────────────────────────
    [Header("5. 문")]
    public List<DoorInteractable> doors;

    // ──────────────────────────────────────
    // 6. 그림 글리치
    // ──────────────────────────────────────
    [Header("6. 그림 글리치")]
    public PictureGlitchManager pictureGlitchManager;

    // ──────────────────────────────────────
    // 상태
    // ──────────────────────────────────────
    [Header("상태")]
    public bool isActing = false;

    // 플레이어 참조 (위치 기반 체크용)
    private Transform playerTransform;
    public float nearbyCheckRadius = 8f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (jumpScareImage != null) jumpScareImage.SetActive(false);
        if (jumpScare3D_1 != null) jumpScare3D_1.SetActive(false);
        if (jumpScare3D_2 != null) jumpScare3D_2.SetActive(false);

        AssignCameraToCanvas();
    }

    void Start()
    {
        // ScareLight 태그 달린 조명 자동 수집
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("ScareLight");
        foreach (var obj in lightObjects)
        {
            Light l = obj.GetComponent<Light>();
            if (l != null) scareLights.Add(l);
        }
        Debug.Log($"[ScareManager] ScareLight {scareLights.Count}개 수집");

        // 플레이어 자동 바인딩
        StartCoroutine(BindPlayerNextFrame());
    }

    IEnumerator BindPlayerNextFrame()
    {
        yield return null;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) playerTransform = pc.transform;
    }

    public void AssignCameraToCanvas()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && jumpScareCanvas != null)
        {
            jumpScareCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            jumpScareCanvas.worldCamera = mainCam;
            jumpScareCanvas.planeDistance = 0.3f;
            jumpScareCanvas.sortingOrder = 999;
        }
    }

    // ──────────────────────────────────────
    // 위치 기반 체크 함수들
    // ──────────────────────────────────────

    // 주변에 조명 있는지
    public bool HasNearbyLight()
    {
        if (playerTransform == null) return false;
        foreach (var l in scareLights)
        {
            if (l == null) continue;
            if (Vector3.Distance(playerTransform.position, l.transform.position)
                <= nearbyCheckRadius)
                return true;
        }
        return false;
    }

    // 주변에 마네킹 있는지 (isPlayerNearby 활용)
    public bool HasNearbyMannequin()
    {
        foreach (var m in mannequins)
            if (m != null && m.IsPlayerNearby) return true;
        return false;
    }

    // 잠금 해제된 문 있는지
    public bool HasUnlockedDoor()
    {
        foreach (var d in doors)
            if (d != null && !d.GetIsLocked() && !d.IsMoving)
                return true;
        return false;
    }

    // 그림 글리치 가능한지 (플레이어가 방 안)
    public bool CanTriggerGlitch()
    {
        return pictureGlitchManager != null
            && pictureGlitchManager.isPlayerInRoom;
    }

    // ──────────────────────────────────────
    // AI가 호출할 연출 함수들
    // ──────────────────────────────────────

    // Action 1: 마네킹 움찔
    public void CallMannequin()
    {
        if (isActing) return;
        if (!HasNearbyMannequin()) return;

        // 주변 마네킹 중 랜덤 선택
        List<MannequinTrigger> nearby = new List<MannequinTrigger>();
        foreach (var m in mannequins)
            if (m != null && m.IsPlayerNearby) nearby.Add(m);

        if (nearby.Count == 0) return;
        nearby[Random.Range(0, nearby.Count)].ActivateScare();
    }

    // Action 2: 조명 깜빡임
    public void CallRedLights()
    {
        if (isActing) return;
        if (!HasNearbyLight()) return;
        StartCoroutine(LightFlickerRoutine());
    }

    // Action 3: 점프스케어 (UI 이미지 + 3D 오브젝트 통합, 랜덤 선택)
    public void CallJumpScare()
    {
        if (isActing) return;
        if (jumpScareCanvas.worldCamera == null) AssignCameraToCanvas();

        // UI 이미지와 3D 오브젝트 합쳐서 랜덤 선택
        // UI 이미지 1개 + 3D 2개 중 랜덤 선택
        // 3D 오브젝트 없으면 UI 이미지만
        List<int> options = new List<int>();
        if (jumpScareImage != null) options.Add(0); // UI 이미지
        if (jumpScare3D_1 != null) options.Add(1); // 3D 오브젝트 1
        if (jumpScare3D_2 != null) options.Add(2); // 3D 오브젝트 2

        if (options.Count == 0) return;

        int pick = options[Random.Range(0, options.Count)];
        switch (pick)
        {
            case 0: StartCoroutine(JumpScareUIRoutine()); break;
            case 1: StartCoroutine(JumpScare3DRoutine(jumpScare3D_1)); break;
            case 2: StartCoroutine(JumpScare3DRoutine(jumpScare3D_2)); break;
        }
    }

    // Action 4: 소리 재생 (종류 랜덤)
    public void CallScareSound()
    {
        if (isActing) return;
        if (scareAudio == null) return;

        AudioClip clip = GetRandomSoundClip();
        if (clip == null) return;

        scareAudio.PlayOneShot(clip);
    }

    // Action 5: 문 열림/닫힘
    public void CallDoorScare()
    {
        if (isActing) return;
        if (!HasUnlockedDoor()) return;
        StartCoroutine(DoorScareRoutine());
    }

    // Action 6: 그림 글리치
    public void CallPictureGlitch()
    {
        if (isActing) return;
        if (!CanTriggerGlitch()) return;
        pictureGlitchManager.TriggerGlitch();
    }

    // ──────────────────────────────────────
    // 코루틴들
    // ──────────────────────────────────────

    IEnumerator LightFlickerRoutine()
    {
        isActing = true;

        // 주변 조명만 깜빡임
        List<Light> nearbyLights = new List<Light>();
        foreach (var l in scareLights)
        {
            if (l == null) continue;
            if (playerTransform == null ||
                Vector3.Distance(playerTransform.position, l.transform.position)
                <= nearbyCheckRadius)
                nearbyLights.Add(l);
        }

        float elapsed = 0f;
        while (elapsed < lightFlickerDuration)
        {
            foreach (var l in nearbyLights) l.enabled = !l.enabled;
            float interval = Random.Range(0.05f, 0.2f);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // 원래대로 복구
        foreach (var l in nearbyLights) l.enabled = true;
        isActing = false;
    }

    IEnumerator JumpScareUIRoutine()
    {
        isActing = true;
        jumpScareImage.SetActive(true);
        if (scareAudio != null && screamSound != null)
            scareAudio.PlayOneShot(screamSound);
        yield return new WaitForSeconds(0.6f);
        jumpScareImage.SetActive(false);
        isActing = false;
    }

    IEnumerator JumpScare3DRoutine(GameObject obj)
    {
        isActing = true;
        obj.SetActive(true);
        if (scareAudio != null && screamSound != null)
            scareAudio.PlayOneShot(screamSound);
        yield return new WaitForSeconds(1.0f);
        obj.SetActive(false);
        isActing = false;
    }

    IEnumerator DoorScareRoutine()
    {
        isActing = true;

        // 잠금 해제된 문 중 랜덤 선택
        List<DoorInteractable> available = new List<DoorInteractable>();
        foreach (var d in doors)
            if (d != null && !d.GetIsLocked() && !d.IsMoving) available.Add(d);

        if (available.Count > 0)
        {
            DoorInteractable door = available[Random.Range(0, available.Count)];
            if (door.IsOpen)
                door.CloseDoor();         // 열린 문 → 갑자기 닫힘
            else
                door.Interact();          // 닫힌 문 → 갑자기 열림
        }

        yield return new WaitForSeconds(1.0f);
        isActing = false;
    }

    // ──────────────────────────────────────
    // 소리 헬퍼
    // ──────────────────────────────────────

    private AudioClip GetRandomSoundClip()
    {
        // 있는 클립들만 모아서 랜덤 선택
        List<AudioClip> available = new List<AudioClip>();
        if (footstepSound != null) available.Add(footstepSound);
        if (eerySound != null) available.Add(eerySound);
        if (laughSound != null) available.Add(laughSound);
        if (hospitalBeepSound != null) available.Add(hospitalBeepSound);
        if (screamSound != null) available.Add(screamSound);

        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }

}