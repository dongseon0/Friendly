using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemRow : MonoBehaviour
{
    [SerializeField] private Button button; // 아이템 행
    [SerializeField] private TMP_Text nameText; // 아이템 이름

    private ItemData _item;

    public void Bind(ItemData item, Action<ItemData> onClick)
    {
        _item = item;
        nameText.text = item.displayName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(_item));
    }

    // Inspector 연결 편하게(자동)
    private void Reset()
    {
        button = GetComponent<Button>();
        nameText = GetComponentInChildren<TMP_Text>();
    }
}