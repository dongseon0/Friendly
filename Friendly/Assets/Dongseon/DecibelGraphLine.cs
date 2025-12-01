using UnityEngine;
using UnityEngine.UI;

// 데시벨에 따라 파형 그래프가 그려지도록 하는 코드
public class DecibelGraphLine : MonoBehaviour
{
    public FearSignalReader mic;
    public RawImage graphImage;

    Texture2D texture;
    Color32[] clearPixels;

    int width = 300;
    int height = 100;

    void Start()
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        graphImage.texture = texture;

        clearPixels = new Color32[width * height];
        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = new Color32(0,0,0,0);
    }

    void Update()
    {
        texture.SetPixels32(clearPixels);

        // y = dB를 height 비율로 변환
        float normalized = Mathf.InverseLerp(-60f, 0f, mic.currentDB);
        int y = Mathf.RoundToInt(normalized * height);

        // x축 이동을 위해 Shift Left
        texture.ReadPixels(new Rect(1, 0, width - 1, height), 0, 0);
        texture.Apply();

        // 오른쪽 끝에 점 찍기
        texture.SetPixel(width - 1, y, Color.green);

        texture.Apply();
    }
}
