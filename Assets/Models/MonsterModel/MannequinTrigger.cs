using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannequinTrigger : MonoBehaviour
{
    private Animator mannequinAnimator;

    void Start()
    {
        // 부모(마네킹 본체)의 애니메이터 가져오기
        mannequinAnimator = GetComponentInParent<Animator>();
    }

    // 1. 플레이어가 감지 범위 안으로 들어왔을 때 발작 시작
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 애니메이션 속도를 1로 올려서 발작 시작
            mannequinAnimator.SetFloat("AnimSpeed", 1f);
        }
    }

    // 2. 플레이어가 감지 범위 밖으로 나갔을 때 발작 멈춤
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 애니메이션 속도를 다시 0으로 낮춰서 멈춤
            mannequinAnimator.SetFloat("AnimSpeed", 0f);
        }
    }
}