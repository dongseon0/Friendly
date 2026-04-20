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

        // 1) 스토리 메인 흐름이 지금 Keypad를 기다리는 중이면 정상 진행
        if (story.IsWaitingForInteractionTarget(targetName))
        {
            story.RequestInteraction(targetName);
            return;
        }

        // 2) 그 외 언제든지 누르면 hard-coded optional fallback
        story.TriggerKeypadOptionalFallback();
    }
}