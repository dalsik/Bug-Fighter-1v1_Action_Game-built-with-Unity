using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StingerController : MonoBehaviour
{
    public float knockbackForce = 5.0f; // �˹� ����

    void OnTriggerEnter2D(Collider2D collision)
    {
        // ���� �±� Ȯ��
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            if (collision.tag != gameObject.tag) // �ڽŰ� �ٸ� �±׸� ó��
            {
                // ������ ó��
                GameObject director = GameObject.Find("GameDirector");
                if (director != null)
                {
                    GameDirector gameDirector = director.GetComponent<GameDirector>();
                    if (gameDirector != null)
                    {
                        if (collision.CompareTag("Player1"))
                        {
                            gameDirector.DecreaseHP1(0.025f); // ������ ����
                        }
                        else if (collision.CompareTag("Player2"))
                        {
                            gameDirector.DecreaseHP2(0.025f); // ������ ����
                        }
                    }
                }

                // �˹� ȿ��
                Rigidbody2D targetRigidBody = collision.GetComponent<Rigidbody2D>();
                if (targetRigidBody != null)
                {
                    float direction = (collision.transform.position.x - transform.position.x) > 0 ? 1 : -1;
                    ApplyKnockback(targetRigidBody, direction);
                }

                // �߻�ü ����
                Destroy(gameObject);
            }
        }
    }

    void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized; // x��� �ణ�� y������ �и�
        targetRigidBody.velocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }
}
