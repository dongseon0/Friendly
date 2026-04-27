using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class HorrorDirector : Agent
{
    // 관찰할 데이터들 (마우스, 마이크 등 - 나중에 수집기랑 연결)
    public float mousePanicValue;
    public float micVolumeValue;

    void Update()
    {
        // 수집기에서 실시간으로 데이터를 가져옴
        if (SignalCollector.Instance != null)
        {
            mousePanicValue = SignalCollector.Instance.currentMouseDelta;
            micVolumeValue = SignalCollector.Instance.currentDecibel;
        }
    }

    // AI가 상황을 보는 눈
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(mousePanicValue);
        sensor.AddObservation(micVolumeValue);
        // 여기에 1부에서 파악한 플레이어 성향 벡터를 추가할 예정
    }

    // AI가 결정을 내리는 부분
    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        // AI가 선택한 번호에 따라 ScareManager의 함수 호출
        switch (action)
        {
            case 1: ScareManager.Instance.CallMannequin(); break;
            case 2: ScareManager.Instance.CallRedLights(); break;
            case 3: ScareManager.Instance.CallJumpScare(); break;
            case 4: ScareManager.Instance.CallScareSound(); break;
            case 0: default: break; // 아무것도 안 함
        }

        // 보상 로직 (연출 후 플레이어가 반응하면 점수 부여)
        if (mousePanicValue > 0.5f || micVolumeValue > 0.5f)
        {
            AddReward(1.0f);
        }
    }
}