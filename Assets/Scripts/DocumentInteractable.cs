using UnityEngine;

public class DocumentInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite documentSprite;

    public void Interact()
    {
        if (DocumentViewerUI.Instance == null) return;

        if (DocumentViewerUI.Instance.IsOpen)
        {
            DocumentViewerUI.Instance.Close();
            return;
        }

        DocumentViewerUI.Instance.Open(documentSprite);
    }
}