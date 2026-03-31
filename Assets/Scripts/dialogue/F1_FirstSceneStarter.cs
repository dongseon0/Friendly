using UnityEngine;

public class F1_FirstSceneStarter : MonoBehaviour
{
    [SerializeField] private dialog story;
    [SerializeField] private string sceneId = "S02_HOSPITAL_LOBBY";
    [SerializeField] private string startNodeId = "S02_N0";
    [SerializeField] private bool triggerOnce = true;

    private bool _triggered;

    private void Awake()
    {
        if (story == null)
            story = FindFirstObjectByType<dialog>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;
        if (story == null) return;

        story.StartScene(sceneId, startNodeId);
        _triggered = true;
    }
}