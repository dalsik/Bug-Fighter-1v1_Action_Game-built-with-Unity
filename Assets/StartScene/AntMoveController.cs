using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AntMoveController : MonoBehaviour
{
    Animator animator;
    int x = 0;
    float time = 0.0f;                              // 개미 등장시간 변수

    void Start()
    {
        Application.targetFrameRate = 60;
        this.animator = GetComponent<Animator>();                                           // 개미 애니메이션 
    }

    void Update()
    {
        this.time += Time.deltaTime;                // 현재 시간 입력

        if (this.time > 5.0f)                       // 게임 시작 후 5초가 지났을 경우
        {
            if (transform.position.x < -7.1f)                                               // 개미 1번째 이동 경로
                transform.Translate(0.2f, 0.2f, 0);

            else if (transform.position.x >= -7.1f && transform.position.x < -6.7f)         // 개미 2번째 이동 경로
            {
                transform.Translate(0.1f, -0.5f, 0);
                this.animator.SetTrigger("AttackTrigger");
            }

            else
            {
                if (this.time > 10 + x)                                                     // 3초마다 개미 액션
                {
                    this.animator.SetTrigger("AttackTrigger");
                    x += 3;
                }
            }
        }
    }
}
