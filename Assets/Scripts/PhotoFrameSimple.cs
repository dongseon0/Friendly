using UnityEngine;

public class PhotoFrameSimple : MonoBehaviour, IInteractable
{
    [Header("Photo Renderer")]
    [SerializeField] private Renderer photoRenderer;

    [Header("Textures")]
    [SerializeField] private Texture2D normalTexture;
    [SerializeField] private Texture2D horrorTexture;

    private bool changed = false;
    private Material runtimeMaterial;

    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        if (photoRenderer == null)
        {
            Debug.LogWarning($"{name}: photoRenderer가 연결되지 않았음.");
            return;
        }

        // Renderer.material 로 런타임 전용 머티리얼 인스턴스를 가져옴
        runtimeMaterial = photoRenderer.material;

        if (normalTexture != null)
        {
            SetTexture(normalTexture);
        }
    }

    public void Interact()
    {
        if (changed) return;

        changed = true;

        if (horrorTexture != null)
        {
            SetTexture(horrorTexture);
        }
    }

    private void SetTexture(Texture texture)
    {
        if (runtimeMaterial == null) return;

        if (runtimeMaterial.HasProperty(BaseMap))
            runtimeMaterial.SetTexture(BaseMap, texture);

        if (runtimeMaterial.HasProperty(MainTex))
            runtimeMaterial.SetTexture(MainTex, texture);
    }
}