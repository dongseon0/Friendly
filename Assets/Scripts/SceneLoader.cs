using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("이동할 씬 이름을 여기에 적으세요")]
    public string sceneName;

    public void LoadScene()
    {
        // 빈칸이 아니면 그 이름의 씬으로 이동
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("이동할 씬 이름을 입력하세요.");
        }
    }
}