using System.Collections.Generic;
using UnityEngine;

public static class unlockingItem
{
    private static readonly HashSet<string> unlockedIds = new HashSet<string>();

    public static void Unlock(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;

        unlockedIds.Add(id);
        Debug.Log($"[unlockingItem] Unlock added: {id}");
    }

    public static bool IsUnlocked(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return true;
        // 비어 있으면 잠금 없는 문으로 취급

        return unlockedIds.Contains(id);
    }
}