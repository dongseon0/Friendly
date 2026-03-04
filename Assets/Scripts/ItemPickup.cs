using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Assign Item Data")]
    public ItemData item;          // ScriptableObject 아이템
    public int amount = 1;         // 아직 수량 구현 안 함

    [Header("Story Link (optional)")]
    [SerializeField] private dialog story;          // 씬에 있는 dialog 오브젝트를 드래그
    [SerializeField] private bool showInspectLine = true;

    public void Interact()
    {
        if (item == null)
        {
            Debug.LogError("[ItemPickup] ItemData is empty. Need to distribute the item");
            return;
        }

        // 1) add item to inventory using ItemData
        InventoryManager.Instance.AddItem(item);  // ✅ ItemData로 추가

        // 2) Json
        if (story == null) story = FindFirstObjectByType<dialog>();
        if (story != null && !string.IsNullOrEmpty(item.itemId))
        {
            story.PickupItemById_FromWorld(item.itemId, showInspectLine);
        }
        else
        {
            Debug.LogWarning("Cannot Find Json of [ItemPickUp] or itemId is invalid.");
        }

        Destroy(gameObject);
    }
}