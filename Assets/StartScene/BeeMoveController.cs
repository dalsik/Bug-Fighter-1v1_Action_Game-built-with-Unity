using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeeMoveController : MonoBehaviour
{
    Animator animator;
    int x = 0;
    float time = 0.0f;                              // �� ����ð� ����

    void Start()
    {
        Application.targetFrameRate = 60;
        this.animator = GetComponent<Animator>();                                       // �� �ִϸ��̼� 
    }

    void Update()
    {
        this.time += Time.deltaTime;                // ���� �ð� �Է�

        if (this.time > 5.0f)                       // ���� ���� �� 5�ʰ� ������ ���
        {
            if (transform.position.x > 5.5)                                               // �� �̵� ���
            {
                transform.Translate(-0.5f, 0, 0);
                this.animator.SetTrigger("AAttackTrigger");
            }

            else
            {
                this.animator.speed = 1.5f;

                if (this.time > 10 + x)                                                     // 3�ʸ��� �� �׼�
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