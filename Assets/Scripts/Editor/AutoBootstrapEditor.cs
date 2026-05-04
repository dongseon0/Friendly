#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AutoBootstrapEditor
{
    static AutoBootstrapEditor()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;

        // PersistentRoot가 이미 있으면 끝
        if (Object.FindFirstObjectByType<PersistentRoot>() != null) return;

        // Prefabs/[PersistentRoot].prefab 로드 (원하는 경로로 바꿔도 됨)
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/[PersistentRoot].prefab");
        if (prefab == null)
        {
            Debug.LogError("[AutoBootstrapEditor] Prefab not found at Assets/Prefabs/[PersistentRoot].prefab");
            return;
        }

        var go = Object.Instantiate(prefab);
        Object.DontDestroyOnLoad(go);
        Debug.Log("[AutoBootstrapEditor] Spawned PersistentRoot for editor play.");
    }
}
#endif