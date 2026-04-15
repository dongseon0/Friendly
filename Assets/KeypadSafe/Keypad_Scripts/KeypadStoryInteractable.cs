using UnityEngine;
using NavKeypad;

public class KeypadStoryInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private dialog story;
    [SerializeField] private string targetName = "Keypad";

    private void Awake()
    {
        AutoBindStory();
    }

    private void AutoBindStory()
    {
        if (story != null) return;

        story = FindFirstObjectByType<dialog>(FindObjectsInactive.Include);

        if (story == null)
            Debug.LogWarning("[KeypadStoryInteractable] dialog not found.");
    }

    public void Interact()
    {
        AutoBindStory();

        if (story == null)
        {
            Debug.LogWarning("[KeypadStoryInteractable] dialog not found on interact.");
            return;
        }

        if (story.IsWaitingForInteractionTarget(targetName))
            story.RequestInteraction(targetName);
        else
            story.TriggerOptionalInteractionNow(targetName);
    }
}