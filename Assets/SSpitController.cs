using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSpitController : MonoBehaviour
{
    private bool isCollided = false; // 충돌 여부 확인용 플래그
    public float knockbackForce = 8.0f; // 넉백의 강도
    public float knockbackDuration = 0.3f; // 넉백 지속 시간

    void Update()
    {
        // 충돌한 후에도 오브젝트는 계속 보여야 함
        if (isCollided)
        {
            // 충돌 시 시각적 효과를 추가하거나 업데이트 가능
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        // 충돌한 대상이 상대 플레이어인지 태그로 확인
        if (collision.CompareTag("Player2")) // 상대 캐릭터에 "Player2" 태그 설정
        {
            HandleCollision("Player2", collision, direction); // -1은 왼쪽 방향 넉백
        }
        else if (collision.CompareTag("Player1")) // 상대 캐릭터에 "Player1" 태그 설정
        {
            HandleCollision("Player1", collision, direction); // 1은 오른쪽 방향 넉백
        }
    }

    private void HandleCollision(string playerTag, Collider2D collision, float direction)
    {
        // GameDirector를 찾아서 데미지 처리
        GameObject director = GameObject.Find("GameDirector");
        if (director != null)
        {
            GameDirector gameDirector = director.GetComponent<GameDirector>();
            if (gameDirector != null)
            {
                if (playerTag == "Player2")
                {
                    gameDirector.DecreaseHP2(0.05f); // Player2 HP 감소
                }
                else if (playerTag == "Player1")
                {
                    gameDirector.DecreaseHP1(0.05f); // Player1 HP 감소
                }
            }
        }

        // 충돌 플래그 활성화
        isCollided = true;
        // 넉백 효과 추가
        Rigidbody2D targetRigidBody = collision.GetComponent<Rigidbody2D>();
        if (targetRigidBody != null)
        {
            ApplyKnockback(targetRigidBody, direction);
        }

        // 3초 후 오브젝트 삭제
        StartCoroutine(DestroyAfterDelay());
    }

    private void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        // 넉백 방향 결정 (약간 위로 밀리는 효과 포함)
        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;

        // 기존 속도 초기화 후 넉백 적용
        targetRigidBody.velocity = Vector2.zero;
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // 넉백 중 플레이어 컨트롤 제한
        StartCoroutine(DisableControlForDuration(targetRigidBody.gameObject, knockbackDuration));
    }

    private IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        PlayerController controller = target.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.enabled = false; // 플레이어 컨트롤 비활성화
            yield return new WaitForSeconds(duration);
            controller.enabled = true; // 플레이어 컨트롤 재활성화
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(3.0f); // 3초 기다림
        Destroy(gameObject); // 오브젝트 삭제
    }
}
