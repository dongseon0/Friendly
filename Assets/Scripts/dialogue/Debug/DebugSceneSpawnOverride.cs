using UnityEngine;

public static class DebugSceneSpawnOverride
{
    private static string _targetUnitySceneName;
    private static string _spawnPointId;

    public static void Set(string unitySceneName, string spawnPointId)
    {
        _targetUnitySceneName = string.IsNullOrWhiteSpace(unitySceneName) ? null : unitySceneName.Trim();
        _spawnPointId = string.IsNullOrWhiteSpace(spawnPointId) ? null : spawnPointId.Trim();

        Debug.Log($"[DebugSpawnOverride] scene={_targetUnitySceneName}, spawn={_spawnPointId}");
    }

    public static string ConsumeIfMatches(string currentUnitySceneName)
    {
        if (string.IsNullOrEmpty(_targetUnitySceneName) || string.IsNullOrEmpty(_spawnPointId))
            return null;

        if (!string.Equals(_targetUnitySceneName, currentUnitySceneName, System.StringComparison.Ordinal))
            return null;

        string result = _spawnPointId;
        _targetUnitySceneName = null;
        _spawnPointId = null;
        return result;
    }

    public static void Clear()
    {
        _targetUnitySceneName = null;
        _spawnPointId = null;
    }
}