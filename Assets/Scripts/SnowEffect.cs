using UnityEngine;

public class SnowEffect: MonoBehaviour
{
    private Transform targetPlayer;

    // 플레이어 머리 위 10m 높이에서 눈을 뿌리도록 설정
    public Vector3 offset = new Vector3(0, 5f, 0);

    void LateUpdate()
    {
        // 1. 플레이어가 아직 씬에 없다면 계속 찾기 (DDOL 대비)
        if (targetPlayer == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                targetPlayer = player.transform;
            }
            return;
        }

        // 2. 플레이어를 찾았다면 머리 위를 졸졸 따라다니기
        transform.position = targetPlayer.position + offset;
    }
}