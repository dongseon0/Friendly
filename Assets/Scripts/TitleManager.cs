using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public string startSceneName;

    public void OnClickStartButton()
    {
        if (!string.IsNullOrEmpty(startSceneName))
        {
            SceneManager.LoadScene(startSceneName);
        }
    }

    public void OnClickExitButton()
    {
        Debug.Log("게임이 종료됨");
        Application.Quit();
    }
}
