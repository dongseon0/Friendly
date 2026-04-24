using UnityEngine;
using UnityEngine.SceneManagement;

public class CctvPcDvdInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string requiredTargetName = "My_PC 2 (4)";
    [SerializeField] private string targetUnitySceneName = "12_F1_Main";
    [SerializeField] private string targetSpawnId = "1F_CCTV";
    [SerializeField] private string storySceneId = "S07_CCTV_ROOM";
    [SerializeField] private string resumeNodeId = "S07_N2";

    private bool _used;

    public void Interact()
    {
        if (_used) return;

        dialog story = FindFirstObjectByType<dialog>(FindObjectsInactive.Include);

        if (story == null)
        {
            Debug.LogError("[CctvPcDvdInteractable] dialog not found.");
            return;
        }

        if (!story.IsWaitingForInteractionTarget(requiredTargetName))
        {
            Debug.Log("[CctvPcDvdInteractable] Story is not waiting for this PC yet.");
            return;
        }

        SceneLoader.nextSpawnID = targetSpawnId;

        GameObject runnerObj = new GameObject("CCTV_PC_DVD_TransitionRunner");
        DontDestroyOnLoad(runnerObj);

        var runner = runnerObj.AddComponent<CctvPcDvdTransitionRunner>();
        runner.Begin(targetUnitySceneName, storySceneId, resumeNodeId);

        _used = true;
    }

    private class CctvPcDvdTransitionRunner : MonoBehaviour
    {
        private string _targetUnitySceneName;
        private string _storySceneId;
        private string _resumeNodeId;

        public void Begin(string targetUnitySceneName, string storySceneId, string resumeNodeId)
        {
            _targetUnitySceneName = targetUnitySceneName;
            _storySceneId = storySceneId;
            _resumeNodeId = resumeNodeId;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(_targetUnitySceneName);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != _targetUnitySceneName) return;

            SceneManager.sceneLoaded -= OnSceneLoaded;

            dialog story = FindFirstObjectByType<dialog>(FindObjectsInactive.Include);

            if (story != null)
                story.StartScene(_storySceneId, _resumeNodeId);
            else
                Debug.LogError("[CctvPcDvdTransitionRunner] dialog not found after scene load.");

            Destroy(gameObject);
        }
    }
}