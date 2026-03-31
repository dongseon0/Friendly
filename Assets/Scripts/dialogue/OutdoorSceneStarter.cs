using UnityEngine;

public class OutdoorSceneStarter : MonoBehaviour
{
    [SerializeField] private string sceneId = "S00_CAR_OUTSIDE";
    [SerializeField] private string nodeId = "S00_N0";
    [SerializeField] private bool startOnlyOnce = true;

    private bool _started;

    private void Start()
    {
        TryStartStory();
    }

    public void TryStartStory()
    {
        if (startOnlyOnce && _started)
            return;

        var story = FindFirstObjectByType<dialog>();
        if (story == null) return;

        // 이미 초반 병원 문 조사 플래그가 켜졌으면
        // 초반 대사를 다시 시작하지 않음
        if (story.IsFlagTrue("hospital_locked_checked"))
            return;

        story.StartScene(sceneId, nodeId);
        _started = true;
    }
}