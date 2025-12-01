/// introduction :
/// 기본 UTF-8 한글 변환 기준으로 작성. 영어 설정 시 persona, utf-8 변환 수정 필요
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;              // API 호출
using System.Threading.Tasks;   // same purpose, Gemini API async/await

using UnityEngine;
using UnityEngine.Networking;   // RestAPI
using UnityEngine.UI;           // ScrollRect

using TMPro;                    // 채팅 UI, 메세지 띄우기

public class ChatManager : MonoBehaviour{     //Unity 연결
    public GameObject npcBubblePrefab;
    public GameObject playerBubblePrefab;
    public Transform content;
    public TMP_InputField inputField;
    public Button sendButton;
    public ScrollRect scrollRect;

    //우선 예시로 이름 설정
    public string playerName = "James";
    public string npcName = "Nick";
    string triggerItemName = "Medical Chart";
    private string persona = "null";
    private float idleTimer=0.0f;
    private float idleLimit=30.0f;

/*
    void Start();
    void InitPersona(); // NPC Persona 프롬프트 초기화
    IEnumerator InitChatAfterStart();

    public async void OnPlayerSendMessage(string playerMsg, string playerName);// Player 입력 받아 처리
    string BuildPrompt(string playerMsg);                   // NPC Persona + 플레이어 메세지
    string ParseGeminiResponse(string jsonText);
    public async Task<string> SendToGeminiAPI(string msg);  // Gemini Rest API  호출
    void OnAIReplyReceived(string reply);                   // AI 응답을 채팅 UI에 표시

    void Update();                  // 무응답 감지
    async void TriggerIdleReply();  // 무응답 답변

    public void AddChatToUI(string speaker, string text);
    void ScrollToBottom();
    IEnumerator ScrollToBottomCoroutine();
*/

    //  Start()
    //   └─ InitPersona()           // 페르소나 생성         // NPC Persona 프롬프트 초기화
    //   └─ InitChatAfterStart()    // NPC가 먼저 대사 보냄
    void Start()
    {
        InitPersona();

        // 비동기 초기화 실행
        StartCoroutine(InitChatAfterStart());

        idleTimer = 0.0f;

        sendButton.onClick.AddListener(() => {
            OnPlayerSendMessage(inputField.text, playerName);
            inputField.text = "";
        });
    }

    void InitPersona() {
        persona =
            "당신은 플레이어 " + playerName + "의 어릴 적 친구이다.\n"
            + "당신의 아버지는 과거 수술 중 의료사고로 사망했으며, "
            + "당신은 그 수술을 " + playerName + "의 아버지가 집도했다고 믿고 있다.\n"
            + "당신은 그 사건에 대해 오랫동안 깊은 슬픔과 분노를 품어왔다.\n"
            + "플레이어가 주운 '" + triggerItemName + "'은 당신 아버지의 수술 기록이 적힌 의료 차트이다.\n"
            + "이를 본 당신은 복잡한 감정이 치밀어 올라, 그 감정에 따라 말한다.\n"
            + "그러나 당신은 플레이어에게 물리적 해를 가하지 않으며, "
            + "대화를 통해 자신의 감정과 진실을 확인하고자 한다.\n"
            + "당신이 원하는 것은 플레이어가 그 사건에 대해 진심어린 이해와 사과를 보여주는 것이다.\n"
            + "이 Persona를 바탕으로 한 감정적이지만 안전한 역할극 대사를 생성한다.";
    }
    
    IEnumerator InitChatAfterStart()
    {
        // 프레임 1번 쉬어 Canvas/UI 배치 완료된 상태 보장
        yield return null; 

        // 비전 캔버스 초기화 등의 대기
        yield return new WaitForSeconds(0.01f);

        // npc 첫 대사 생성
        string startPrompt = persona + "\nNPC:";
        var task = SendToGeminiAPI(startPrompt);

        while (!task.IsCompleted)
            yield return null;

        OnAIReplyReceived(task.Result);
    }

    //  OnPlayerSendMessage()   // Player 입력 받아 처리
    //   ├─ BuildPrompt()       // NPC Persona + 플레이어 메세지
    //   ├─ SendToGeminiAPI()   // Gemini Rest API  호출
    //   └─ OnAIReplyReceived() // AI 응답을 채팅 UI에 표시
    public async void OnPlayerSendMessage(string playerMsg, string playerName){
        try{
        idleTimer = 0.0f;                // 타이머 리셋
        AddChatToUI(playerName, playerMsg); // UI

        string prompt = BuildPrompt(playerMsg);         // 프롬프트 생성 메세지를 받아오기
        string reply = await SendToGeminiAPI(prompt);   // 리플라이 생성, await로 API 호출

        OnAIReplyReceived(reply);
        }
        catch (Exception e) {   //예외처리 구현
            Debug.LogError("Error OnPlayerSendMessage" + e.Message);
        }
    }

    string BuildPrompt(string playerMsg){
        return persona + "\n" + playerName + " : " + playerMsg + "\n" + npcName + " : ";
    }
    
    //  Update()                    // 무응답 감지
    //   └─ UpdateIdleTimer()
    //      └─ TriggerIdleReply()   // 무응답 답변
    //           ├─ BuildPrompt()
    //           ├─ SendToGeminiAPI()
    //           └─ OnAIReplyReceived()
    string ParseGeminiResponse(string jsonText)
    {
        // GeminiResponse 클래스를 네가 생성해야 함
        var response = JsonUtility.FromJson<GeminiResponse>(jsonText);

        if (response.candidates == null || response.candidates.Length == 0) {
            return "No response from Gemini.";
        }
        if (response.candidates[0].content?.parts == null ||
            response.candidates[0].content.parts.Length == 0) {
            return "Empty content.";
        }

        return response.candidates[0].content.parts[0].text;
    }

    public async Task<string> SendToGeminiAPI(string msg){
        //endpoint URL
        string url = 
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=YOUR_API_KEY";
        
        //json body : GeminiRequest json 구조
        GeminiRequest req = new GeminiRequest{
            contents = new GeminiContent[]{
                new GeminiContent{
                    parts = new GeminiPart[] { new GeminiPart { text = msg }}
                }
            }
        };

        string json = JsonUtility.ToJson(req);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json); //한글 변환(UTF-8)

        //UnityWebRequest 생성
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(jsonBytes);
        www.downloadHandler = new DownloadHandlerBuffer();

        //헤더 설정
        www.SetRequestHeader("Content-Type", "application/json");
        // API 키는 url parameter

        await www.SendWebRequest();

        if(www.result == UnityWebRequest.Result.Success){
            string resultText = www.downloadHandler.text;
            return ParseGeminiResponse(resultText); //응답 파싱 함수
        }
        else{
            Debug.LogError(www.error);
            return "Error : " + www.error;
        }
    }
    

    void OnAIReplyReceived(string reply){
        idleTimer = 0.0f;
        AddChatToUI(npcName, reply);
    }

    void Update(){
        idleTimer += Time.deltaTime;     //30초 대기

        if(idleTimer > idleLimit){
            TriggerIdleReply();
        }
    }

    async void TriggerIdleReply(){
        try{
            idleTimer=0.0f;
            string prompt = BuildPrompt("(침묵)");
            string reply = await SendToGeminiAPI(prompt);

            OnAIReplyReceived(reply);
        }
        catch (Exception e){
            Debug.LogError("Error TriggerIdleReply: " + e.Message);
        }
    }

    public void AddChatToUI(string speaker, string text)
    {
        // ChatBubble 프리팹 생성
        GameObject bubble;

        if (speaker == playerName)  // 오른쪽 정렬
        {
            bubble = Instantiate(playerBubblePrefab, content);
        }
        else                        // 왼쪽 정렬
        {
            bubble = Instantiate(npcBubblePrefab, content);
        }
        // 텍스트 넣기
        TMP_Text tmp = bubble.GetComponentInChildren<TMP_Text>();
        tmp.text = $"{speaker}: {text}";

        // 자동 스크롤
        ScrollToBottom();
    }

    void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }

    IEnumerator ScrollToBottomCoroutine()
    {
        // 한 프레임 대기 → Content 높이 갱신 후 적용됨
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f; // 맨 아래로 자동 스크롤
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

}