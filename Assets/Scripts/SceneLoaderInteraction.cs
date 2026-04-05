using UnityEngine;

public class SceneLoaderInteraction : MonoBehaviour, IInteractable
{
    private SceneLoader sceneLoader;

    void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
    }

    public void Interact()
    {

        if (sceneLoader != null)
        {
            Debug.Log("씬 전환:"+sceneLoader.sceneName);
            sceneLoader.LoadScene();
        }
    }

}