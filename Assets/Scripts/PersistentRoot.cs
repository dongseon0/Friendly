using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentRoot : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string firstGameplaySceneName = "TitleScene";

    private PlayerSpawner _spawner;

    private void Awake()
    {
        // 중복 방지
        var roots = FindObjectsByType<PersistentRoot>(FindObjectsSortMode.None);
        if (roots.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); 

        _spawner = gameObject.AddComponent<PlayerSpawner>();
        _spawner.Configure(playerPrefab);

        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void Start()
    {
        // 빌드 인덱스 0 = BootstrapScene
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            SceneManager.LoadScene(firstGameplaySceneName);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Bootstrap(0번)과 Title에서는 스폰 스킵
        if (scene.buildIndex == 0) return;
        if (scene.name == "TitleScene") return;

        _spawner.SpawnOrMoveToSceneSpawn();
    }
}
