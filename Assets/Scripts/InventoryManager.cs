using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel;
    public TextMeshProUGUI inventoryText;
    public static InventoryManager Instance; // 중복 생성 안 되게 싱글톤

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

    // Scene 넘어가도 인벤토리 유지되도록ㄴ
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // 중복이면 삭제
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        Time.timeScale = inventoryPanel.activeSelf ? 0f : 1f;
        Cursor.visible = inventoryPanel.activeSelf;
        Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
