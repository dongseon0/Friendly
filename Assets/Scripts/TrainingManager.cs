using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [Header("연결 오브젝트")]
    public HorrorDirector horrorDirector;
    public TrainingSignalSimulator simulator;

    [Header("에피소드 설정")]
    public int maxStepsPerEpisode = 200;
    private int _stepCount = 0;

    // 시나리오: 다양한 플레이어 유형 순환
    // (겁쟁이 / 침묵형 / 일반 / 강심장)
    // 파라미터: (micSensitivity, mouseSensitivity)
    private (float mic, float mouse, string name)[] _scenarios =
    {
        (0.9f, 0.6f, "겁쟁이"),   // 마이크 강, 마우스 중
        (0.1f, 0.8f, "침묵형"),   // 마이크 약, 마우스 강
        (0.5f, 0.5f, "일반"),     // 중간
        (0.1f, 0.1f, "강심장"),   // 반응 거의 없음
    };

    private int _scenarioIndex = 0;

    void Start()
    {
        ApplyCurrentScenario();
    }

    void Update()
    {
        _stepCount++;
        if (_stepCount >= maxStepsPerEpisode)
        {
            _stepCount = 0;
            NextScenario();
            horrorDirector.EndEpisode();
        }
    }

    void NextScenario()
    {
        _scenarioIndex = (_scenarioIndex + 1) % _scenarios.Length;
        ApplyCurrentScenario();
    }

    void ApplyCurrentScenario()
    {
        var s = _scenarios[_scenarioIndex];
        simulator.ApplyScenario(s.mic, s.mouse, s.name);
        Debug.Log($"[TrainingManager] 시나리오: {s.name}");
    }
}