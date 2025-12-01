using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class FearSignalReader : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI decibelText;
    public TextMeshProUGUI mouseShakeText;

    private AudioClip micClip;
    private Vector2 lastMousePos;

    public float currentDB = 0f;
    public float mouseShakeAmount;

    void Start()
    {
        // 마이크 활성화
        if (Microphone.devices.Length > 0)
        {
            micClip = Microphone.Start(null, true, 1, 44100);
        }
        else
        {
            Debug.LogError("마이크 없음! 데시벨 계산 불가.");
        }
    }

    void Update()
    {
        UpdateDecibel();
        UpdateMouseShake();
    }

    // 1. 데시벨 계산
    void UpdateDecibel()
    {
        if (micClip == null) return;

        float[] samples = new float[1024];
        int micPos = Microphone.GetPosition(null) - samples.Length;

        if (micPos < 0) return;

        micClip.GetData(samples, micPos);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        float rms = Mathf.Sqrt(sum / samples.Length);

        float dB = 20 * Mathf.Log10(rms + 1e-7f); 
        currentDB = dB;


        if (decibelText)
            decibelText.text = $"Decibel_Now: {Mathf.RoundToInt(dB)} dB";
        
    }

    // 2. 마우스 흔들림 측정
    void UpdateMouseShake()
    {
        Vector2 currentMousePos = Mouse.current.delta.ReadValue();

        // 마우스 움직임 절댓값 합
        mouseShakeAmount = Mathf.Abs(currentMousePos.x) + Mathf.Abs(currentMousePos.y);

        if (mouseShakeText)
            mouseShakeText.text = $"Mouse Shake_Now: {mouseShakeAmount:F2}";
    }
}
