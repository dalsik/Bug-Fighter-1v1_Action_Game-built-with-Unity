using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AntMoveController : MonoBehaviour
{
    Animator animator;
    int x = 0;
    float time = 0.0f;                              // ���� ����ð� ����

    void Start()
    {
        Application.targetFrameRate = 60;
        this.animator = GetComponent<Animator>();                                           // ���� �ִϸ��̼� 
    }

    void Update()
    {
        this.time += Time.deltaTime;                // ���� �ð� �Է�

        if (this.time > 5.0f)                       // ���� ���� �� 5�ʰ� ������ ���
        {
            if (transform.position.x < -7.1f)                                               // ���� 1��° �̵� ���
                transform.Translate(0.2f, 0.2f, 0);

            else if (transform.position.x >= -7.1f && transform.position.x < -6.7f)         // ���� 2��° �̵� ���
            {
                transform.Translate(0.1f, -0.5f, 0);
                this.animator.SetTrigger("AttackTrigger");
            }

            else
            {
                if (this.time > 10 + x)                                                     // 3�ʸ��� ���� �׼�
                {
                    this.animator.SetTrigger("AttackTrigger");
                    x += 3;
                }
            }
        }
    }
}
