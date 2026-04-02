using UnityEngine;

public class PhoneInteract : MonoBehaviour, IInteractable
{
    private AudioSource audioSource;

    void Start()
    {
        //전화기에 붙은 audioSource 가져옴
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        PlayPhoneAudio();
    }

    void PlayPhoneAudio()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log("전화기 재생 중");
        }
    }

}