using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.Collections;

public class NPCStoryLoader : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private dialog story;
    [SerializeField] private GameObject npcPrefab;

    [Header("Scene Names")]
    [SerializeField] private string outdoorSceneName = "OutdoorScene";
    [SerializeField] private string ossuarySceneName = "OssuaryIndoorScene";

    [Header("Marker Names")]
    [SerializeField] private string outdoorLocation1Name = "NPC_Location1";
    [SerializeField] private string outdoorLocation2Name = "NPC_Location2";
    [SerializeField] private string ossuaryLocation3Name = "NPC_Location3";
    [SerializeField] private string ossuaryLocation4Name = "NPC_Location4";

    [Header("Story Flags")]
    [SerializeField] private string outdoorMoveFlag = "npc_move_outdoor";
    [SerializeField] private string ossuaryMoveFlag = "npc_move_ossuary";

    private GameObject npcInstance;
    private NavMeshAgent agent;

    private Animator animator;

    private bool outdoorMoveStarted = false;
    private bool outdoorMoveFinished = false;

    private bool ossuaryMoveStarted = false;
    private bool ossuaryMoveFinished = false;

    private bool outdoorScenePhaseEnded = false; // outdoor 복귀 시 다시 안 나오게
    private bool initialized = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (story == null)
            story = FindFirstObjectByType<dialog>();

        CreateNpcIfNeeded();
        HideNpcImmediate();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (npcInstance == null || agent == null) return;

        // animation
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }

        if (story == null)
            story = FindFirstObjectByType<dialog>();

        if (story == null || npcInstance == null)
            return;

        string currentScene = SceneManager.GetActiveScene().name;

        // Move NPC in Outdoor after S00_N4 and hide after arrival
        if (!outdoorMoveStarted && !outdoorMoveFinished && story.IsFlagTrue(outdoorMoveFlag))
        {
            if (currentScene == outdoorSceneName)
            {
                Transform loc2 = FindMarker(outdoorLocation2Name);
                if (loc2 != null)
                {
                    outdoorMoveStarted = true;
                    StartCoroutine(MoveNpcAndHide(loc2, () =>
                    {
                        outdoorMoveFinished = true;
                        outdoorScenePhaseEnded = true;
                    }));
                }
            }
        }

        // move NPC in Ossuary after S01_N5 and hide after arrival
        if (!ossuaryMoveStarted && !ossuaryMoveFinished && story.IsFlagTrue(ossuaryMoveFlag))
        {
            if (currentScene == ossuarySceneName)
            {
                Transform loc4 = FindMarker(ossuaryLocation4Name);
                if (loc4 != null)
                {
                    ossuaryMoveStarted = true;
                    StartCoroutine(MoveNpcAndHide(loc4, () =>
                    {
                        ossuaryMoveFinished = true;
                    }));
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (story == null)
            story = FindFirstObjectByType<dialog>();

        CreateNpcIfNeeded();

        // Enter Outdoor Scene at first, show NPC at location1
        if (scene.name == outdoorSceneName)
        {
            // Don't show NPC if outdoor phase already ended or moved once (복귀 시 안 보이게)
            if (outdoorScenePhaseEnded || outdoorMoveFinished)
            {
                HideNpcImmediate();
                return;
            }

            Transform loc1 = FindMarker(outdoorLocation1Name);
            if (loc1 != null)
            {
                PlaceNpcAt(loc1);
                ShowNpc();
                initialized = true;
            }
            else
            {
                Debug.LogWarning("[NPCstoryloader] Outdoor location1 not found.");
                HideNpcImmediate();
            }
        }
        // Enter Ossuary Scene, show NPC at location3 if not moved yet
        else if (scene.name == ossuarySceneName)
        {
            // Don't show NPC if ossuary phase already ended
            if (ossuaryMoveFinished)
            {
                HideNpcImmediate();
                return;
            }

            Transform loc3 = FindMarker(ossuaryLocation3Name);
            if (loc3 != null)
            {
                PlaceNpcAt(loc3);
                ShowNpc();
            }
            else
            {
                Debug.LogWarning("[NPCstoryloader] Ossuary location3 not found.");
                HideNpcImmediate();
            }
        }
        else
        {
            // Hide NPC in any other scene
            HideNpcImmediate();
        }
    }

    private void CreateNpcIfNeeded()
    {
        if (npcInstance != null) return;

        npcInstance = Instantiate(npcPrefab);
        npcInstance.name = npcPrefab.name + "_PersistentNPC";
        DontDestroyOnLoad(npcInstance);

        agent = npcInstance.GetComponent<NavMeshAgent>();
        animator = npcInstance.GetComponent<Animator>();

        if (agent == null)
            Debug.LogError("[NPCstoryloader] Need NavMeshAgent");

        if (animator == null)
            Debug.LogWarning("[NPCstoryloader] No Animator (animations will not play)");
    }

    private Transform FindMarker(string markerName)
    {
        GameObject go = GameObject.Find(markerName);
        return go != null ? go.transform : null;
    }

    private void PlaceNpcAt(Transform marker)
    {
        if (npcInstance == null || marker == null) return;

        if (agent != null)
        {
            agent.enabled = false;
        }

        npcInstance.transform.SetPositionAndRotation(marker.position, marker.rotation);

        if (agent != null)
        {
            agent.enabled = true;
            agent.ResetPath();
        }
    }

    private void ShowNpc()
    {
        if (npcInstance != null && !npcInstance.activeSelf)
            npcInstance.SetActive(true);
    }

    private void HideNpcImmediate()
    {
        if (npcInstance != null && npcInstance.activeSelf)
            npcInstance.SetActive(false);
    }

    private IEnumerator MoveNpcAndHide(Transform target, System.Action onArrived)
    {
        if (npcInstance == null || target == null)
            yield break;

        ShowNpc();

        if (agent == null)
        {
            Debug.LogError("[NPCstoryloader] NavMeshAgent missing.");
            yield break;
        }

        if (!agent.enabled)
            agent.enabled = true;

        agent.isStopped = false;
        agent.SetDestination(target.position);

        while (true)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                        break;
                }
            }

            yield return null;
        }

        agent.ResetPath();
        HideNpcImmediate();
        onArrived?.Invoke();
    }
}