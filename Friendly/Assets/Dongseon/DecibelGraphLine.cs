using UnityEngine;
using UnityEngine.UI;

// 데시벨에 따라 파형 그래프가 그려지도록 하는 코드
public class DecibelGraphLine : MonoBehaviour
{
    public FearSignalReader mic;
    public RawImage graphImage;

    Texture2D texture;
    Color32[] pixels;

    int width = 300;
    int height = 100;

    void Start()
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        graphImage.texture = texture;

        pixels = new Color32[width * height];

        // 처음 초기화
        ClearTexture();
    }

    void Update()
    {
        ShiftLeft();
        DrawNewValue(mic.currentDB);
        texture.SetPixels32(pixels);
        texture.Apply();
    }

    // 배경 검정 초기화
    void ClearTexture()
    {
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 255);
    }

    // 그래프 한 칸씩 왼쪽으로 밀기
    void ShiftLeft()
    {
        // (y * width + x) 형태로 저장
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                pixels[y * width + x] = pixels[y * width + (x + 1)];
            }

            // 마지막 칸 비우기
            pixels[y * width + (width - 1)] = new Color32(0, 0, 0, 255);
        }
    }

    void DrawNewValue(float db)
    {
        // 데시벨을 픽셀 높이로 변환 (0~100)
        float normalized = Mathf.InverseLerp(-60f, 0f, db);
        int y = Mathf.Clamp(Mathf.RoundToInt(normalized * (height - 1)), 0, height - 1);

        // 오른쪽 끝에 초록 점 찍기
        pixels[y * width + (width - 1)] = new Color32(0, 255, 0, 255);
    }
}
