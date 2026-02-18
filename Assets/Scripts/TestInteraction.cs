using UnityEngine;

public class TestInteraction : MonoBehaviour
{
    // 플레이어가 SceneLoader 영역 안에 있는지
    private bool isPlayerInRange = false;

    private void Update()
    {
        // 범위 안에 있고 Z키 누름
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Z키: " + GetComponent<SceneLoader>().sceneName);
            GetComponent<SceneLoader>().LoadScene();
        }
    }

    // 플레이어가 SceneLoader 영역 안
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("플레이어 진입" + isPlayerInRange);
        }
    }

    // 플레이어가 SceneLoader 영역 밖
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("플레이어 나감" + isPlayerInRange);
        }
    }
}