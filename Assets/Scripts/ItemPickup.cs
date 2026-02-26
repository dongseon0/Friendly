using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Assign Item Data")]
    public ItemData item;          // ScriptableObject 아이템
    public int amount = 1;         // 아직 수량 구현 안 함

    public void Interact()
    {
        if (item == null)
        {
            Debug.LogError("[ItemPickup] ItemData가 비어있음. item 할당 필요");
            return;
        }

        InventoryManager.Instance.AddItem(item);  // ✅ ItemData로 추가
        Destroy(gameObject);
    }
}