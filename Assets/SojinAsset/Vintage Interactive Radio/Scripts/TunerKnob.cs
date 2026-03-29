using UnityEngine;

public class TunerKnob : MonoBehaviour
{
    public float rotationSpeed = 20f;
    private Transform tunerIndicator;

    private float frequency = 80f;
    private float minRotation = -0.9f; // 80 지점
    private float maxRotation = 0.85f; // 110 지점
    private float tunerSpeed; 

    void Start()
    {
        // 바늘 오브젝트 찾기
        tunerIndicator = GameObject.Find("Arrow").transform;
        SetSpeed(rotationSpeed);

        // 시작 시 현재 다이얼의 위치에 맞춰 주파수와 바늘을 한 번 동기화해줍니다.
        UpdateValues();
    }

    void OnMouseOver()
    {
        float currentZ = transform.localRotation.z;

        // 왼쪽 클릭: 주파수 증가
        if (Input.GetKey(KeyCode.Mouse0) && currentZ < maxRotation)
        {
            transform.Rotate(Vector3.forward * Time.deltaTime * rotationSpeed);
        }
        // 오른쪽 클릭: 주파수 감소
        else if (Input.GetKey(KeyCode.Mouse1) && currentZ > minRotation)
        {
            transform.Rotate(Vector3.back * Time.deltaTime * rotationSpeed);
        }

        UpdateValues();
    }

    // 주파수 계산과 바늘 위치 업데이트를 하나의 함수로 묶었습니다.
    private void UpdateValues()
    {
        float clampedZ = Mathf.Clamp(transform.localRotation.z, minRotation, maxRotation);

        // 범위를 벗어나지 않게 고정
        if (transform.localRotation.z != clampedZ)
        {
            transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, clampedZ, transform.localRotation.w);
        }

        // 주파수 매핑 (80 ~ 110)
        float t = (clampedZ - minRotation) / (maxRotation - minRotation);
        frequency = Mathf.Lerp(80f, 110f, t);

        // 바늘 동기화
        if (tunerIndicator != null)
        {
            float arrowAngle = Mathf.Lerp(80f, -80f, t);
            tunerIndicator.localRotation = Quaternion.Euler(0, 0, -arrowAngle);
        }
    }

    public float GetFrequency() => frequency;

    public void SetSpeed(float speed)
    {
        rotationSpeed = speed;
        tunerSpeed = rotationSpeed * 0.6667f;
    }
}