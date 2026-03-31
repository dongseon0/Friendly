using UnityEngine;

public class StoryInteractionGate : MonoBehaviour
{
    [SerializeField] private dialog story;
    [SerializeField] private string targetName = "Abandoned Hospital : Main Entrance";
    [SerializeField] private string flagName = "hospital_locked_checked";
    [SerializeField] private KeyCode interactKey = KeyCode.Z;

    [Header("Optional self refs")]
    [SerializeField] private Collider gateCollider;
    [SerializeField] private GameObject[] objectsToDisable;

    [SerializeField] private bool destroyAfterUnlock = true;

    private bool _playerInside;
    private bool _unlocked;

    public bool IsUnlocked => _unlocked;

    private void Awake()
    {
        if (story == null)
            story = FindFirstObjectByType<dialog>();

        if (gateCollider == null)
            gateCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        RefreshGateState();
    }

    private void Update()
    {
        if (_unlocked) return;

        if (_playerInside && Input.GetKeyDown(interactKey))
        {
            if (story != null)
                story.RequestInteraction(targetName);
        }

        RefreshGateState();
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

    private void RefreshGateState()
    {
        if (story == null) return;
        if (!story.IsFlagTrue(flagName)) return;

        UnlockGate();
    }

    private void UnlockGate()
    {
        if (_unlocked) return;
        _unlocked = true;

        if (objectsToDisable != null)
        {
            foreach (var go in objectsToDisable)
            {
                if (go != null)
                    go.SetActive(false);
            }
        }

        if (gateCollider != null)
            gateCollider.enabled = false;

        if (destroyAfterUnlock)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}