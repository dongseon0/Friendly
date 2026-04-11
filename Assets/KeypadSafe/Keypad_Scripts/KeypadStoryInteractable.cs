using UnityEngine;
using NavKeypad;

public class KeypadStoryInteractable : MonoBehaviour
{
    [SerializeField] private dialog story;
    [SerializeField] private string targetName = "Keypad";
    [SerializeField] private KeyCode interactKey = KeyCode.Z;
    [SerializeField] private GameObject promptUI;

    [Header("Optional")]
    [SerializeField] private KeypadModalController modal;   // 직접 열기용
    [SerializeField] private bool openModalDirectly = true; // 스토리 거치지 않고 바로 열기

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

        if (Input.GetKeyDown(interactKey))
        {
            // 1) 스토리 요청
            if (story != null)
                story.RequestInteraction(targetName);

            // 2) 실제 키패드 모달 열기
            if (openModalDirectly && modal != null && !modal.IsOpen)
            {
                modal.Open();
                SetPrompt(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        SetPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        SetPrompt(false);
    }
}