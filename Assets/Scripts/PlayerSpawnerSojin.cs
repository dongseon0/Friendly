using UnityEngine;

public class PlayerSpawnerSojin : MonoBehaviour
{
    private GameObject _playerPrefab;
    private GameObject _playerInstance;

    public void Configure(GameObject playerPrefab)
    {
        _playerPrefab = playerPrefab;
    }
    public void SpawnOrMoveToSceneSpawn()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Player Prefab not assigned on PersistentRoot.");
            return;
        }
        // 1. 씬에 있는 '모든' 스폰 포인트를 다 찾고 allSpawns에 저장
        PlayerSpawnPoint[] allSpawns = Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
        PlayerSpawnPoint targetSpawn = null;

        if (allSpawns.Length > 0)
        {
            // 2. nextSpawnID와 이름이 똑같은 스폰 포인트를 찾고 targetSpawn으로 지정 
            if (!string.IsNullOrEmpty(SceneLoader.nextSpawnID))
            {
                foreach (var spawn in allSpawns)
                {
                    if (spawn.spawnID == SceneLoader.nextSpawnID)
                    {
                        targetSpawn = spawn;
                        break;
                    }
                }
            }

            // 3. targetSpawn이 없다면 그냥 첫 번째 걸 쓰기
            if (targetSpawn == null)
            {
                targetSpawn = allSpawns[0];
            }
        }

        if (_playerInstance == null)
        {
            _playerInstance = Object.Instantiate(_playerPrefab);
            Object.DontDestroyOnLoad(_playerInstance);
        }

        // 4. 찾은 스폰 포인트 위치로 플레이어를 순간이동
        if (targetSpawn != null)
        {
            _playerInstance.transform.SetPositionAndRotation(targetSpawn.transform.position, targetSpawn.transform.rotation);
        }
    }
}