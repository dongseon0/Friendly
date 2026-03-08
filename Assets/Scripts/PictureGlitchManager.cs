using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureGlitchManager : MonoBehaviour
{
    [Header("공통으로 바뀔 무서운 그림")]
    public Material scaryMaterial;  // 무서운 그림 마테리얼 (이것만 넣으세요!)

    [Header("시간 설정 (초)")]
    public float minNormalTime = 10f;
    public float maxNormalTime = 30f;
    public float scaryDuration = 0.5f;

    // ?? 핵심: 각 액자(Renderer)와 그 액자의 '원래 마테리얼'을 짝지어서 기억할 메모장(Dictionary)
    private Dictionary<MeshRenderer, Material> originalMaterials = new Dictionary<MeshRenderer, Material>();

    private Coroutine glitchRoutine;
    private bool isPlayerInRoom = false;

    void Start()
    {
        // 1. 맵에 있는 모든 PictureTarget 마커를 찾습니다.
        PictureTarget[] targets = FindObjectsByType<PictureTarget>(FindObjectsSortMode.None);

        foreach (PictureTarget target in targets)
        {
            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // 2. 게임 시작하자마자 각 액자의 원래 마테리얼을 메모장에 적어둡니다!
                originalMaterials.Add(renderer, renderer.material);
            }
        }
    }

    // 모든 액자를 끔찍한 그림으로 통일!
    void SetScaryPictures()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.material = scaryMaterial;
            }
        }
    }

    // 각 액자를 '메모장에 적어둔 자기 원래 그림'으로 각자 복구!
    void RestoreOriginalPictures()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.material = kvp.Value; // 기억해둔 원래 마테리얼 덮어쓰기
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerInRoom)
        {
            isPlayerInRoom = true;
            glitchRoutine = StartCoroutine(GlitchLoopRoutine());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isPlayerInRoom)
        {
            isPlayerInRoom = false;
            if (glitchRoutine != null) StopCoroutine(glitchRoutine);

            // 방에서 나가면 무조건 원래 그림들로 싹 복구
            RestoreOriginalPictures();
        }
    }

    IEnumerator GlitchLoopRoutine()
    {
        while (isPlayerInRoom)
        {
            float waitTime = Random.Range(minNormalTime, maxNormalTime);
            yield return new WaitForSeconds(waitTime);

            if (isPlayerInRoom)
            {
                // 순간적으로 다 똑같은 무서운 그림으로 변신!
                SetScaryPictures();

                yield return new WaitForSeconds(scaryDuration);

                // 다시 각자 원래 걸려있던 그림으로 감쪽같이 복구!
                RestoreOriginalPictures();
            }
        }
    }
}