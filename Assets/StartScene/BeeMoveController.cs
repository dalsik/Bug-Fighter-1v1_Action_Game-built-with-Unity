using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeeMoveController : MonoBehaviour
{
    Animator animator;
    int x = 0;
    float time = 0.0f;                              // 벌 등장시간 변수

    void Start()
    {
        Application.targetFrameRate = 60;
        this.animator = GetComponent<Animator>();                                       // 벌 애니메이션 
    }

    void Update()
    {
        this.time += Time.deltaTime;                // 현재 시간 입력

        if (this.time > 5.0f)                       // 게임 시작 후 5초가 지났을 경우
        {
            if (transform.position.x > 5.5)                                               // 벌 이동 경로
            {
                transform.Translate(-0.5f, 0, 0);
                this.animator.SetTrigger("AAttackTrigger");
            }

            else
            {
                this.animator.speed = 1.5f;

                if (this.time > 10 + x)                                                     // 3초마다 벌 액션
                {
                    this.animator.SetTrigger("SpitTrigger");
                    x += 3;
                }

                else
                    this.animator.SetTrigger("WalkTrigger");
            }
        }
    }
}