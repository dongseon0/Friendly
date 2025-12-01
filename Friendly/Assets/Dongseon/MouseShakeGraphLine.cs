using UnityEngine;
using UnityEngine.UI;

// 마우스흔들림에 따라 파형 그래프가 그려지도록 하는 코드
public class MouseShakeGraphLine : MonoBehaviour
{
    public FearSignalReader reader;
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
        ClearTexture();
    }

    void Update()
    {
        ShiftLeft();
        DrawNewValue(reader.mouseShakeAmount);
        texture.SetPixels32(pixels);
        texture.Apply();
    }

    // 배경 검정 초기화
    void ClearTexture()
    {
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 255);  
    }

    void ShiftLeft()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                pixels[y * width + x] = pixels[y * width + (x + 1)];
            }
            pixels[y * width + (width - 1)] = new Color32(0, 0, 0, 255);
        }
    }

    void DrawNewValue(float shake)
    {
        float normalized = Mathf.InverseLerp(0f, 50f, shake);
        int y = Mathf.Clamp(Mathf.RoundToInt(normalized * (height - 1)), 0, height - 1);

        int thickness = 3; // ← 라인 두께 3픽셀

        for (int dy = -thickness; dy <= thickness; dy++)
        {
            int yy = Mathf.Clamp(y + dy, 0, height - 1);
            pixels[yy * width + (width - 1)] = new Color32(255, 255, 40, 255);  // 노란색
        }
    }

}