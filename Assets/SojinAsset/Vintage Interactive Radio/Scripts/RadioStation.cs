using UnityEngine;

[System.Serializable]
public class RadioStation
{
    public string stationName;
    public float minFrequency;
    public float maxFrequency;
    public AudioClip clip;

    [Header("Event Settings")]
    public bool isEventChannel;      // 2번 채널인가?
    public bool isPermanentDisabled; // 이벤트 후 화이트 노이즈로 변했는가?

    public bool isTuned(float frequency)
    {
        if (isPermanentDisabled) return false; // 비활성 상태면 미검출 -> 자동으로 화이트 노이즈 재생
        return (frequency <= maxFrequency && frequency >= minFrequency);
    }
}