using UnityEngine;

public class PlayerProfiler : MonoBehaviour
{
    public static PlayerProfiler Instance;

    // 성향 벡터 4차원 (0~1 정규화)
    // [0] 평균 마이크 반응
    // [1] 평균 마우스 반응
    // [2] 가장 반응 좋은 액션 ID (정규화)
    // [3] 반응 감쇠율 (높을수록 금방 무뎌지는 플레이어)
    private float[] profileVector = new float[4] { 0.5f, 0.5f, 0.5f, 0.5f };

    private int sampleCount = 0;
    private float totalMicResponse = 0f;
    private float totalMouseResponse = 0f;
    private int[] actionResponseCount = new int[5];
    private float lastFearSignal = 0f;
    private float totalDecay = 0f;
    private int decaySamples = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ScareManager가 연출 실행할 때마다 호출
    public void RecordResponse(int action, float micVal, float mouseVal)
    {
        sampleCount++;
        totalMicResponse += micVal;
        totalMouseResponse += mouseVal;

        if (action >= 0 && action < actionResponseCount.Length)
            actionResponseCount[action]++;

        float fearSignal = micVal * 0.7f + mouseVal * 0.3f;

        if (lastFearSignal > 0f)
        {
            float decay = Mathf.Clamp01(
                1f - (fearSignal / (lastFearSignal + 0.001f)));
            totalDecay += decay;
            decaySamples++;
        }
        lastFearSignal = fearSignal;

        UpdateProfileVector();
    }

    private void UpdateProfileVector()
    {
        if (sampleCount == 0) return;

        profileVector[0] = totalMicResponse / sampleCount;
        profileVector[1] = totalMouseResponse / sampleCount;

        int bestAction = 1;
        for (int i = 2; i < actionResponseCount.Length; i++)
            if (actionResponseCount[i] > actionResponseCount[bestAction])
                bestAction = i;
        profileVector[2] = bestAction / 4f;

        profileVector[3] = decaySamples > 0
            ? totalDecay / decaySamples
            : 0f;
    }

    public float[] GetProfileVector() => profileVector;

    public void SaveProfile()
    {
        for (int i = 0; i < profileVector.Length; i++)
            PlayerPrefs.SetFloat($"PlayerProfile_{i}", profileVector[i]);
        PlayerPrefs.Save();
        Debug.Log("[PlayerProfiler] 성향 벡터 저장 완료");
    }

    public void LoadProfile()
    {
        for (int i = 0; i < profileVector.Length; i++)
            profileVector[i] = PlayerPrefs.GetFloat($"PlayerProfile_{i}", 0.5f);
        Debug.Log("[PlayerProfiler] 성향 벡터 로드 완료");
    }
}