using UnityEngine;

public class PlayerSpawner : MonoBehaviour
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

        var spawn = Object.FindFirstObjectByType<PlayerSpawnPoint>();
        if (spawn == null)
        {
            Debug.LogWarning("[PlayerSpawner] No PlayerSpawnPoint found in this scene.");
            if (_playerInstance == null)
            {
                _playerInstance = Object.Instantiate(_playerPrefab);
                Object.DontDestroyOnLoad(_playerInstance);
            }
            return;
        }

        if (_playerInstance == null)
        {
            _playerInstance = Object.Instantiate(_playerPrefab);
            Object.DontDestroyOnLoad(_playerInstance);
        }

        _playerInstance.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
    }
}
