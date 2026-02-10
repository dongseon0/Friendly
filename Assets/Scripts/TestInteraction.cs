using UnityEngine;

public class TestInteraction : MonoBehaviour
{
    // 플레이어가 투명 박스 안에 들어와 있는 동안 계속 실행됨
    private void OnTriggerStay(Collider other)
    {
        // 들어온 게 플레이어이고 + Z키를 눌렀다면?
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.Z))
        {
            // 같은 오브젝트에 붙어있는 SceneLoader를 가져와서 실행!
            GetComponent<SceneLoader>().LoadScene();
        }
    }
}