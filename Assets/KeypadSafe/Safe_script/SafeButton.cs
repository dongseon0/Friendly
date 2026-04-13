using UnityEngine;

public class SafeButton : MonoBehaviour
{
    [SerializeField] private int digit;
    [SerializeField] private SafeModalController modal;

    public void OnClick()
    {
        if (modal == null) return;
        modal.ToggleDigit(digit);
    }
}