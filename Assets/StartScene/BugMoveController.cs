using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BugMoveController : MonoBehaviour
{
    float time = 0.0f;                              // ������� ����ð� ����

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        this.time += Time.deltaTime;                // ���� �ð� �Է�

        if (this.time > 5.0f)                       // ���� ���� �� 5�ʰ� ������ ���
        {
            if (transform.position.x < -7)          // ������� �̵� ���
                transform.Translate(0.5f, 0, 0);
        }
    }
}
