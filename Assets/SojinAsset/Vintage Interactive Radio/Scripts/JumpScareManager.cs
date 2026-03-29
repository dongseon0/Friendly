using UnityEngine;
using System.Collections;

public class JumpScareManager : MonoBehaviour
{
    [Header("Scare Target")]
    public GameObject monsterObject;    // 씬에 배치한 Quad
    public AudioSource scareSound;      // 효과음

    [Header("Settings")]
    public float displayDuration = 1.0f; // 나타나 있을 시간

    void Start()
    {
        // 게임 시작 시 Quad 비활성화
        if (monsterObject != null)
            monsterObject.SetActive(false);
    }

    public void TriggerMonsterEvent()
    {
        if (monsterObject == null) return;

        // 이미 켜져 있다면 중복 실행 방지
        if (monsterObject.activeSelf) return;

        StartCoroutine(MonsterRoutine());
    }

    IEnumerator MonsterRoutine()
    {
  
        monsterObject.SetActive(true);
        if (scareSound != null) scareSound.Play();

        yield return new WaitForSeconds(displayDuration);
        monsterObject.SetActive(false);
    }
}