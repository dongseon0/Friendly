//Carefully uploading this file, I should open the unity project first and making new cs file.
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIController : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueRoot;     // 패널 전체
    [SerializeField] private TMP_Text speakerText;        // speaker 라벨
    [SerializeField] private TMP_Text bodyText;           // 본문
    [SerializeField] private Button dialogueContinueBtn;  // 계속 버튼(없으면 클릭/스페이스로 대체 가능)

    [Header("Toast UI")]
    [SerializeField] private GameObject toastRoot;
    [SerializeField] private TMP_Text toastText;

    [Header("Objective UI")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Interact Hint UI (Optional)")]
    [SerializeField] private GameObject interactHintRoot;
    [SerializeField] private TMP_Text interactHintText;

    [Header("Choice UI")]
    [SerializeField] private GameObject choiceRoot;
    [SerializeField] private TMP_Text choicePromptText;
    [SerializeField] private Transform choiceButtonParent;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<NamedClip> clips = new();

    [Header("Video (Optional)")]
    [SerializeField] private GameObject videoRoot;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private List<NamedVideo> videos = new();

    [Header("Speaker Portrait")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite defaultPortrait;
    [SerializeField] private List<NamedPortrait> portraits = new();

    [SerializeField] private float objectiveVisibleSeconds = 3f;
    private Coroutine _objectiveHideRoutine;
    private bool _objectiveAutoVisible;

    [SerializeField] private float dialogueAdvanceBlockSeconds = 1f;
    private float _dialogueInputUnlockTime;

    private bool _objectiveWasVisibleBeforeDialogue;
    private bool _waitForAdvanceRelease;

    // Choice 지원 여부
    public bool SupportsChoiceUI => choiceRoot != null && choiceButtonPrefab != null;

    // 내부 상태
    private Action _onDialogueDone;
    private bool _dialogueWaiting;

    [Serializable]
    public class NamedClip
    {
        public string name;
        public AudioClip clip;
    }

    [Serializable]
    public class NamedVideo
    {
        public string id;
        public VideoClip clip;
    }

    [Serializable]
    public class NamedPortrait
    {
        public string speaker;
        public Sprite sprite;
    }

    private readonly Dictionary<string, AudioClip> _clipMap = new();
    private readonly Dictionary<string, VideoClip> _videoMap = new();

    private Dictionary<string, Sprite> _portraitMap = new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        _portraitMap.Clear();
        foreach (var p in portraits)
        {
            if (p == null) continue;

            string key = NormalizeSpeakerKey(p.speaker);
            if (!string.IsNullOrEmpty(key) && p.sprite != null)
                _portraitMap[key] = p.sprite;
        }
        // lookup
        _clipMap.Clear();
        foreach (var c in clips)
            if (!string.IsNullOrEmpty(c.name) && c.clip) _clipMap[c.name] = c.clip;

        _videoMap.Clear();
        foreach (var v in videos)
            if (!string.IsNullOrEmpty(v.id) && v.clip) _videoMap[v.id] = v.clip;

        // 초기 UI 상태
        if (dialogueRoot) dialogueRoot.SetActive(false);
        if (toastRoot) toastRoot.SetActive(false);
        if (interactHintRoot) interactHintRoot.SetActive(false);
        if (choiceRoot) choiceRoot.SetActive(false);
        if (videoRoot) videoRoot.SetActive(false);

        if (dialogueContinueBtn)
            dialogueContinueBtn.onClick.AddListener(ContinueDialogue);
    }

    private void Update()
    {
        // 대사 표시 중: 마우스 클릭/스페이스로 진행
        if (_dialogueWaiting)
        {
            if (Time.unscaledTime < _dialogueInputUnlockTime)
                return;

            bool anyAdvanceHeld =
                Input.GetMouseButton(0) ||
                Input.GetKey(KeyCode.Space) ||
                Input.GetKey(KeyCode.Return);

            if (_waitForAdvanceRelease)
            {
                if (!anyAdvanceHeld)
                    _waitForAdvanceRelease = false;

                return;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                ContinueDialogue();
        }
    }

    // ---------------------------
    // 1) Dialogue
    // ---------------------------
    public void ShowDialogue(string speaker, string text, Action onDone)
    {
        if (!dialogueRoot)
        {
            Debug.Log($"[DIALOGUE] {speaker}: {text}");
            onDone?.Invoke();
            return;
        }

        dialogueRoot.SetActive(true);

        // Portrait
        UpdatePortrait(speaker);

        if (objectiveText != null)
        {
            _objectiveWasVisibleBeforeDialogue = objectiveText.gameObject.activeSelf && _objectiveAutoVisible;
            objectiveText.gameObject.SetActive(false);
        }

        // speaker
        if (speakerText)
        {
            bool hasSpeaker = !string.IsNullOrWhiteSpace(speaker);
            speakerText.gameObject.SetActive(hasSpeaker);
            if (hasSpeaker) speakerText.text = speaker;
        }

        if (bodyText) bodyText.text = text ?? "";

        _onDialogueDone = onDone;
        _dialogueWaiting = true;

        _dialogueInputUnlockTime = Time.unscaledTime + dialogueAdvanceBlockSeconds;
        _waitForAdvanceRelease = true;
    }

    private void UpdatePortrait(string speaker)
    {
        if (portraitImage == null) return;

        if (string.IsNullOrEmpty(speaker))
        {
            portraitImage.gameObject.SetActive(false);
            return;
        }

        string key = NormalizeSpeakerKey(speaker);

        if (_portraitMap.TryGetValue(key, out var foundSprite) && foundSprite != null)
        {
            portraitImage.sprite = foundSprite;
            portraitImage.gameObject.SetActive(true);
            return;
        }

        if (defaultPortrait != null)
        {
            Debug.LogWarning($"[Portrait] Not found for speaker: {speaker}. Using default portrait.");
            portraitImage.sprite = defaultPortrait;
            portraitImage.gameObject.SetActive(true);
            return;
        }

        Debug.LogWarning($"[Portrait] Not found for speaker: {speaker}, and no default portrait assigned.");
        portraitImage.gameObject.SetActive(false);
    }

    private string NormalizeSpeakerKey(string speaker)
    {
        if (string.IsNullOrWhiteSpace(speaker)) return string.Empty;
        return speaker.Trim();
    }

    private void ContinueDialogue()
    {
        if (!_dialogueWaiting) return;

        _dialogueWaiting = false;
        dialogueRoot.SetActive(false);

        var cb = _onDialogueDone;
        bool restoreObjective = _objectiveWasVisibleBeforeDialogue;
        _onDialogueDone = null;
        StartCoroutine(FinishDialogueNextFrame(cb, restoreObjective));
    }

    private IEnumerator FinishDialogueNextFrame(Action cb, bool restoreObjective)
    {
        yield return null;

        cb?.Invoke();

        if (!_dialogueWaiting && objectiveText != null && restoreObjective)
            objectiveText.gameObject.SetActive(true);
    }

    public void HideDialogueImmediate()
    {
        _dialogueWaiting = false;
        _waitForAdvanceRelease = false;
        _onDialogueDone = null;

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }

    // ---------------------------
    // 2) Toast
    // ---------------------------
    public void ShowToast(string text, float seconds)
    {
        if (!toastRoot)
        {
            Debug.Log($"[TOAST] {text} ({seconds}s)");
            return;
        }

        StopCoroutine(nameof(ToastRoutine));
        toastRoot.SetActive(true);
        if (toastText) toastText.text = text ?? "";
        StartCoroutine(ToastRoutine(Mathf.Max(0.5f, seconds)));
    }

    private IEnumerator ToastRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (toastRoot) toastRoot.SetActive(false);
    }

    // ---------------------------
    // 3) Objective
    // ---------------------------
    public void ShowObjective(string text)
    {
        if (!objectiveText)
        {
            Debug.Log($"[OBJECTIVE] {text}");
            return;
        }

        objectiveText.gameObject.SetActive(true);
        objectiveText.text = text ?? "";
        _objectiveAutoVisible = true;

        if (_objectiveHideRoutine != null)
            StopCoroutine(_objectiveHideRoutine);

        if (objectiveVisibleSeconds > 0f)
            _objectiveHideRoutine = StartCoroutine(HideObjectiveAfterSeconds(objectiveVisibleSeconds));
    }

    private IEnumerator HideObjectiveAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        _objectiveAutoVisible = false;

        if (!_dialogueWaiting && objectiveText != null)
            objectiveText.gameObject.SetActive(false);
    }

    // ---------------------------
    // 4) Interact Hint (Optional)
    // ---------------------------
    public void ShowInteractHint(bool on, string target)
    {
        if (!interactHintRoot) return;

        interactHintRoot.SetActive(on);
        if (on && interactHintText)
        {
            if (string.IsNullOrWhiteSpace(target))
                interactHintText.text = "Press Z to interact";
            else
                interactHintText.text = $"Press Z to interact: {target}";
        }
    }

    // ---------------------------
    // 5) Sound (Optional)
    // ---------------------------
    public void PlaySound(string name)
    {
        if (sfxSource == null)
        {
            Debug.Log($"[SOUND] {name}");
            return;
        }

        if (string.IsNullOrEmpty(name) || !_clipMap.TryGetValue(name, out var clip) || clip == null)
        {
            Debug.LogWarning($"Sound not found: {name}");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    // ---------------------------
    // 6) Video (Optional)
    // ---------------------------
    public IEnumerator PlayVideoAndWait(string id)
    {
        if (videoPlayer == null || videoRoot == null)
        {
            Debug.Log($"[VIDEO] {id} (no video UI)");
            yield break;
        }

        if (string.IsNullOrEmpty(id) || !_videoMap.TryGetValue(id, out var clip) || clip == null)
        {
            Debug.LogWarning($"Video not found: {id}");
            yield break;
        }

        videoRoot.SetActive(true);
        videoPlayer.clip = clip;
        videoPlayer.Play();

        // 끝까지 대기
        while (videoPlayer.isPlaying)
            yield return null;

        videoRoot.SetActive(false);
    }

    // ---------------------------
    // 7) Choice UI
    // ---------------------------
    public void ShowChoice(string prompt, List<string> options, Action<int> onPicked)
    {
        if (!SupportsChoiceUI)
        {
            Debug.Log($"[CHOICE] {prompt} (fallback pick 0)");
            onPicked?.Invoke(0);
            return;
        }

        choiceRoot.SetActive(true);
        if (choicePromptText) choicePromptText.text = prompt ?? "";

        // 기존 버튼 정리
        for (int i = choiceButtonParent.childCount - 1; i >= 0; i--)
            Destroy(choiceButtonParent.GetChild(i).gameObject);

        // 버튼 생성
        for (int i = 0; i < options.Count; i++)
        {
            int idx = i;
            var btn = Instantiate(choiceButtonPrefab, choiceButtonParent);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = options[i];

            btn.onClick.AddListener(() =>
            {
                choiceRoot.SetActive(false);
                onPicked?.Invoke(idx);
            });
        }
    }

    public void SetLight(string mode, float intensity)
    {
        Debug.Log($"[UI] SetLight mode={mode}, intensity={intensity}");
    }

    public void SetVignette(string mode, float strength)
    {
        Debug.Log($"[UI] SetVignette mode={mode}, strength={strength}");
    }

}