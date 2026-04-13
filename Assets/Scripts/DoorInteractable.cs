using UnityEngine;
using System.Collections;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door Parts")]
    [SerializeField] private Transform hingePivot; // 실제 회전할 축
    [SerializeField] private Transform player;     // 플레이어 자동 바인딩

    [Header("Open Settings")]
    [SerializeField] private float openAngle = 100f;
    [SerializeField] private float openCloseDuration = 0.25f;   // 문 열리고 대기 시간

    [Header("State")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool isMoving = false;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private void Awake()
    {
        if (hingePivot == null)
            hingePivot = transform;

        closedRotation = hingePivot.localRotation;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void OnEnable()
    {
        // 씬 전환 후 플레이어 재스폰 대비
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    public void Interact()
    {
        if (isMoving) return;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (!isOpen)
            OpenDoorAwayFromPlayer();
        else
            StartCoroutine(RotateDoor(closedRotation, false));
    }

    private void OpenDoorAwayFromPlayer()
    {
        if (player == null) return;

        // 플레이어가 문 기준 어느 쪽에 있는지 판단
        Vector3 toPlayer = (player.position - hingePivot.position).normalized;

        // 문의 오른쪽 방향 기준으로 부호 결정
        float side = Vector3.Dot(hingePivot.right, toPlayer);

        // 플레이어 쪽으로 안 열리게, 반대 방향으로 회전
        float targetAngle = (side >= 0f) ? -openAngle : openAngle;

        openedRotation = closedRotation * Quaternion.Euler(0f, targetAngle, 0f);

        StartCoroutine(RotateDoor(openedRotation, true));
    }

    public void CloseDoor()
    {
        if (isMoving) return;
        if (!isOpen) return;

        StartCoroutine(RotateDoor(closedRotation, false));
    }

    private IEnumerator RotateDoor(Quaternion targetRotation, bool openState)
    {
        isMoving = true;

        Quaternion startRotation = hingePivot.localRotation;
        float elapsed = 0f;

        while (elapsed < openCloseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openCloseDuration);
            hingePivot.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        hingePivot.localRotation = targetRotation;
        isOpen = openState;
        isMoving = false;
    }

    public bool IsOpen => isOpen;
}