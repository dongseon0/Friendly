using System.Collections;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    // Materials
    public Material redLightMaterial;   // Red Light 1
    public Material noLightMaterial;    // No Light 2

    // ½ÇÁ¦ Á¶¸í
    public Light dimPointLight;

    // ±ôºýÀÓ ¼Óµµ
    public float minDelay = 0.05f;
    public float maxDelay = 0.3f;

    void Start()
    {
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // ·£´ý ¼±ÅÃ (0 or 1)
            bool isOn = Random.value > 0.7f;

            if (isOn)
            {
                meshRenderer.materials[0] = redLightMaterial;
                dimPointLight.intensity = 1.5f;
            }
            else
            {
                meshRenderer.materials[0] = noLightMaterial;
                dimPointLight.intensity = 0f;
            }

            // ·£´ý µô·¹ÀÌ
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
        }
    }
}