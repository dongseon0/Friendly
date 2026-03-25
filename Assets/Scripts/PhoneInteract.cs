using UnityEngine;

public class PhoneInteract : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isPlayerNearby = false;

    void Start()
    {
        //전화기에 붙은 audioSource 가져옴
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 플레이어가 근처에 있고, Z 키를 눌렀으며, 현재 소리가 재생 중이 아닐 때
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.Z))
        {
            PlayPhoneAudio();
        }
    }

    void PlayPhoneAudio()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log("전화기 재생 중");
        }
    }

    // 플레이어가 감지 영역에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    // 플레이어가 영역을 벗어났을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}