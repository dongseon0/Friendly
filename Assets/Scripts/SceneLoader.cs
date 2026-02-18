using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string sceneName;

    [Header("스폰 지점의 ID")]
    public string targetSpawnID;

    //씬이 넘어가도 지워지지 않는 공용 메모지
    public static string nextSpawnID = "";

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // 씬을 넘어가기 직전에 도착할 ID를 nextSpawnID에 
            nextSpawnID = targetSpawnID;
            SceneManager.LoadScene(sceneName);
        }
    }
}