using UnityEngine;

public class Annex2FSceneStoryStarter : MonoBehaviour
{
    [SerializeField] private string storySceneId = "S09_ANNEX_2F";
    [SerializeField] private string startNodeId = "S09_N0";

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        dialog story = FindFirstObjectByType<dialog>(FindObjectsInactive.Include);

        if (story == null)
        {
            Debug.LogWarning("[Annex2FSceneStoryStarter] dialog not found.");
            return;
        }

        _triggered = true;

        story.StartScene(storySceneId, startNodeId);

        gameObject.SetActive(false);
    }
}