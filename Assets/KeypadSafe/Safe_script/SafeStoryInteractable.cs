using UnityEngine;

public class SafeStoryInteractable : MonoBehaviour, IInteractable
{
    [Header("Story")]
    [SerializeField] private dialog story;
    [SerializeField] private string targetName = "Locked Safe";

    private void Awake()
    {
        AutoBindStory();
    }

    private void AutoBindStory()
    {
        if (story != null) return;

        story = FindFirstObjectByType<dialog>(FindObjectsInactive.Include);

        if (story == null)
            Debug.LogWarning("[SafeStoryInteractable] dialog not found.");
    }

    public void Interact()
    {
        AutoBindStory();

        if (story == null)
        {
            Debug.LogWarning("[SafeStoryInteractable] dialog not found on interact.");
            return;
        }

        story.RequestInteraction(targetName);
    }
}