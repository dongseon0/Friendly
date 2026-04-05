using UnityEngine;

public class MannequinTrigger : MonoBehaviour
{
    private Animator mannequinAnimator;
    private bool isPlayerNearby = false;

    void Start()
    {
        mannequinAnimator = GetComponentInParent<Animator>();
        mannequinAnimator.SetFloat("AnimSpeed", 0f);
    }

    // ScareManager가 호출할 함수
    public void ActivateScare()
    {
        // 플레이어가 근처에 있을 때만 작동하게 하거나, 무조건 작동하게 할 수 있음
        if (isPlayerNearby)
        {
            mannequinAnimator.SetFloat("AnimSpeed", 1f);
            Invoke("StopScare", 1.5f); // 1.5초 뒤에 자동으로 멈춤
        }
    }

    private void StopScare()
    {
        mannequinAnimator.SetFloat("AnimSpeed", 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = false;
    }
}