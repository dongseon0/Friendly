using UnityEngine;

public class DoorAutoCloseTrigger : MonoBehaviour
{
    [SerializeField] private DoorInteractable door;

    private void Awake()
    {
        if (door == null)
            door = GetComponentInParent<DoorInteractable>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (door == null) return;

        // 문이 완전히 열린 상태에서만 닫기
        if (!door.IsOpen) return;
        if (door.IsMoving) return;

        door.CloseDoor();
    }
}