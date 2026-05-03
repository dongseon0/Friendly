using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureGlitchManager : MonoBehaviour
{
    [Header("공통으로 바뀔 무서운 그림")]
    public Material scaryMaterial;

    [Header("시간 설정 (초)")]
    public float scaryDuration = 0.5f;

    private Dictionary<MeshRenderer, Material> originalMaterials
        = new Dictionary<MeshRenderer, Material>();

    private Coroutine glitchRoutine;
    public bool isPlayerInRoom = false;

    void Start()
    {
        PictureTarget[] targets = FindObjectsByType<PictureTarget>(FindObjectsSortMode.None);
        foreach (PictureTarget target in targets)
        {
            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            if (renderer != null)
                originalMaterials.Add(renderer, renderer.material);
        }
    }

    // AI가 직접 호출하는 함수
    public void TriggerGlitch()
    {
        if (!isPlayerInRoom) return;
        if (glitchRoutine != null) return; // 이미 실행 중이면 무시
        glitchRoutine = StartCoroutine(SingleGlitchRoutine());
    }

    IEnumerator SingleGlitchRoutine()
    {
        SetScaryPictures();
        yield return new WaitForSeconds(scaryDuration);
        RestoreOriginalPictures();
        glitchRoutine = null;
    }

    void SetScaryPictures()
    {
        foreach (var kvp in originalMaterials)
            if (kvp.Key != null) kvp.Key.material = scaryMaterial;
    }

    void RestoreOriginalPictures()
    {
        foreach (var kvp in originalMaterials)
            if (kvp.Key != null) kvp.Key.material = kvp.Value;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInRoom = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRoom = false;
            RestoreOriginalPictures();
        }
    }
}