using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    // 인벤 왼쪽 리스트
    [Header("List")]
    [SerializeField] private Transform contentRoot;        // ScrollView/Content
    [SerializeField] private InventoryItemRow rowPrefab;

    // 인벤 오른쪽 디테일 창
    [Header("Detail")]
    [SerializeField] private Image detailImage;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailDesc;

    private readonly List<InventoryItemRow> _rows = new();
    private ItemData _selected;

    void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += Rebuild;

        Rebuild();
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Rebuild;
    }

    void Rebuild()
    {
        // clear rows
        foreach (var r in _rows) Destroy(r.gameObject);
        _rows.Clear();

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        foreach (var item in inv.Items)
        {
            var row = Instantiate(rowPrefab, contentRoot);
            row.Bind(item, OnClickItem);
            _rows.Add(row);
        }

        if (_selected == null && inv.Items.Count > 0)
            OnClickItem(inv.Items[0]);
        else
            RefreshDetail(_selected);
    }

    void OnClickItem(ItemData item)
    {
        _selected = item;
        RefreshDetail(item);
    }

    void RefreshDetail(ItemData item)
    {
        if (item == null)
        {
            if (detailImage) detailImage.enabled = false;
            if (detailName) detailName.text = "";
            if (detailDesc) detailDesc.text = "";
            return;
        }

        if (detailImage)
        {
            detailImage.sprite = item.icon;
            detailImage.enabled = (item.icon != null);
        }
        if (detailName) detailName.text = item.displayName;
        if (detailDesc) detailDesc.text = item.description;
    }
}