using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IInteractable
{
    void Interact();
}

#if UNITY_EDITOR
[InitializeOnLoad]
public static class IInteractableOutlineAutoSetup
{
    private const float OUTLINE_WIDTH = 5f;

    static IInteractableOutlineAutoSetup()
    {
        EditorApplication.delayCall += ApplyToAllInteractables;
        EditorApplication.hierarchyChanged += ApplyToAllInteractables;
    }

    private static void ApplyToAllInteractables()
    {
        if (Application.isPlaying) return;

        MonoBehaviour[] behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null) continue;
            if (EditorUtility.IsPersistent(behaviour.gameObject)) continue;

            if (behaviour is IInteractable)
            {
                SetupOutline(behaviour.gameObject);
            }
        }
    }

    private static void SetupOutline(GameObject target)
    {
        Outline outline = target.GetComponent<Outline>();

        if (outline == null)
        {
            outline = Undo.AddComponent<Outline>(target);
        }

        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = OUTLINE_WIDTH;

        // 컴포넌트 체크해제
        outline.enabled = false;

        EditorUtility.SetDirty(outline);
        EditorUtility.SetDirty(target);
    }
}
#endif