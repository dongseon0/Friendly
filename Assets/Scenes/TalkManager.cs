using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트메쉬프로 쓰려면 필수

[System.Serializable]
public struct DialogueData
{
    public string name; // 화자 이름
    [TextArea(3, 5)]
    public string content; // 대사 내용
    public Sprite portrait; // 캐릭터 표정 이미지 (없으면 비워도 됨)
}

public class TalkManager : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    public GameObject dialoguePanel; // 대화창 전체 (켜고 끄기용)
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI bodyText;
    public Image portraitImg;

    [Header("대사 데이터")]
    public DialogueData[] dialogues; // 인스펙터에서 대사 쭉 적을 곳

    private int currentIndex = 0; // 현재 몇 번째 대사인지

    void Start()
    {
        // 시작하자마자 첫 대사 보여주기
        ShowDialogue();
    }

    void Update()
    {
        // 마우스 클릭 or 스페이스바 누르면 다음 대사
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            NextDialogue();
        }
    }

    void ShowDialogue()
    {
        // 1. 패널 켜기
        dialoguePanel.SetActive(true);

        // 2. 텍스트 갈아끼우기
        nameText.text = dialogues[currentIndex].name;
        bodyText.text = dialogues[currentIndex].content;

        // 3. 이미지 갈아끼우기 (이미지가 있을 때만)
        if (dialogues[currentIndex].portrait != null)
        {
            portraitImg.sprite = dialogues[currentIndex].portrait;
            portraitImg.gameObject.SetActive(true);
        }
        else
        {
            // 이미지가 없으면 숨김
            portraitImg.gameObject.SetActive(false);
        }
    }

    void NextDialogue()
    {
        currentIndex++;

        // 대사가 더 남아있으면 갱신
        if (currentIndex < dialogues.Length)
        {
            ShowDialogue();
        }
        else
        {
            // 대사 다 끝났으면 창 닫기 (또는 씬 전환)
            Debug.Log("대화 종료");
            dialoguePanel.SetActive(false);
        }
    }
}