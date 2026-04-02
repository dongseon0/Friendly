using UnityEngine;

public class GateOpener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryInteractionGate gate;

    [Header("Enable after story gate opens")]
    [SerializeField] private MonoBehaviour[] componentsToEnable;

    private bool _triggered;

    private void Awake()
    {
        if (gate == null)
            gate = FindFirstObjectByType<StoryInteractionGate>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 실행됐으면 종료
        if (_triggered) return;

        // 1. Player만 반응
        if (!other.CompareTag("Player")) return;

        // 2. Gate 열렸는지 확인
        if (gate == null || !gate.IsUnlocked) return;

        // 실행 처리
        _triggered = true;

        // 3. 컴포넌트 활성화
        if (componentsToEnable != null)
        {
            foreach (var comp in componentsToEnable)
            {
                if (comp != null)
                    comp.enabled = true;
            }
        }

        // 4. 자기 자신 비활성화, 오브젝트 비활성화
        enabled = false;

        gameObject.SetActive(false);
    }
}