using UnityEngine;
using UnityEngine.UI;

public class DocumentViewerUI : MonoBehaviour
{
    public static DocumentViewerUI Instance { get; private set; }

    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Image documentImage;

    public bool IsOpen => rootPanel != null && rootPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void Open(Sprite sprite)
    {
        if (rootPanel == null || documentImage == null || sprite == null)
            return;

        documentImage.sprite = sprite;
        rootPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        if (rootPanel == null)
            return;

        rootPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Toggle(Sprite sprite)
    {
        if (IsOpen) Close();
        else Open(sprite);
    }
}