using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel;
    public TextMeshProUGUI inventoryText;

    List<string> items = new List<string>();

    // 아이템 얻기
    public void AddItem(string itemName)
    {
        items.Add(itemName);
        UpdateUI();
    }

    void UpdateUI()
    {
        inventoryText.text = "Inventory:\n";
        foreach (var item in items)
        {
            inventoryText.text += "- " + item + "\n";
        }
    }

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        Time.timeScale = inventoryPanel.activeSelf ? 0f : 1f;
        Cursor.visible = inventoryPanel.activeSelf;
        Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
