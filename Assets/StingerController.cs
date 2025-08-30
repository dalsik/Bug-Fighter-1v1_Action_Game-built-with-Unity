using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StingerController : MonoBehaviour
{
    public float knockbackForce = 5.0f; // 넉백 강도

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 상대방 태그 확인
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            if (collision.tag != gameObject.tag) // 자신과 다른 태그만 처리
            {
                // 데미지 처리
                GameObject director = GameObject.Find("GameDirector");
                if (director != null)
                {
                    GameDirector gameDirector = director.GetComponent<GameDirector>();
                    if (gameDirector != null)
                    {
                        if (collision.CompareTag("Player1"))
                        {
                            gameDirector.DecreaseHP1(0.025f); // 데미지 적용
                        }
                        else if (collision.CompareTag("Player2"))
                        {
                            gameDirector.DecreaseHP2(0.025f); // 데미지 적용
                        }
                    }
                }

                // 넉백 효과
                Rigidbody2D targetRigidBody = collision.GetComponent<Rigidbody2D>();
                if (targetRigidBody != null)
                {
                    float direction = (collision.transform.position.x - transform.position.x) > 0 ? 1 : -1;
                    ApplyKnockback(targetRigidBody, direction);
                }

                // 발사체 삭제
                Destroy(gameObject);
            }
        }
    }

    void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized; // x축과 약간의 y축으로 밀림
        targetRigidBody.velocity = Vector2.zero; // 기존 속도 초기화
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }
}
