using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUIController : MonoBehaviour
{
    public static LoadingUIController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private Slider progressBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void Show(string text = "Loading...")
    {
        if (loadingText != null)
            loadingText.text = text;

        if (progressBar != null)
            progressBar.value = 0f;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);
    }

    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.value = Mathf.Clamp01(value);
    }

    public void Hide()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
}