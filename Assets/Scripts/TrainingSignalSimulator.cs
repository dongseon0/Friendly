using UnityEngine;

public class TrainingSignalSimulator : MonoBehaviour
{
    public static TrainingSignalSimulator Instance;

    [Header("시뮬레이션 모드")]
    public SimulationMode mode = SimulationMode.ReactToScare;

    [Header("기본 파라미터")]
    public float baseNoise = 0.05f;
    public float scareResponsePeak = 0.8f;
    public float responseDecayRate = 3.0f;

    // SignalCollector와 동일한 인터페이스
    public float currentDecibel { get; private set; }
    public float currentMouseDelta { get; private set; }

    // 기준치는 0으로 고정 (시뮬레이터는 이미 정규화된 값 사용)
    public float baselineDecibel = 0f;
    public float baselineMouseDelta = 0f;
    public bool isBaselineReady = true;

    private float _responseMagnitude = 0f;
    private float _micSensitivity = 0.5f;
    private float _mouseSensitivity = 0.5f;

    public enum SimulationMode
    {
        RandomNoise,    // 완전 랜덤 노이즈 (초기 탐험용)
        ReactToScare,   // 연출 직후 반응 시뮬레이션 (메인)
    }

    void Awake()
    {
        Instance = this;
        // 학습 씬에서는 실제 SignalCollector 비활성화
        var realCollector = FindFirstObjectByType<SignalCollector>();
        if (realCollector != null) realCollector.enabled = false;
    }

    void Update()
    {
        switch (mode)
        {
            case SimulationMode.RandomNoise:
                SimulateRandom();
                break;
            case SimulationMode.ReactToScare:
                SimulateReactive();
                break;
        }
    }

    private void SimulateRandom()
    {
        currentMouseDelta = baseNoise + Random.Range(0f, 0.2f);
        currentDecibel = baseNoise + Random.Range(0f, 0.15f);

        // 2% 확률로 큰 반응 스파이크
        if (Random.value < 0.02f)
        {
            currentMouseDelta = Random.Range(0.6f, 1.0f);
            currentDecibel = Random.Range(0.5f, 0.9f);
        }
    }

    private void SimulateReactive()
    {
        _responseMagnitude = Mathf.Lerp(
            _responseMagnitude, 0f, Time.deltaTime * responseDecayRate);

        currentDecibel = Mathf.Clamp01(
            baseNoise + _responseMagnitude * _micSensitivity
            + Random.Range(0f, 0.03f));
        currentMouseDelta = Mathf.Clamp01(
            baseNoise + _responseMagnitude * _mouseSensitivity
            + Random.Range(0f, 0.05f));
    }

    // ScareManager_Training에서 연출 실행 시 호출
    public void TriggerResponse(float intensity = 1.0f)
    {
        _responseMagnitude = scareResponsePeak * intensity;
    }

    // TrainingManager에서 시나리오 전환 시 호출
    public void ApplyScenario(float micSens, float mouseSens, string scenarioName)
    {
        _micSensitivity = micSens;
        _mouseSensitivity = mouseSens;
        _responseMagnitude = 0f;
        Debug.Log($"[Simulator] 시나리오 변경: {scenarioName}");
    }

    // SignalCollector와 동일한 인터페이스
    public float GetNormalizedMic() => currentDecibel;
    public float GetNormalizedMouse() => currentMouseDelta;
}