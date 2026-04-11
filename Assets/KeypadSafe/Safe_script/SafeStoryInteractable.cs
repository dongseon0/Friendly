using UnityEngine;

public class SafeStoryInteractable : MonoBehaviour
{
    [SerializeField] private dialog story;
    [SerializeField] private string targetName = "Locked Safe";
    [SerializeField] private KeyCode interactKey = KeyCode.Z;
    [SerializeField] private GameObject promptUI;

    private bool _playerInside;

    private void Awake()
    {
        if (story == null)
            story = FindFirstObjectByType<dialog>();
    }

    private void OnEnable()
    {
        SetPrompt(false);
    }

    private void OnDisable()
    {
        SetPrompt(false);
    }

    private void Update()
    {
        SetPrompt(_playerInside);

        if (!_playerInside) return;
        if (story == null) return;

        if (Input.GetKeyDown(interactKey))
            story.RequestInteraction(targetName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
    }

    private void SetPrompt(bool active)
    {
        if (promptUI != null && promptUI.activeSelf != active)
            promptUI.SetActive(active);
    }
}