using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private bool hasCollided = false; // 충돌 여부를 확인하는 변수
    private float destroyDelay = 3.0f; // 레이저가 유지되는 시간 (초)
    public float knockbackForce = 10.0f; // 넉백의 강도
    public float knockbackDuration = 0.2f; // 넉백 지속 시간
    void Start()
    {
        // 필요 시 초기 설정 가능
    }

    void Update()
    {
        // 필요 시 매 프레임 처리 가능
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 충돌 처리된 경우 더 이상 처리하지 않음
        if (hasCollided)
            return;
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        // 충돌한 대상이 상대 플레이어인지 태그로 확인
        if (collision.CompareTag("Player2")) // 상대 캐릭터에 "Player2" 태그 설정
        {
            GameObject director = GameObject.Find("GameDirector");
            director.GetComponent<GameDirector>().DecreaseHP2(0.2f); // 상대 캐릭터의 HP 감소 처리
            ApplyKnockback(collision.gameObject, direction); // 넉백 효과 추가 (-1은 왼쪽 방향)
            HandleCollisionEffect(); // 충돌 효과 처리
        }
        else if (collision.CompareTag("Player1")) // 상대 캐릭터에 "Player1" 태그 설정
        {
            GameObject director = GameObject.Find("GameDirector");
            director.GetComponent<GameDirector>().DecreaseHP1(0.2f); // 상대 캐릭터의 HP 감소 처리
            ApplyKnockback(collision.gameObject, direction); // 넉백 효과 추가 (-1은 왼쪽 방향)
            HandleCollisionEffect(); // 충돌 효과 처리
        }
    }

    private void HandleCollisionEffect()
    {
        // 충돌 처리 후 일정 시간 동안 유지
        hasCollided = true; // 충돌 상태 설정
        StartCoroutine(DestroyAfterDelay()); // 일정 시간 후 레이저 삭제
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay); // 3초 대기
        Destroy(gameObject); // 레이저 삭제
    }

    void ApplyKnockback(GameObject target, float direction)
    {
        Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();

        if (targetRigidBody != null)
        {
            // 넉백 방향 설정 (약간 위로 밀리는 효과 포함)
            Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;

            // 기존 속도 초기화 후 넉백 적용
            targetRigidBody.velocity = Vector2.zero;
            targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            // 넉백 중 플레이어 컨트롤 제한
            StartCoroutine(DisableControlForDuration(target, knockbackDuration));
        }
    }

    IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        PlayerController controller = target.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.enabled = false; // 플레이어 컨트롤 비활성화
            yield return new WaitForSeconds(duration);
            controller.enabled = true; // 플레이어 컨트롤 재활성화
        }
    }

}
