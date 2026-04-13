using UnityEngine;

// 플레이어가 문을 통과하면 닫게 만드는 스크립트

public class DoorAutoCloseTrigger : MonoBehaviour
{
    [SerializeField] private DoorInteractable door;

    private void Awake()
    {
        if (door == null)
            door = GetComponentInParent<DoorInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (door == null) return;

        door.CloseDoor();
    }
}