using UnityEngine;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (Object.FindFirstObjectByType<PersistentRoot>() != null) return;

        var prefab = Resources.Load<GameObject>("[PersistentRoot]");
        if (prefab == null)
        {
            Debug.LogError("[Bootstrapper] Resources/[PersistentRoot].prefab not found.");
            return;
        }

        var go = Object.Instantiate(prefab);
        Object.DontDestroyOnLoad(go);
    }
}
