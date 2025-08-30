using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BugMoveController : MonoBehaviour
{
    float time = 0.0f;                              // 무당벌레 등장시간 변수

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        this.time += Time.deltaTime;                // 현재 시간 입력

        if (this.time > 5.0f)                       // 게임 시작 후 5초가 지났을 경우
        {
            if (transform.position.x < -7)          // 무당벌레 이동 경로
                transform.Translate(0.5f, 0, 0);
        }
    }
}
