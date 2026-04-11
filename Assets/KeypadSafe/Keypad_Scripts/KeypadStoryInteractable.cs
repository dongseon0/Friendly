using UnityEngine;

public class KeypadStoryInteractable : MonoBehaviour
{
    [SerializeField] private dialog story;
    [SerializeField] private string targetName = "Keypad";
    [SerializeField] private KeyCode interactKey = KeyCode.Z;
    [SerializeField] private GameObject promptUI;

    private bool _playerInside;

    private void Awake()
    {
        if (story == null)
            story = FindFirstObjectByType<dialog>();

        if (promptUI == null)
        {
            var persistentRoot = GameObject.Find("[PersistentRoot]");
            if (persistentRoot != null)
            {
                var t = persistentRoot.transform.Find("UICanvas/InteractText");
                if (t != null)
                    promptUI = t.gameObject;
            }
        }
    }

    private void OnEnable()
    {
        SetPrompt(false);
    }

    private void OnDisable()
    {
        SetPrompt(false);
    }

    private void SetPrompt(bool active)
    {
        if (promptUI != null && promptUI.activeSelf != active)
            promptUI.SetActive(active);
    }

    private void Update()
    {
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
}