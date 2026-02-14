using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public string itemName = "Strange File";

    public void Interact()
    {
        var inv = FindObjectOfType<InventoryManager>();
        inv.AddItem(itemName);  // 인벤토리에 이름 저장

        Destroy(gameObject); // 게임 오브젝트 파괴
    }
}
