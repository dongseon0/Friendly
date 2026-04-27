using UnityEngine;

public class SignalCollector : MonoBehaviour
{
    public static SignalCollector Instance;

    [Header("추적 대상")]
    public Transform targetPlayer; // 자동으로 찾을 플레이어

    [Header("마이크 감지")]
    private AudioClip microphoneInput;
    private string deviceName;
    public float currentDecibel;
    public float currentMouseDelta;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        InitMicrophone();
    }

    void Update()
    {
        // 1. 플레이어가 아직 없다면 씬에서 계속 찾음 (부트스트랩 대응)
        if (targetPlayer == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) targetPlayer = player.transform;
            return; // 찾을 때까지 아래 로직 건너뜀
        }
        else
        {
            Debug.Log("플레이어 찾음");
        }

        // 2. 마우스 움직임 (입력 시스템은 카메라 기준이므로 그대로 사용 가능)
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        currentMouseDelta = new Vector2(mouseX, mouseY).magnitude;

        // 3. 마이크 데시벨 계산
        currentDecibel = GetMaxVolume();
    }

    void InitMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0];
            microphoneInput = Microphone.Start(deviceName, true, 999, 44100);
        }
    }

    float GetMaxVolume()
    {
        float maxVol = 0;
        float[] waveData = new float[128];
        int micPos = Microphone.GetPosition(deviceName) - 128;
        if (micPos < 0) return 0;
        microphoneInput.GetData(waveData, micPos);
        foreach (var sample in waveData)
        {
            float absSample = Mathf.Abs(sample);
            if (maxVol < absSample) maxVol = absSample;
        }
        return maxVol;
    }
}