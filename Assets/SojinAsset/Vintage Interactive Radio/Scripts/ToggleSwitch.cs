using UnityEngine;

public class ToggleSwitch : MonoBehaviour
{
    public AudioSource controlledAudio;
    private TunerKnob tuner;
    private bool isOn = false;

    void Start()
    {
        tuner = Object.FindAnyObjectByType<TunerKnob>();

        if (controlledAudio != null)
        {
            controlledAudio.Stop(); // 시작할 때 확실히 정지
            controlledAudio.playOnAwake = false; // 코드에서도 한 번 더 방어
        }

        // 초기 스위치 외형 (꺼진 상태: 아래로 스위치)
        UpdateVisuals();
    }

    void OnMouseUpAsButton()
    {
        isOn = !isOn;

        UpdateVisuals();

        if (controlledAudio != null)
        {
            if (isOn)
            {
                controlledAudio.Play(); // 스위치 올리면 재생 시작
            }
            else
            {
                controlledAudio.Stop(); // 스위치 내리면 완전히 정지
            }
        }
    }

    void UpdateVisuals()
    {
        // isOn이 true(켜짐)일 때 45도, false(꺼짐)일 때 -45도
        transform.localEulerAngles = isOn ? new Vector3(-45f, 0f, 0f) : new Vector3(45f, 0f, 0f);
    }

    // InteractiveRadio에서 전원 상태를 확인할 수 있도록 함수 추가
    public bool IsOn() => isOn;
}