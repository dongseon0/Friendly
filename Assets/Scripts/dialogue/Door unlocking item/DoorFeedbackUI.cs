using System.Collections;
using UnityEngine;

public class DoorFeedbackUI : MonoBehaviour
{
    public static DoorFeedbackUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject doorLockedUI;
    [SerializeField] private GameObject doorUnlockingAlarmUI;
    [SerializeField] private float showDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip keyUnlockingClip;

    private Coroutine _lockedRoutine;
    private Coroutine _unlockRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (doorLockedUI != null) doorLockedUI.SetActive(false);
        if (doorUnlockingAlarmUI != null) doorUnlockingAlarmUI.SetActive(false);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void ShowLockedUI()
    {
        if (doorLockedUI == null) return;

        if (_lockedRoutine != null)
            StopCoroutine(_lockedRoutine);

        _lockedRoutine = StartCoroutine(ShowForSeconds(doorLockedUI, showDuration));
    }

    public void ShowUnlockingFeedback()
    {
        if (keyUnlockingClip != null && audioSource != null)
            audioSource.PlayOneShot(keyUnlockingClip);

        if (doorUnlockingAlarmUI == null) return;

        if (_unlockRoutine != null)
            StopCoroutine(_unlockRoutine);

        _unlockRoutine = StartCoroutine(ShowForSeconds(doorUnlockingAlarmUI, showDuration));
    }

    private IEnumerator ShowForSeconds(GameObject target, float seconds)
    {
        target.SetActive(true);
        yield return new WaitForSeconds(seconds);
        target.SetActive(false);
    }
}